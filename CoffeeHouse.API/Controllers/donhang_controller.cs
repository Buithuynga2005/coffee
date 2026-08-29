using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Collections.Generic;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/DonHang")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly string _connStr;
        public DonHangController(IConfiguration config) { _connStr = config.GetConnectionString("DefaultConnection") ?? ""; }

        [HttpGet]
        public IActionResult GetOrders([FromQuery] string? dateRange, [FromQuery] string? keyword)
        {
            try
            {
                var orders = new List<object>();
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    string query = @"
                        SELECT o.order_date, e.full_name as cashier, p.payment_method, o.status, o.total_amount, o.order_type
                        FROM `order` o
                        JOIN employee e ON o.employee_id = e.employee_id
                        LEFT JOIN payment p ON o.order_id = p.order_id
                        WHERE 1=1 ";
                    
                    if (!string.IsNullOrEmpty(keyword)) query += " AND e.full_name LIKE @kw";
                    query += " ORDER BY o.order_date DESC LIMIT 50";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string payMethod = reader["payment_method"].ToString() ?? "N/A";
                                string status = reader["status"].ToString() ?? "";
                                
                                orders.Add(new {
                                    time = Convert.ToDateTime(reader["order_date"]).ToString("dd/MM/yyyy HH:mm"),
                                    cashier = reader["cashier"].ToString(),
                                    server = reader["order_type"].ToString() == "TAI_BAN" ? "Tại bàn" : "Mang đi",
                                    totalAmount = Convert.ToDecimal(reader["total_amount"]),
                                    paymentMethod = payMethod == "CASH" ? "Tiền mặt" : (payMethod == "BANK_TRANSFER" ? "Chuyển khoản" : "QR Code"),
                                    status = status == "COMPLETED" ? "Đã hoàn thành" : (status == "CANCELLED" ? "Đã hủy" : "Đang xử lý")
                                });
                            }
                        }
                    }
                }
                return Ok(new { success = true, data = new { shiftStartTime = DateTime.Now.ToString("dd/MM/yyyy 06:30"), staffName = "Hệ thống", orders } });
            }
            catch (Exception ex) { return Ok(new { success = false, message = ex.Message }); }
        }
    }
}