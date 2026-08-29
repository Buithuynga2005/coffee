using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using CoffeeHouse.API.Models;
using System.Collections.Generic;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/KhoHangGD")]
    [ApiController]
    public class KhoHangGDController : ControllerBase
    {
        private readonly string _connStr;
        public KhoHangGDController(IConfiguration config) { _connStr = config.GetConnectionString("DefaultConnection") ?? ""; }

        [HttpGet]
        public IActionResult GetInventoryData([FromQuery] string? dateRange, [FromQuery] string? keyword)
        {
            var inventory = new List<KhoHangItemModel>();
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    string query = @"
                        SELECT b.branch_name,
                            (SELECT COALESCE(SUM(total_amount), 0) FROM stock_in WHERE branch_id = b.branch_id) as ImportValue,
                            (SELECT COALESCE(SUM(total_amount), 0) FROM stock_out WHERE branch_id = b.branch_id) as ExportValue,
                            (SELECT COUNT(*) FROM branch_material_stock bms JOIN material m ON bms.material_id = m.material_id WHERE bms.branch_id = b.branch_id AND bms.quantity_on_hand <= m.reorder_level AND bms.quantity_on_hand > 0) as LowStock,
                            (SELECT COUNT(*) FROM branch_material_stock WHERE branch_id = b.branch_id AND quantity_on_hand = 0) as OutOfStock
                        FROM branch b WHERE b.is_active = 1 ";
                    
                    if (!string.IsNullOrEmpty(keyword)) query += " AND b.branch_name LIKE @kw";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int lowStock = Convert.ToInt32(reader["LowStock"]);
                                int outStock = Convert.ToInt32(reader["OutOfStock"]);
                                string status = outStock > 0 ? "Cần xử lý" : (lowStock > 0 ? "Chú ý" : "Tốt");

                                inventory.Add(new KhoHangItemModel
                                {
                                    BranchName = reader["branch_name"].ToString(),
                                    ImportValue = Convert.ToDecimal(reader["ImportValue"]),
                                    ExportValue = Convert.ToDecimal(reader["ExportValue"]),
                                    LowStockCount = lowStock,
                                    OutOfStockCount = outStock,
                                    Status = status
                                });
                            }
                        }
                    }
                }
                return Ok(new { success = true, data = new KhoHangGDViewModel { InventoryList = inventory } });
            }
            catch (Exception ex) { return Ok(new { success = false, message = ex.Message }); }
        }
    }
}