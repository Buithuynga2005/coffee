using System.Collections.Generic;

namespace CoffeeHouse.API.Models
{
    // Model cho màn hình Tổng quan
    public class ChiTieuOverviewModel
    {
        public decimal TotalExpense { get; set; }
        public decimal AvgExpense { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ExpenseCategoryStat>? Categories { get; set; }
    }

    public class ExpenseCategoryStat
    {
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }

    // Model cho màn hình Chi tiết danh mục
    public class ChiTieuDetailViewModel
    {
        public List<ExpenseDetailItem>? Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }

    public class ExpenseDetailItem
    {
        public string? RequestCode { get; set; }
        public string? CreatedAt { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? BranchName { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
    }
}