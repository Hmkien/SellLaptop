using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.ViewModels;

namespace WebsiteSellLaptop.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            CartViewModel cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(Guid productId, int quantity = 1)
        {
            Product? product = _context.Products
                .FirstOrDefault(p => p.Id == productId && p.Status == Models.Enums.StatusEntity.Approved);

            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            CartViewModel cart = GetCart();
            CartItemViewModel? existingItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                List<string> specs = [];
                if (!string.IsNullOrEmpty(product.CPU))
                {
                    specs.Add(product.CPU);
                }

                if (!string.IsNullOrEmpty(product.RAM))
                {
                    specs.Add(product.RAM);
                }

                if (!string.IsNullOrEmpty(product.Storage))
                {
                    specs.Add(product.Storage);
                }

                cart.Items.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ThumbnailUrl = product.ThumbnailUrl,
                    Specifications = string.Join(" · ", specs),
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    Quantity = quantity
                });
            }

            SaveCart(cart);

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng",
                cartCount = cart.TotalItems
            });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(Guid productId, int quantity)
        {
            if (quantity is < 1 or > 99)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ" });
            }

            CartViewModel cart = GetCart();
            CartItemViewModel? item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);

                return Json(new
                {
                    success = true,
                    subtotal = item.Subtotal.ToString("N0"),
                    totalAmount = cart.TotalAmount.ToString("N0"),
                    finalAmount = cart.FinalAmount.ToString("N0")
                });
            }

            return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
        }

        [HttpPost]
        public IActionResult RemoveItem(Guid productId)
        {
            CartViewModel cart = GetCart();
            CartItemViewModel? item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                _ = cart.Items.Remove(item);
                SaveCart(cart);

                return Json(new
                {
                    success = true,
                    cartCount = cart.TotalItems,
                    totalAmount = cart.TotalAmount.ToString("N0"),
                    finalAmount = cart.FinalAmount.ToString("N0")
                });
            }

            return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return Json(new { success = true, message = "Đã xóa tất cả sản phẩm" });
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });
            }

            Coupon? coupon = _context.Coupons
                .FirstOrDefault(c => c.Code.ToUpper() == code.ToUpper()
                    && c.Status == Models.Enums.StatusEntity.Approved
                    && c.StartDate <= DateTime.Now
                    && c.EndDate >= DateTime.Now
                    && (c.MaxUsage == null || c.UsedCount < c.MaxUsage));

            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn" });
            }

            CartViewModel cart = GetCart();

            if (coupon.MinOrderAmount.HasValue && cart.TotalAmount < coupon.MinOrderAmount.Value)
            {
                return Json(new
                {
                    success = false,
                    message = $"Đơn hàng tối thiểu {coupon.MinOrderAmount.Value:N0}₫ để áp dụng mã này"
                });
            }

            decimal discountAmount = coupon.IsPercent ? cart.TotalAmount * coupon.DiscountAmount / 100 : coupon.DiscountAmount;
            cart.CouponCode = coupon.Code;
            cart.DiscountAmount = discountAmount;
            SaveCart(cart);

            return Json(new
            {
                success = true,
                message = "Áp dụng mã giảm giá thành công",
                discountAmount = discountAmount.ToString("N0"),
                finalAmount = cart.FinalAmount.ToString("N0")
            });
        }

        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            CartViewModel cart = GetCart();
            cart.CouponCode = null;
            cart.DiscountAmount = 0;
            SaveCart(cart);

            return Json(new
            {
                success = true,
                finalAmount = cart.FinalAmount.ToString("N0")
            });
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            CartViewModel cart = GetCart();
            return Json(cart.TotalItems);
        }

        private CartViewModel GetCart()
        {
            string? cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson)
                ? new CartViewModel()
                : JsonConvert.DeserializeObject<CartViewModel>(cartJson) ?? new CartViewModel();
        }

        private void SaveCart(CartViewModel cart)
        {
            string cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }
    }
}
