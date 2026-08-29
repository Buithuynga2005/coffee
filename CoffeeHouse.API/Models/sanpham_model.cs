using System.Collections.Generic;

namespace CoffeeHouse.API.Models
{
    public class SanPhamViewModel
    {
        public List<CategoryModel>? Categories { get; set; }
        public List<ProductModel>? Products { get; set; }
        public int TotalProducts { get; set; }
        public string? CurrentCategoryName { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class CategoryModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int Count { get; set; }
    }

    public class ProductModel
    {
        public string? Id { get; set; }
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        
        // Đây là dòng bị thiếu gây ra lỗi CS0117
        public bool IsTopping { get; set; } 
    }
}