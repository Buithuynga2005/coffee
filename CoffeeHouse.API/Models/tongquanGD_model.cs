using System.Collections.Generic;

namespace CoffeeHouse.API.Models
{
    public class TongQuanGDModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveBranches { get; set; }
        
        public List<BranchRevenueModel> BranchList { get; set; }
        public FinanceSummaryModel FinanceSummary { get; set; }
        public List<ChartItemModel> ExpenseChartData { get; set; }
    }

    public class BranchRevenueModel
    {
        public string BranchName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expense { get; set; }
        public decimal Profit { get; set; }
        public double Percentage { get; set; } 
        public string Color { get; set; }      
    }

    public class FinanceSummaryModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Profit { get; set; }
        public double ProfitMargin { get; set; }
    }

    public class ChartItemModel
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; }
    }
}