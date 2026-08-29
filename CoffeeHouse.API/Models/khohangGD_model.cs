using System.Collections.Generic;

namespace CoffeeHouse.API.Models
{
    public class KhoHangGDViewModel
    {
        public List<KhoHangItemModel>? InventoryList { get; set; }
    }

    public class KhoHangItemModel
    {
        public string? BranchName { get; set; }
        public decimal ImportValue { get; set; }
        public decimal ExportValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public string? Status { get; set; }
    }
}