using System.Collections.Generic;

namespace CoffeeHouse.API.Models
{
    public class KhachHangGDViewModel
    {
        // Thống kê tổng
        public int TotalCustomers { get; set; }
        public int RegularCustomers { get; set; }
        public int VipCustomers { get; set; }

        // Danh sách khách hàng hiển thị trên bảng
        public List<CustomerModel>? Customers { get; set; }

        // Thông tin phân trang
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class CustomerModel
    {
        public string? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Rank { get; set; } // VIP, Quen, Thường
        public int TotalOrders { get; set; }
        public decimal TotalSpend { get; set; }
        public string? RegisterDate { get; set; }
    }
}