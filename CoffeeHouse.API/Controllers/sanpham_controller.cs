using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Collections.Generic;
using System;

namespace CoffeeHouse.API.Controllers
{
    [Route("api/SanPham")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly string _connStr;
        public SanPhamController(IConfiguration config) { _connStr = config.GetConnectionString("DefaultConnection") ?? ""; }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] string categoryId = "ALL", [FromQuery] string? keyword = "", [FromQuery] int page = 1)
        {
            try
            {
                var categories = new List<object>();
                var products = new List<object>();
                int pageSize = 8, totalItems = 0;
                string catName = "Tất cả sản phẩm";

                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    // Lấy danh mục
                    using (var cmd = new MySqlCommand("SELECT category_id, category_name, (SELECT COUNT(*) FROM product WHERE category_id = c.category_id AND is_active=1) as count FROM category c ORDER BY display_order", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        int allCount = 0;
                        var tempCats = new List<object>();
                        while (reader.Read())
                        {
                            int count = Convert.ToInt32(reader["count"]);
                            allCount += count;
                            string id = reader["category_id"].ToString() ?? "";
                            string name = reader["category_name"].ToString() ?? "";
                            if (categoryId == id) catName = name;
                            tempCats.Add(new { id = id, name = name, count = count });
                        }
                        categories.Add(new { id = "ALL", name = "Tất cả sản phẩm", count = allCount });
                        categories.AddRange(tempCats);
                    }

                    // Lấy sản phẩm
                    string baseQuery = "FROM product WHERE is_active=1 ";
                    if (categoryId != "ALL") baseQuery += $" AND category_id = '{categoryId}'";
                    if (!string.IsNullOrEmpty(keyword)) baseQuery += " AND product_name LIKE @kw";

                    using (var cmdCount = new MySqlCommand("SELECT COUNT(*) " + baseQuery, conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmdCount.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                        totalItems = Convert.ToInt32(cmdCount.ExecuteScalar());
                    }

                    using (var cmdData = new MySqlCommand($"SELECT product_name, price, image_url, is_topping {baseQuery} ORDER BY product_id DESC LIMIT @limit OFFSET @offset", conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmdData.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                        cmdData.Parameters.AddWithValue("@limit", pageSize);
                        cmdData.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

                        using (var reader = cmdData.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string pName = reader["product_name"].ToString() ?? "";
                                string imgName = "";

                                // MAP TÊN SẢN PHẨM VỚI TÊN FILE ẢNH (Không chạm vào CSDL)
                                switch (pName)
                                {
                                    case "Cà phê đen": case "Cà phê đen đá": imgName = "capheden.jpg"; break;
                                    case "Cà phê sữa": case "Cà phê sữa đá": imgName = "caphesua.jpg"; break;
                                    case "Bạc xỉu": case "Bạc xỉu đá": imgName = "bacsiu.jpg"; break;
                                    case "Cà phê muối": imgName = "caphemuoi.jpg"; break;
                                    case "Cà phê cốt dừa": imgName = "caphecotdua.jpg"; break;
                                    case "Cà phê trứng": imgName = "caphetrung.jpg"; break;
                                    case "Cà phê kem cheese": imgName = "caphekemcheese.jpg"; break;
                                    
                                    case "Trà đào": imgName = "tradao.jpg"; break;
                                    case "Trà vải": imgName = "travai.jpg"; break;
                                    case "Trà chanh": imgName = "trachanh.jpg"; break;
                                    case "Trà chanh dây": case "Trà tắc": imgName = "trachanhday.jpg"; break;
                                    case "Trà đào cam sả": imgName = "tradaocamsa.jpg"; break;
                                    case "Trà dâu": imgName = "tradau.jpg"; break;
                                    case "Trà xoài": imgName = "traxoai.jpg"; break;
                                    case "Trà nhãn": imgName = "tranhan.jpg"; break;
                                    case "Trà dứa nhiệt đới": imgName = "traduanhietdoi.jpeg"; break;
                                    case "Trà mật ong": imgName = "tramatong.jpg"; break;

                                    case "Matcha đá xay": imgName = "matchadaxay.jpg"; break;
                                    case "Chocolate đá xay": imgName = "Chocolatedaxay.jpg"; break;
                                    case "Cà phê đá xay": imgName = "caphedaxay.jpg"; break;
                                    case "Cookies đá xay": imgName = "Cookiesdaxay.jpg"; break;
                                    case "Caramel đá xay": imgName = "Carameldaxay.jpg"; break;
                                    case "Dâu đá xay": imgName = "daudaxay.jpg"; break;
                                    case "Xoài đá xay": imgName = "xoaidaxay.jpg"; break;
                                    case "Việt quất đá xay": imgName = "vietquatdaxay.jpg"; break;

                                    case "Nước cam": imgName = "camep.jpg"; break;
                                    case "Nước táo": imgName = "taoep.jpg"; break;
                                    case "Nước dứa": imgName = "epdua.jpg"; break;
                                    case "Nước dưa hấu": imgName = "epduahau.jpg"; break;
                                    case "Nước cà rốt": imgName = "epcarot.jpg"; break;
                                    case "Nước chanh dây": imgName = "epchanhday.jpg"; break;
                                    case "Nước ổi": imgName = "epoi.jpg"; break;
                                    case "Nước cóc": imgName = "epcoc.jpg"; break;
                                    case "Nước ép mix": imgName = "epmix.jpg"; break;

                                    case "Sinh tố bơ": imgName = "sinhtobo.jpg"; break;
                                    case "Sinh tố xoài": imgName = "sinhtoxoai.jpg"; break;
                                    case "Sinh tố dâu": imgName = "sinhtodau.jpg"; break;
                                    case "Sinh tố mãng cầu": imgName = "sinhtomangcau.jpg"; break;
                                    case "Sinh tố chuối": imgName = "sinhtochuoi.jpg"; break;
                                    case "Sinh tố việt quất": imgName = "sinhtovietquat.jpg"; break;
                                    case "Sinh tố mix trái cây": imgName = "sinhtomix.jpg"; break;

                                    case "Chocolate nóng": imgName = "Chocolatenong.jpg"; break;
                                    case "Chocolate đá": imgName = "Chocolateda.jpg"; break;
                                    case "Chocolate kem": imgName = "Chocolatekem.jpg"; break;
                                    case "Sữa tươi": imgName = "suatuoi.jpg"; break;
                                    case "Sữa tươi trân châu đường đen": imgName = "suatuoitranchauduongden.jpg"; break;
                                    case "Sữa chua đá": imgName = "suachuadanhda.jpg"; break;
                                    case "Sữa chua trái cây": imgName = "suachuatraicay.jpg"; break;

                                    case "Bánh tiramisu": imgName = "banhtiramisu.jpg"; break;
                                    case "Bánh cheesecake": imgName = "banhcheesecake.jpg"; break;
                                    case "Bánh mousse chocolate": imgName = "Banhmoussechocolate.jpg"; break;
                                    case "Bánh red velvet": imgName = "Banhredvelvet.jpg"; break;
                                    case "Bánh matcha": imgName = "banhmatcha.jpg"; break;
                                    case "Bánh croissant": imgName = "banhcroissant.jpg"; break;
                                    case "Croissant chocolate": imgName = "chocolatecroissant.jpg"; break;
                                    case "Bánh su kem": imgName = "banhsukem.jpg"; break;
                                    case "Muffin chocolate": imgName = "Muffinchocolate.jpg"; break;
                                    case "Bánh chuối": imgName = "banhchuoi.jpg"; break;

                                    case "Trân châu đen": imgName = "tranchauden.jpg"; break;
                                    case "Trân châu trắng": imgName = "tranchautrang.jpg"; break;
                                    case "Pudding": imgName = "Pudding.jpg"; break;
                                    case "Kem cheese (topping)": imgName = "kemcheese.jpg"; break;
                                    case "Shot Espresso": imgName = "ShotEspresso.jpg"; break;
                                    case "Sữa tươi (topping)": imgName = "suatuoi.jpg"; break;

                                    default: imgName = "capheden.jpg"; break; // Ảnh mặc định nếu không khớp
                                }

                                // Tạo đường dẫn tương đối trỏ thẳng vào thư mục assets của bạn
                                string finalImageUrl = $"../assets/images/products/{imgName}";

                                products.Add(new {
                                    name = pName,
                                    price = Convert.ToDecimal(reader["price"]),
                                    imageUrl = finalImageUrl,
                                    isTopping = Convert.ToBoolean(reader["is_topping"])
                                });
                            }
                        }
                    }
                }
                return Ok(new { success = true, data = new { categories, currentCategoryName = catName, totalProducts = totalItems, products, currentPage = page, totalPages = (int)Math.Ceiling(totalItems / (double)pageSize) } });
            }
            catch (Exception ex) { return Ok(new { success = false, message = ex.Message }); }
        }
    }
}