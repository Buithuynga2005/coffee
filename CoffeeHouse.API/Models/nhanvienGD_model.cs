using System.Collections.Generic;

namespace CoffeeHouse.API.Models
{
    public class NhanVienGDViewModel
    {
        public List<NhanVienModel>? Employees { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalEmployees { get; set; }
    }

    public class NhanVienModel
    {
        public string? MaNV { get; set; }
        public string? HoTen { get; set; }
        public string? SDT { get; set; }
        public string? ChucVu { get; set; }
        public string? CaLam { get; set; }
        public string? NgayVaoLam { get; set; }
    }
}