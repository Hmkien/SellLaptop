using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using WebsiteSellLaptop.Models.ViewModels;
using WebsiteSellLaptop.Services;

namespace WebsiteSellLaptop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IVnPayService _vnPayService;
        private const string CartSessionKey = "Cart";

        public CheckoutController(
            AppDbContext context, 
            UserManager<AppUser> userManager,
            IVnPayService vnPayService)
        {
            _context = context;
            _userManager = userManager;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Pre-fill user info if logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.UserFullName = user.FullName;
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserPhone = user.PhoneNumber;
                    ViewBag.UserAddress = user.Address;
                }
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.Items.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            // Create order
            var order = new Order
            {
                OrderCode = GenerateOrderCode(),
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Address = $"{model.AddressDetail}, {model.Ward}, {model.District}, {model.Province}",
                Note = model.Note,
                PaymentMethod = model.PaymentMethod,
                OrderStatus = OrderStatus.Pending,
                TotalAmount = cart.FinalAmount,
                DiscountAmount = cart.DiscountAmount,
                CouponCode = cart.CouponCode,
                UserId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null,
                Created = DateTime.Now
            };

            // Add order details
            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    return Json(new { success = false, message = $"Sản phẩm {item.ProductName} không đủ hàng" });
                }

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    TotalPrice = item.Subtotal
                });

                // Update stock
                product.StockQuantity -= item.Quantity;
            }

            // Update coupon usage if applied
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == cart.CouponCode.ToUpper());
                if (coupon != null)
                {
                    coupon.UsedCount++;
                }
            }

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Clear cart
            HttpContext.Session.Remove(CartSessionKey);

            // If VNPay payment, redirect to payment gateway
            if (model.PaymentMethod == PaymentMethod.EWallet)
            {
                var paymentInfo = new PaymentInformationModel
                {
                    OrderId = order.Id,
                    Name = model.FullName,
                    OrderDescription = order.OrderCode,
                    Amount = order.TotalAmount,
                    OrderType = "order"
                };

                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);
                return Json(new { success = true, paymentUrl = paymentUrl });
            }

            // COD or other methods - direct to success page
            return Json(new { success = true, redirectUrl = Url.Action("Success", new { orderId = order.Id }) });
        }

        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success && response.OrderId.HasValue)
            {
                var order = _context.Orders.Find(response.OrderId.Value);
                if (order != null)
                {
                    order.OrderStatus = OrderStatus.Confirmed;
                    _context.SaveChanges();

                    return RedirectToAction("Success", new { orderId = order.Id });
                }
            }

            return RedirectToAction("Failed");
        }

        public async Task<IActionResult> Success(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult Failed()
        {
            return View();
        }

        private string GenerateOrderCode()
        {
            return "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private CartViewModel GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            return JsonConvert.DeserializeObject<CartViewModel>(cartJson) ?? new CartViewModel();
        }
    }

    public class CheckoutViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;
        public string? Note { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
