using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using CoffeeHouse.API.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/NhanVienGD")]
    [ApiController]
    public class NhanVienGDController : ControllerBase
    {
        private readonly string _connectionString;

        public NhanVienGDController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpGet]
        public IActionResult GetEmployees([FromQuery] string? keyword, [FromQuery] string? shiftFilter, [FromQuery] int page = 1)
        {
            int pageSize = 9; // Giới hạn 9 nhân viên 1 trang
            var allEmployees = new List<NhanVienModel>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Lấy tất cả nhân viên đang hoạt động (ACTIVE) từ bảng employee
                    string query = "SELECT employee_id, full_name, phone, position, hire_date FROM employee WHERE status = 'ACTIVE'";
                    
                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Lấy ID thật và format thành dạng NV01, NV02 cho đẹp mắt
                            string rawId = reader["employee_id"].ToString() ?? "0";
                            int empId = int.Parse(rawId);
                            string formattedId = "NV" + empId.ToString("D2");

                            // THUẬT TOÁN TẠO CA LÀM (Vì DB của bạn chưa có cột này)
                            // Chia ngẫu nhiên theo ID để giao diện có dữ liệu lọc
                            string caLam = "Fulltime";
                            if (empId % 4 == 1) caLam = "Ca sáng";
                            else if (empId % 4 == 2) caLam = "Ca chiều";
                            else if (empId % 4 == 3) caLam = "Ca tối";

                            // Thêm vào danh sách
                            allEmployees.Add(new NhanVienModel
                            {
                                MaNV = formattedId,
                                HoTen = reader["full_name"].ToString(),
                                SDT = reader["phone"].ToString(),
                                ChucVu = reader["position"].ToString(),
                                CaLam = caLam, 
                                NgayVaoLam = reader["hire_date"] != DBNull.Value ? Convert.ToDateTime(reader["hire_date"]).ToString("dd/MM/yyyy") : ""
                            });
                        }
                    }
                }

                // 1. Xử lý Tìm kiếm (Theo Tên, SĐT, Mã NV)
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower().Trim();
                    allEmployees = allEmployees.Where(e => 
                        (e.HoTen != null && e.HoTen.ToLower().Contains(keyword)) ||
                        (e.SDT != null && e.SDT.Contains(keyword)) ||
                        (e.MaNV != null && e.MaNV.ToLower().Contains(keyword))
                    ).ToList();
                }

                // 2. Xử lý Lọc theo Ca làm (Bấm Tabs)
                if (!string.IsNullOrEmpty(shiftFilter) && shiftFilter != "Tất cả")
                {
                    allEmployees = allEmployees.Where(e => e.CaLam == shiftFilter).ToList();
                }

                // 3. Xử lý Phân trang (Pagination)
                int totalItems = allEmployees.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = page < 1 ? 1 : (page > totalPages && totalPages > 0 ? totalPages : page);

                var pagedEmployees = allEmployees.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var data = new NhanVienGDViewModel
                {
                    Employees = pagedEmployees,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalEmployees = totalItems
                };

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Lỗi Database: " + ex.Message });
            }
        }
    }
}