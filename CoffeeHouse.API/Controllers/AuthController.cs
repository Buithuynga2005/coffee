using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using CoffeeHouse.API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeHouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // =========================================================
        // REGISTER
        // POST: /api/Auth/register
        // =========================================================

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                // =================================================
                // KIỂM TRA INPUT
                // =================================================

                if (request == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu đăng ký không hợp lệ."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập họ tên."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Phone))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập số điện thoại."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập email."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập tên đăng nhập."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập mật khẩu."
                    });
                }

                if (request.Password.Length < 6)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mật khẩu phải có ít nhất 6 ký tự."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Role))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn chức vụ."
                    });
                }

                if (request.BranchId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn chi nhánh."
                    });
                }

                // =================================================
                // CONNECTION STRING
                // =================================================

                string? connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Không tìm thấy DefaultConnection trong appsettings.json."
                    });
                }

                using var connection =
                    new MySqlConnection(connectionString);

                connection.Open();

                // =================================================
                // CHUẨN HÓA DỮ LIỆU
                // =================================================

                string username =
                    request.Username.Trim();

                string email =
                    request.Email.Trim();

                string phone =
                    request.Phone.Trim();

                string roleCode =
                    request.Role.Trim().ToUpper();

                // =================================================
                // KIỂM TRA USERNAME
                // =================================================

                const string checkUsernameSql = @"
                    SELECT COUNT(*)
                    FROM `user`
                    WHERE username = @username;
                ";

                using (var cmd =
                    new MySqlCommand(
                        checkUsernameSql,
                        connection))
                {
                    cmd.Parameters.AddWithValue(
                        "@username",
                        username);

                    long count =
                        Convert.ToInt64(
                            cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = "Tên đăng nhập đã tồn tại."
                        });
                    }
                }

                // =================================================
                // KIỂM TRA EMAIL
                // =================================================

                const string checkEmailSql = @"
                    SELECT COUNT(*)
                    FROM `user`
                    WHERE email = @email;
                ";

                using (var cmd =
                    new MySqlCommand(
                        checkEmailSql,
                        connection))
                {
                    cmd.Parameters.AddWithValue(
                        "@email",
                        email);

                    long count =
                        Convert.ToInt64(
                            cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = "Email đã được sử dụng."
                        });
                    }
                }

                // =================================================
                // KIỂM TRA CHI NHÁNH
                // =================================================

                const string checkBranchSql = @"
                    SELECT COUNT(*)
                    FROM branch
                    WHERE branch_id = @branch_id;
                ";

                using (var cmd =
                    new MySqlCommand(
                        checkBranchSql,
                        connection))
                {
                    cmd.Parameters.AddWithValue(
                        "@branch_id",
                        request.BranchId);

                    long branchCount =
                        Convert.ToInt64(
                            cmd.ExecuteScalar());

                    if (branchCount == 0)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                $"Không tìm thấy chi nhánh có branch_id = {request.BranchId}."
                        });
                    }
                }

                // =================================================
                // TÌM ROLE
                // =================================================

                long roleId;

                const string roleSql = @"
                    SELECT role_id
                    FROM role
                    WHERE UPPER(role_code) = @role_code
                    LIMIT 1;
                ";

                using (var cmd =
                    new MySqlCommand(
                        roleSql,
                        connection))
                {
                    cmd.Parameters.AddWithValue(
                        "@role_code",
                        roleCode);

                    object? result =
                        cmd.ExecuteScalar();

                    if (result == null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                $"Chức vụ '{roleCode}' không tồn tại trong bảng role."
                        });
                    }

                    roleId =
                        Convert.ToInt64(result);
                }

                // =================================================
                // ROLE → POSITION
                // =================================================

                string employeePosition =
                    roleCode switch
                    {
                        "ADMIN" => "Giám đốc",
                        "MANAGER" => "Quản lý",
                        "FINANCE" => "Tài chính",
                        "POS_STAFF" => "Thu ngân",
                        "BARISTA" => "Pha chế",
                        _ => "Nhân viên"
                    };

                // =================================================
                // TRANSACTION
                // =================================================

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    // =================================================
                    // TẠO EMPLOYEE
                    // =================================================

                    long employeeId;

                    const string employeeSql = @"
                        INSERT INTO employee
                        (
                            branch_id,
                            full_name,
                            phone,
                            email,
                            position,
                            hire_date,
                            status
                        )
                        VALUES
                        (
                            @branch_id,
                            @full_name,
                            @phone,
                            @email,
                            @position,
                            CURRENT_DATE,
                            'ACTIVE'
                        );

                        SELECT LAST_INSERT_ID();
                    ";

                    using (var cmd =
                        new MySqlCommand(
                            employeeSql,
                            connection,
                            transaction))
                    {
                        cmd.Parameters.AddWithValue(
                            "@branch_id",
                            request.BranchId);

                        cmd.Parameters.AddWithValue(
                            "@full_name",
                            request.FullName.Trim());

                        cmd.Parameters.AddWithValue(
                            "@phone",
                            phone);

                        cmd.Parameters.AddWithValue(
                            "@email",
                            email);

                        cmd.Parameters.AddWithValue(
                            "@position",
                            employeePosition);

                        employeeId =
                            Convert.ToInt64(
                                cmd.ExecuteScalar());
                    }

                    // =================================================
                    // HASH PASSWORD
                    // =================================================

                    string passwordHash =
                        HashPassword(request.Password);

                    // =================================================
                    // TẠO USER
                    // =================================================

                    const string userSql = @"
                        INSERT INTO `user`
                        (
                            employee_id,
                            role_id,
                            username,
                            password_hash,
                            email,
                            status
                        )
                        VALUES
                        (
                            @employee_id,
                            @role_id,
                            @username,
                            @password_hash,
                            @email,
                            'ACTIVE'
                        );
                    ";

                    using (var cmd =
                        new MySqlCommand(
                            userSql,
                            connection,
                            transaction))
                    {
                        cmd.Parameters.AddWithValue(
                            "@employee_id",
                            employeeId);

                        cmd.Parameters.AddWithValue(
                            "@role_id",
                            roleId);

                        cmd.Parameters.AddWithValue(
                            "@username",
                            username);

                        cmd.Parameters.AddWithValue(
                            "@password_hash",
                            passwordHash);

                        cmd.Parameters.AddWithValue(
                            "@email",
                            email);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    // =================================================
                    // RESPONSE
                    // =================================================

                    return Ok(new
                    {
                        success = true,
                        message =
                            "Đăng ký tài khoản thành công.",
                        employeeId = employeeId,
                        username = username,
                        role = roleCode
                    });
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi MySQL.",
                    detail = ex.Message,
                    errorCode = ex.Number
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống.",
                    detail = ex.Message
                });
            }
        }


        // =========================================================
        // LOGIN
        // POST: /api/Auth/login
        // =========================================================

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // =================================================
                // KIỂM TRA INPUT
                // =================================================

                if (request == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Dữ liệu đăng nhập không hợp lệ."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Vui lòng nhập tên đăng nhập."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Vui lòng nhập mật khẩu."
                    });
                }

                // =================================================
                // CONNECTION STRING
                // =================================================

                string? connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Không tìm thấy DefaultConnection trong appsettings.json."
                    });
                }

                using var connection =
                    new MySqlConnection(connectionString);

                connection.Open();

                // =================================================
                // LOGIN
                // USERNAME / EMAIL / PHONE
                // =================================================

                const string sql = @"
                    SELECT
                        u.user_id,
                        u.employee_id,
                        u.username,
                        u.password_hash,
                        u.email,
                        u.status,
                        r.role_code,
                        e.full_name,
                        e.phone,
                        e.position,
                        e.branch_id
                    FROM `user` u

                    LEFT JOIN role r
                        ON u.role_id = r.role_id

                    LEFT JOIN employee e
                        ON u.employee_id = e.employee_id

                    WHERE
                        u.username = @username
                        OR u.email = @username
                        OR e.phone = @username

                    LIMIT 1;
                ";

                using var cmd =
                    new MySqlCommand(
                        sql,
                        connection);

                cmd.Parameters.AddWithValue(
                    "@username",
                    request.Username.Trim());

                using var reader =
                    cmd.ExecuteReader();

                // =================================================
                // KHÔNG TÌM THẤY
                // =================================================

                if (!reader.Read())
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message =
                            "Tên đăng nhập, email hoặc số điện thoại không tồn tại."
                    });
                }

                // =================================================
                // PASSWORD
                // =================================================

                string storedPasswordHash =
                    reader["password_hash"]?.ToString()
                    ?? "";

                bool passwordCorrect =
                    VerifyPassword(
                        request.Password,
                        storedPasswordHash);

                if (!passwordCorrect)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message =
                            "Mật khẩu không chính xác."
                    });
                }

                // =================================================
                // STATUS
                // =================================================

                string status =
                    reader["status"]?.ToString()
                    ?? "";

                if (!status.Equals(
                        "ACTIVE",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message =
                            "Tài khoản đã bị khóa hoặc không hoạt động."
                    });
                }

                // =================================================
                // LẤY THÔNG TIN USER
                // =================================================

                long userId =
                    Convert.ToInt64(
                        reader["user_id"]);

                long? employeeId =
                    reader["employee_id"] == DBNull.Value
                        ? null
                        : Convert.ToInt64(
                            reader["employee_id"]);

                string username =
                    reader["username"]?.ToString()
                    ?? "";

                string email =
                    reader["email"]?.ToString()
                    ?? "";

                string role =
                    reader["role_code"]?.ToString()
                    ?? "";

                string fullName =
                    reader["full_name"]?.ToString()
                    ?? "";

                string phone =
                    reader["phone"]?.ToString()
                    ?? "";

                string position =
                    reader["position"]?.ToString()
                    ?? "";

                long? branchId =
                    reader["branch_id"] == DBNull.Value
                        ? null
                        : Convert.ToInt64(
                            reader["branch_id"]);

                // =================================================
                // KIỂM TRA ROLE
                // =================================================

                role =
                    role.Trim().ToUpper();

                if (string.IsNullOrWhiteSpace(role))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Tài khoản chưa được gán chức vụ."
                    });
                }

                // =================================================
                // JWT
                // =================================================

                string token =
                    GenerateJwtToken(
                        userId,
                        username,
                        role,
                        employeeId);

                // =================================================
                // RESPONSE
                // =================================================

                return Ok(new
                {
                    success = true,
                    message =
                        "Đăng nhập thành công.",

                    token = token,

                    user = new
                    {
                        userId,
                        employeeId,
                        username,
                        email,
                        fullName,
                        phone,
                        position,
                        branchId,
                        role
                    }
                });
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi MySQL.",
                    detail = ex.Message,
                    errorCode = ex.Number
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống.",
                    detail = ex.Message
                });
            }
        }


        // =========================================================
        // CHANGE PASSWORD
        // POST: /api/Auth/change-password
        // =========================================================

        [HttpPost("change-password")]
        public IActionResult ChangePassword(
            [FromBody] ChangePasswordRequest request)
        {
            try
            {
                // =================================================
                // KIỂM TRA REQUEST
                // =================================================

                if (request == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Dữ liệu yêu cầu không hợp lệ."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    request.Username))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Username không được để trống."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    request.NewPassword))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Mật khẩu mới không được để trống."
                    });
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Mật khẩu mới phải có ít nhất 6 ký tự."
                    });
                }

                // =================================================
                // CONNECTION STRING
                // =================================================

                string? connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection");

                if (string.IsNullOrWhiteSpace(
                    connectionString))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Không tìm thấy DefaultConnection trong appsettings.json."
                    });
                }

                using var connection =
                    new MySqlConnection(
                        connectionString);

                connection.Open();

                // =================================================
                // TÌM USER
                // =================================================

                const string selectSql = @"
                    SELECT
                        user_id,
                        status
                    FROM `user`
                    WHERE username = @username
                    LIMIT 1;
                ";

                long userId;
                string status;

                using (var cmd =
                    new MySqlCommand(
                        selectSql,
                        connection))
                {
                    cmd.Parameters.AddWithValue(
                        "@username",
                        request.Username.Trim());

                    using var reader =
                        cmd.ExecuteReader();

                    if (!reader.Read())
                    {
                        return NotFound(new
                        {
                            success = false,
                            message =
                                "Không tìm thấy tài khoản."
                        });
                    }

                    userId =
                        Convert.ToInt64(
                            reader["user_id"]);

                    status =
                        reader["status"]?.ToString()
                        ?? "";
                }

                // =================================================
                // KIỂM TRA STATUS
                // =================================================

                if (!status.Equals(
                        "ACTIVE",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Tài khoản không ở trạng thái ACTIVE."
                    });
                }

                // =================================================
                // HASH PASSWORD MỚI
                // =================================================

                string newPasswordHash =
                    HashPassword(
                        request.NewPassword);

                // =================================================
                // UPDATE PASSWORD
                // =================================================

                const string updateSql = @"
                    UPDATE `user`
                    SET
                        password_hash = @password_hash,
                        updated_at = CURRENT_TIMESTAMP
                    WHERE user_id = @user_id;
                ";

                int affectedRows;

                using (var cmd =
                    new MySqlCommand(
                        updateSql,
                        connection))
                {
                    cmd.Parameters.AddWithValue(
                        "@password_hash",
                        newPasswordHash);

                    cmd.Parameters.AddWithValue(
                        "@user_id",
                        userId);

                    affectedRows =
                        cmd.ExecuteNonQuery();
                }

                // =================================================
                // KIỂM TRA UPDATE
                // =================================================

                if (affectedRows == 0)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Không thể cập nhật mật khẩu."
                    });
                }

                // =================================================
                // SUCCESS
                // =================================================

                return Ok(new
                {
                    success = true,
                    message =
                        "Đổi mật khẩu thành công.",
                    username =
                        request.Username.Trim()
                });
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi MySQL.",
                    detail = ex.Message,
                    errorCode = ex.Number
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống.",
                    detail = ex.Message
                });
            }
        }


        // =========================================================
        // HASH PASSWORD
        // =========================================================

        private static string HashPassword(
            string password)
        {
            byte[] salt =
                RandomNumberGenerator.GetBytes(16);

            byte[] hash =
                Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    100000,
                    HashAlgorithmName.SHA256,
                    32
                );

            return
                Convert.ToBase64String(salt)
                + "."
                + Convert.ToBase64String(hash);
        }


        // =========================================================
        // VERIFY PASSWORD
        // =========================================================

        private static bool VerifyPassword(
            string password,
            string storedHash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    storedHash))
                {
                    return false;
                }

                string[] parts =
                    storedHash.Split('.');

                if (parts.Length != 2)
                {
                    return false;
                }

                byte[] salt =
                    Convert.FromBase64String(
                        parts[0]);

                byte[] expectedHash =
                    Convert.FromBase64String(
                        parts[1]);

                byte[] actualHash =
                    Rfc2898DeriveBytes.Pbkdf2(
                        password,
                        salt,
                        100000,
                        HashAlgorithmName.SHA256,
                        32
                    );

                return
                    CryptographicOperations.FixedTimeEquals(
                        actualHash,
                        expectedHash);
            }
            catch
            {
                return false;
            }
        }


        // =========================================================
        // GENERATE JWT
        // =========================================================

        private string GenerateJwtToken(
            long userId,
            string username,
            string role,
            long? employeeId)
        {
            string jwtKey =
                _configuration["Jwt:Key"]
                ?? "COFFEE_HOUSE_SECRET_KEY_VERY_LONG_AND_SECURE_123456";

            string jwtIssuer =
                _configuration["Jwt:Issuer"]
                ?? "CoffeeHouseAPI";

            string jwtAudience =
                _configuration["Jwt:Audience"]
                ?? "CoffeeHouseApp";

            var claims =
                new List<Claim>
                {
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        userId.ToString()
                    ),

                    new Claim(
                        ClaimTypes.Name,
                        username
                    ),

                    new Claim(
                        ClaimTypes.Role,
                        role
                    ),

                    new Claim(
                        "role",
                        role
                    )
                };

            if (employeeId.HasValue)
            {
                claims.Add(
                    new Claim(
                        "employeeId",
                        employeeId.Value.ToString()
                    )
                );
            }

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token =
                new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(8),
                    signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }


    // =============================================================
    // CHANGE PASSWORD REQUEST
    // =============================================================

    public class ChangePasswordRequest
    {
        public string Username { get; set; }
            = string.Empty;

        public string NewPassword { get; set; }
            = string.Empty;
    }
}