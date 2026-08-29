using System.Collections.Generic;

namespace CoffeeHouse.API.Models
{
    public class DonHangViewModel
    {
        public string? ShiftStartTime { get; set; }
        public string? StaffName { get; set; }
        public List<OrderItemModel>? Orders { get; set; }
    }

    public class OrderItemModel
    {
        public string? OrderId { get; set; }
        public string? Time { get; set; }
        public string? Cashier { get; set; }
        public string? Server { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
    }
}