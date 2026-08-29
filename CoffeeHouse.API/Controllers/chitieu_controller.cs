using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using CoffeeHouse.API.Models;
using System.Collections.Generic;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/ChiTieuGD")]
    [ApiController]
    public class ChiTieuGDController : ControllerBase
    {
        private readonly string _connectionString;

        public ChiTieuGDController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // 1. API Lấy dữ liệu Tổng quan
        [HttpGet("Overview")]
        public IActionResult GetOverview()
        {
            try
            {
                var categories = new List<ExpenseCategoryStat>();
                decimal totalExpense = 0;
                decimal totalRevenue = 0;

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Lấy Doanh thu (Từ bảng order đã hoàn thành)
                    using (var cmd = new MySqlCommand("SELECT SUM(total_amount) FROM `order` WHERE status = 'COMPLETED'", conn))
                    {
                        var revObj = cmd.ExecuteScalar();
                        if (revObj != DBNull.Value) totalRevenue = Convert.ToDecimal(revObj);
                    }

                    // Lấy Tổng Chi Phí (Từ bảng expense_request đã duyệt)
                    using (var cmd = new MySqlCommand("SELECT SUM(amount) FROM expense_request WHERE status = 'APPROVED'", conn))
                    {
                        var expObj = cmd.ExecuteScalar();
                        if (expObj != DBNull.Value) totalExpense = Convert.ToDecimal(expObj);
                    }

                    // Lấy Chi phí theo từng Danh mục
                    string catQuery = @"
                        SELECT ec.expense_category_id, ec.category_name, COALESCE(SUM(er.amount), 0) as total_amount
                        FROM expense_category ec
                        LEFT JOIN expense_request er ON ec.expense_category_id = er.expense_category_id AND er.status = 'APPROVED'
                        GROUP BY ec.expense_category_id, ec.category_name
                        ORDER BY total_amount DESC";

                    using (var cmd = new MySqlCommand(catQuery, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal amt = Convert.ToDecimal(reader["total_amount"]);
                            double pct = totalExpense > 0 ? (double)(amt / totalExpense) * 100 : 0;

                            // TỰ ĐỘNG CHUYỂN ĐỔI SANG TIẾNG VIỆT CÓ DẤU
                            string rawName = reader["category_name"].ToString() ?? "";
                            string vnName = rawName switch
                            {
                                "Nhan su" => "Nhân sự",
                                "Van hanh" => "Vận hành",
                                "Trang thiet bi" => "Trang thiết bị",
                                "Nguyen lieu" => "Nguyên liệu",
                                "Mat bang" => "Mặt bằng",
                                "Dien nuoc" => "Điện nước",
                                "Khac" => "Khác",
                                _ => rawName
                            };

                            categories.Add(new ExpenseCategoryStat
                            {
                                CategoryId = Convert.ToInt64(reader["expense_category_id"]),
                                CategoryName = vnName,
                                Amount = amt,
                                Percentage = Math.Round(pct, 1)
                            });
                        }
                    }
                }

                var data = new ChiTieuOverviewModel
                {
                    TotalExpense = totalExpense,
                    AvgExpense = totalExpense > 0 ? totalExpense / 4 : 0, 
                    TotalRevenue = totalRevenue,
                    Categories = categories
                };

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        // 2. API Lấy chi tiết Hóa đơn theo Danh mục
        [HttpGet("Details")]
        public IActionResult GetDetails([FromQuery] long categoryId, [FromQuery] int page = 1)
        {
            int pageSize = 10;
            var items = new List<ExpenseDetailItem>();
            int totalItems = 0;

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Đếm tổng số lượng hóa đơn
                    using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM expense_request WHERE expense_category_id = @catId", conn))
                    {
                        cmd.Parameters.AddWithValue("@catId", categoryId);
                        totalItems = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Lấy dữ liệu chi tiết có phân trang
                    string query = @"
                        SELECT er.request_code, er.created_at, b.branch_name, er.title, er.description, er.amount, er.status 
                        FROM expense_request er
                        JOIN branch b ON er.branch_id = b.branch_id
                        WHERE er.expense_category_id = @catId
                        ORDER BY er.created_at DESC
                        LIMIT @limit OFFSET @offset";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@catId", categoryId);
                        cmd.Parameters.AddWithValue("@limit", pageSize);
                        cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new ExpenseDetailItem
                                {
                                    RequestCode = reader["request_code"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("dd/MM/yyyy"),
                                    BranchName = reader["branch_name"].ToString(),
                                    Title = reader["title"].ToString(),
                                    Description = reader["description"].ToString(),
                                    Amount = Convert.ToDecimal(reader["amount"]),
                                    Status = reader["status"].ToString()
                                });
                            }
                        }
                    }
                }

                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var data = new ChiTieuDetailViewModel
                {
                    Items = items,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = totalItems
                };

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}