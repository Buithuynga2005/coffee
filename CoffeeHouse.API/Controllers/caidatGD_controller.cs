using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using CoffeeHouse.API.Models;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/CaiDatGD")]
    [ApiController]
    public class CaiDatGDController : ControllerBase
    {
        private readonly string _connectionString;

        public CaiDatGDController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpGet("Info")]
        public IActionResult GetStoreInfo([FromQuery] string branchCode = "CG")
        {
            try
            {
                var settings = new CaiDatGDViewModel();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    // Lấy dữ liệu thật từ bảng branch 
                    string query = "SELECT branch_code, branch_name, address, phone, is_active, created_at FROM branch WHERE branch_code = @code";
                    
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", branchCode);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                settings.StoreName = "Coffee House"; // Tên thương hiệu 
                                settings.BranchCode = reader["branch_code"].ToString();
                                settings.BranchName = reader["branch_name"].ToString();
                                settings.Phone = reader["phone"].ToString();
                                settings.Address = reader["address"].ToString();
                                
                                bool isActive = Convert.ToBoolean(reader["is_active"]);
                                settings.Status = isActive ? "Hoạt động" : "Ngừng hoạt động";
                                
                                settings.CreatedAt = reader["created_at"] != DBNull.Value 
                                    ? Convert.ToDateTime(reader["created_at"]).ToString("dd/MM/yyyy") 
                                    : "";
                            }
                        }
                    }
                }

                // Cấu hình phụ chưa có trong Database
                settings.Email = "coffeehouse@gmail.com";
                settings.Website = "https://coffeehouse.vn";
                settings.OpenTime = "06:30";
                settings.CloseTime = "22:30";
                settings.Currency = "VND (Việt Nam Đồng)";

                return Ok(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Lỗi Database: " + ex.Message });
            }
        }

        [HttpPost("Save")]
        public IActionResult SaveStoreInfo([FromBody] CaiDatGDViewModel model, [FromQuery] string branchCode = "CG")
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    // Lệnh UPDATE ghi đè dữ liệu mới vào CSDL thật
                    string query = "UPDATE branch SET phone = @phone, address = @address WHERE branch_code = @code";
                    
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@phone", model.Phone ?? "");
                        cmd.Parameters.AddWithValue("@address", model.Address ?? "");
                        cmd.Parameters.AddWithValue("@code", branchCode);
                        
                        cmd.ExecuteNonQuery(); // Thực thi lưu vào MySQL
                    }
                }
                return Ok(new { success = true, message = "Đã lưu thay đổi vào cơ sở dữ liệu thành công!" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Lỗi khi lưu Database: " + ex.Message });
            }
        }
    }
}