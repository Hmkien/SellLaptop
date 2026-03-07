using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            DateTime now = DateTime.Now;
            DateTime startOfMonth = new(now.Year, now.Month, 1);
            DateTime startOfYear = new(now.Year, 1, 1);

            // Thống kê tổng quan
            ViewBag.TotalProducts = await _context.Products.Where(p => p.Status == StatusEntity.Approved).CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);
            ViewBag.TotalCustomers = await _context.Users.CountAsync();

            // Đơn hàng tháng này
            ViewBag.MonthOrders = await _context.Orders
                .Where(o => o.Created >= startOfMonth)
                .CountAsync();
            ViewBag.MonthRevenue = await _context.Orders
                .Where(o => o.Created >= startOfMonth && o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            // Đơn hàng năm nay
            ViewBag.YearOrders = await _context.Orders
                .Where(o => o.Created >= startOfYear)
                .CountAsync();
            ViewBag.YearRevenue = await _context.Orders
                .Where(o => o.Created >= startOfYear && o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            // Trạng thái đơn hàng
            ViewBag.PendingOrders = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.Pending).CountAsync();
            ViewBag.ConfirmedOrders = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.Confirmed).CountAsync();
            ViewBag.ShippingOrders = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.Shipping).CountAsync();
            ViewBag.CompletedOrders = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.Completed).CountAsync();
            ViewBag.CancelledOrders = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.Cancelled).CountAsync();

            // Top sản phẩm bán chạy
            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.Product != null && od.Product.Status == StatusEntity.Approved)
                .GroupBy(od => new { od.ProductId, od.Product!.Name, od.Product.ThumbnailUrl })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.Name,
                    g.Key.ThumbnailUrl,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.TotalPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();
            ViewBag.TopProducts = topProducts;

            // Doanh thu 12 tháng gần nhất
            List<decimal> monthlyRevenue = [];
            for (int i = 11; i >= 0; i--)
            {
                DateTime monthStart = now.AddMonths(-i).Date.AddDays(1 - now.AddMonths(-i).Day);
                DateTime monthEnd = monthStart.AddMonths(1);
                decimal revenue = await _context.Orders
                    .Where(o => o.Created >= monthStart && o.Created < monthEnd && o.OrderStatus == OrderStatus.Completed)
                    .SumAsync(o => o.TotalAmount);
                monthlyRevenue.Add(revenue);
            }
            ViewBag.MonthlyRevenue = monthlyRevenue;

            // Đơn hàng 7 ngày gần nhất
            List<int> dailyOrders = [];
            for (int i = 6; i >= 0; i--)
            {
                DateTime dayStart = now.AddDays(-i).Date;
                DateTime dayEnd = dayStart.AddDays(1);
                int count = await _context.Orders
                    .Where(o => o.Created >= dayStart && o.Created < dayEnd)
                    .CountAsync();
                dailyOrders.Add(count);
            }
            ViewBag.DailyOrders = dailyOrders;

            return View();
        }
    }
}
