using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Collections.Generic;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/TongQuanGD")]
    [ApiController]
    public class TongQuanGDController : ControllerBase
    {
        private readonly string _connStr;
        public TongQuanGDController(IConfiguration config) { _connStr = config.GetConnectionString("DefaultConnection") ?? ""; }

        [HttpGet]
        public IActionResult GetDashboard()
        {
            try
            {
                decimal totalRev = 0, totalExp = 0;
                int totalOrders = 0, activeBranches = 0;
                var branchList = new List<object>();
                var expChart = new List<object>();

                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    // Thống kê tổng
                    using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM branch WHERE is_active=1", conn)) activeBranches = Convert.ToInt32(cmd.ExecuteScalar());
                    using (var cmd = new MySqlCommand("SELECT COUNT(*), SUM(total_amount) FROM `order` WHERE status='COMPLETED'", conn))
                    using (var reader = cmd.ExecuteReader()) { if (reader.Read()) { totalOrders = Convert.ToInt32(reader[0]); totalRev = reader[1] != DBNull.Value ? Convert.ToDecimal(reader[1]) : 0; } }
                    using (var cmd = new MySqlCommand("SELECT SUM(amount) FROM expense_request WHERE status='APPROVED'", conn)) { var e = cmd.ExecuteScalar(); totalExp = e != DBNull.Value ? Convert.ToDecimal(e) : 0; }

                    // Bảng chi nhánh
                    string[] colors = { "#d88b4b", "#649e49", "#d33f33", "#9c27b0" };
                    int colorIdx = 0;
                    using (var cmd = new MySqlCommand("SELECT b.branch_id, b.branch_name, (SELECT COALESCE(SUM(total_amount), 0) FROM `order` WHERE branch_id = b.branch_id AND status='COMPLETED') as Rev, (SELECT COALESCE(SUM(amount), 0) FROM expense_request WHERE branch_id = b.branch_id AND status='APPROVED') as Exp FROM branch b WHERE b.is_active=1", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal r = Convert.ToDecimal(reader["Rev"]);
                            decimal e = Convert.ToDecimal(reader["Exp"]);
                            branchList.Add(new { branchName = reader["branch_name"].ToString(), revenue = r, expense = e, profit = r - e, percentage = totalRev > 0 ? (double)(r / totalRev * 100) : 0, color = colors[colorIdx++ % colors.Length] });
                        }
                    }

                    // Biểu đồ chi phí
                    using (var cmd = new MySqlCommand("SELECT ec.category_name, SUM(er.amount) as Amt FROM expense_request er JOIN expense_category ec ON er.expense_category_id = ec.expense_category_id WHERE er.status='APPROVED' GROUP BY ec.category_name ORDER BY Amt DESC LIMIT 4", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        colorIdx = 0;
                        while (reader.Read())
                        {
                            decimal amt = Convert.ToDecimal(reader["Amt"]);
                            expChart.Add(new { name = reader["category_name"].ToString(), value = amt, percentage = totalExp > 0 ? (double)(amt / totalExp * 100) : 0, color = colors[colorIdx++ % colors.Length] });
                        }
                    }
                }

                double margin = totalRev > 0 ? (double)((totalRev - totalExp) / totalRev * 100) : 0;
                var data = new {
                    totalRevenue = totalRev, totalOrders, activeBranches, branchList, expenseChartData = expChart,
                    financeSummary = new { totalRevenue = totalRev, totalExpense = totalExp, profit = totalRev - totalExp, profitMargin = margin }
                };
                return Ok(new { success = true, data });
            }
            catch (Exception ex) { return Ok(new { success = false, message = ex.Message }); }
        }
    }
}