using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using CoffeeHouse.API.Models;
using System.Collections.Generic;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/KhachHangGD")]
    [ApiController]
    public class KhachHangGDController : ControllerBase
    {
        private readonly string _connStr;
        public KhachHangGDController(IConfiguration config) { _connStr = config.GetConnectionString("DefaultConnection") ?? ""; }

        [HttpGet]
        public IActionResult GetCustomers([FromQuery] string? keyword, [FromQuery] string? rankFilter, [FromQuery] int page = 1)
        {
            int pageSize = 9;
            var customers = new List<CustomerModel>();
            int total = 0, regular = 0, vip = 0, totalItems = 0;

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    // 1. Thống kê tổng quan
                    using (var cmd = new MySqlCommand("SELECT COUNT(*) as Total, SUM(CASE WHEN membership_tier='QUEN' THEN 1 ELSE 0 END) as Quen, SUM(CASE WHEN membership_tier='VIP' THEN 1 ELSE 0 END) as Vip FROM customer", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
                            regular = reader["Quen"] != DBNull.Value ? Convert.ToInt32(reader["Quen"]) : 0;
                            vip = reader["Vip"] != DBNull.Value ? Convert.ToInt32(reader["Vip"]) : 0;
                        }
                    }

                    // 2. Query lấy danh sách
                    string baseQuery = @"
                        FROM customer c 
                        WHERE 1=1 ";
                    
                    if (!string.IsNullOrEmpty(keyword)) baseQuery += " AND (c.full_name LIKE @kw OR c.phone LIKE @kw OR c.customer_id LIKE @kw)";
                    if (!string.IsNullOrEmpty(rankFilter) && rankFilter != "Tất cả") 
                    {
                        string dbRank = rankFilter == "Khách VIP" ? "VIP" : (rankFilter == "Khách quen" ? "QUEN" : "THUONG");
                        baseQuery += $" AND c.membership_tier = '{dbRank}'";
                    }

                    using (var cmdCount = new MySqlCommand("SELECT COUNT(*) " + baseQuery, conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmdCount.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                        totalItems = Convert.ToInt32(cmdCount.ExecuteScalar());
                    }

                    string dataQuery = $@"
                        SELECT c.customer_id, c.full_name, c.phone, c.membership_tier, c.total_spending, c.registered_at,
                        (SELECT COUNT(*) FROM `order` o WHERE o.customer_id = c.customer_id) as total_orders
                        {baseQuery} 
                        ORDER BY c.registered_at DESC LIMIT @limit OFFSET @offset";

                    using (var cmdData = new MySqlCommand(dataQuery, conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmdData.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                        cmdData.Parameters.AddWithValue("@limit", pageSize);
                        cmdData.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

                        using (var reader = cmdData.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tier = reader["membership_tier"].ToString() ?? "THUONG";
                                customers.Add(new CustomerModel
                                {
                                    CustomerId = reader["customer_id"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    Phone = reader["phone"].ToString(),
                                    Rank = tier == "VIP" ? "VIP" : (tier == "QUEN" ? "Quen" : "Thường"),
                                    TotalOrders = Convert.ToInt32(reader["total_orders"]),
                                    TotalSpend = Convert.ToDecimal(reader["total_spending"]),
                                    RegisterDate = Convert.ToDateTime(reader["registered_at"]).ToString("dd/MM/yyyy")
                                });
                            }
                        }
                    }
                }

                return Ok(new { success = true, data = new KhachHangGDViewModel { TotalCustomers = total, RegularCustomers = regular, VipCustomers = vip, Customers = customers, CurrentPage = page, TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize) } });
            }
            catch (Exception ex) { return Ok(new { success = false, message = ex.Message }); }
        }
    }
}