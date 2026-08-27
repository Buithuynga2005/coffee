using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace CoffeeHouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // =========================================================
        // LẤY CHUỖI KẾT NỐI MYSQL
        // =========================================================
        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection")
                   ?? throw new Exception("Không tìm thấy chuỗi kết nối MySQL.");
        }


        // =========================================================
        // GET: api/User
        // LẤY DANH SÁCH TÀI KHOẢN
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = new List<object>();

            try
            {
                using var connection =
                    new MySqlConnection(GetConnectionString());

                await connection.OpenAsync();

                string sql = @"
                    SELECT
                        u.user_id,
                        u.employee_id,
                        u.role_id,
                        r.role_code,
                        r.role_name,
                        u.username,
                        u.email,
                        u.status,
                        u.last_login_at,
                        u.created_at,
                        u.updated_at
                    FROM user u
                    INNER JOIN role r
                        ON u.role_id = r.role_id
                    ORDER BY u.user_id DESC;
                ";

                using var command =
                    new MySqlCommand(sql, connection);

                using var reader =
                    await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(new
                    {
                        user_id = reader["user_id"],
                        employee_id = reader["employee_id"] == DBNull.Value
                            ? null
                            : reader["employee_id"],

                        role_id = reader["role_id"],
                        role_code = reader["role_code"],
                        role_name = reader["role_name"],

                        username = reader["username"],
                        email = reader["email"],

                        status = reader["status"],

                        last_login_at =
                            reader["last_login_at"] == DBNull.Value
                                ? null
                                : reader["last_login_at"],

                        created_at = reader["created_at"],
                        updated_at = reader["updated_at"]
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể lấy danh sách tài khoản.",
                    error = ex.Message
                });
            }
        }


        // =========================================================
        // GET: api/User/5
        // LẤY THÔNG TIN 1 TÀI KHOẢN
        // =========================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(long id)
        {
            try
            {
                using var connection =
                    new MySqlConnection(GetConnectionString());

                await connection.OpenAsync();

                string sql = @"
                    SELECT
                        u.user_id,
                        u.employee_id,
                        u.role_id,
                        r.role_code,
                        r.role_name,
                        u.username,
                        u.email,
                        u.status,
                        u.last_login_at,
                        u.created_at,
                        u.updated_at
                    FROM user u
                    INNER JOIN role r
                        ON u.role_id = r.role_id
                    WHERE u.user_id = @user_id;
                ";

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@user_id", id);

                using var reader =
                    await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài khoản."
                    });
                }

                var user = new
                {
                    user_id = reader["user_id"],

                    employee_id =
                        reader["employee_id"] == DBNull.Value
                            ? null
                            : reader["employee_id"],

                    role_id = reader["role_id"],
                    role_code = reader["role_code"],
                    role_name = reader["role_name"],

                    username = reader["username"],
                    email = reader["email"],
                    status = reader["status"],

                    last_login_at =
                        reader["last_login_at"] == DBNull.Value
                            ? null
                            : reader["last_login_at"],

                    created_at = reader["created_at"],
                    updated_at = reader["updated_at"]
                };

                return Ok(new
                {
                    success = true,
                    data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể lấy thông tin tài khoản.",
                    error = ex.Message
                });
            }
        }


        // =========================================================
        // GET: api/User/role/MANAGER
        // LẤY TÀI KHOẢN THEO CHỨC VỤ
        // =========================================================
        [HttpGet("role/{roleCode}")]
        public async Task<IActionResult> GetUsersByRole(string roleCode)
        {
            var users = new List<object>();

            try
            {
                using var connection =
                    new MySqlConnection(GetConnectionString());

                await connection.OpenAsync();

                string sql = @"
                    SELECT
                        u.user_id,
                        u.employee_id,
                        u.username,
                        u.email,
                        u.status,
                        r.role_code,
                        r.role_name,
                        u.created_at
                    FROM user u
                    INNER JOIN role r
                        ON u.role_id = r.role_id
                    WHERE r.role_code = @role_code
                    ORDER BY u.user_id DESC;
                ";

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@role_code",
                    roleCode.ToUpper()
                );

                using var reader =
                    await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(new
                    {
                        user_id = reader["user_id"],

                        employee_id =
                            reader["employee_id"] == DBNull.Value
                                ? null
                                : reader["employee_id"],

                        username = reader["username"],
                        email = reader["email"],
                        status = reader["status"],

                        role_code = reader["role_code"],
                        role_name = reader["role_name"],

                        created_at = reader["created_at"]
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể lấy tài khoản theo chức vụ.",
                    error = ex.Message
                });
            }
        }


        // =========================================================
        // PUT: api/User/5/lock
        // KHÓA TÀI KHOẢN
        // =========================================================
        [HttpPut("{id}/lock")]
        public async Task<IActionResult> LockUser(long id)
        {
            try
            {
                using var connection =
                    new MySqlConnection(GetConnectionString());

                await connection.OpenAsync();

                string sql = @"
                    UPDATE user
                    SET status = 'LOCKED'
                    WHERE user_id = @user_id;
                ";

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@user_id", id);

                int rows =
                    await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài khoản."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã khóa tài khoản."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể khóa tài khoản.",
                    error = ex.Message
                });
            }
        }


        // =========================================================
        // PUT: api/User/5/unlock
        // MỞ KHÓA TÀI KHOẢN
        // =========================================================
        [HttpPut("{id}/unlock")]
        public async Task<IActionResult> UnlockUser(long id)
        {
            try
            {
                using var connection =
                    new MySqlConnection(GetConnectionString());

                await connection.OpenAsync();

                string sql = @"
                    UPDATE user
                    SET status = 'ACTIVE'
                    WHERE user_id = @user_id;
                ";

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@user_id", id);

                int rows =
                    await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài khoản."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã mở khóa tài khoản."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể mở khóa tài khoản.",
                    error = ex.Message
                });
            }
        }


        // =========================================================
        // PUT: api/User/5/inactive
        // VÔ HIỆU HÓA TÀI KHOẢN
        // =========================================================
        [HttpPut("{id}/inactive")]
        public async Task<IActionResult> InactiveUser(long id)
        {
            try
            {
                using var connection =
                    new MySqlConnection(GetConnectionString());

                await connection.OpenAsync();

                string sql = @"
                    UPDATE user
                    SET status = 'INACTIVE'
                    WHERE user_id = @user_id;
                ";

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@user_id", id);

                int rows =
                    await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài khoản."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã vô hiệu hóa tài khoản."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể vô hiệu hóa tài khoản.",
                    error = ex.Message
                });
            }
        }


        // =========================================================
        // DELETE: api/User/5
        // XÓA TÀI KHOẢN
        // =========================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            try
            {
                using var connection =
                    new MySqlConnection(GetConnectionString());

                await connection.OpenAsync();

                string sql = @"
                    DELETE FROM user
                    WHERE user_id = @user_id;
                ";

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@user_id", id);

                int rows =
                    await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tài khoản."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã xóa tài khoản."
                });
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Không thể xóa tài khoản vì tài khoản đang được sử dụng ở dữ liệu khác.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể xóa tài khoản.",
                    error = ex.Message
                });
            }
        }
    }
}