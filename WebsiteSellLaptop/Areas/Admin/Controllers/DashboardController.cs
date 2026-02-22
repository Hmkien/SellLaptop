using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Basic stats
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalBlogs = await _context.Blogs.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalBrands = await _context.Brands.CountAsync();

            // Revenue stats
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(o => o.OrderStatus == OrderStatus.Pending);

            // Recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.Created)
                .Take(5)
                .ToListAsync();
            ViewBag.RecentOrders = recentOrders;

            // Top products
            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.ProductId)
                .Select(g => new { 
                    ProductName = g.First().Product.Name, 
                    TotalSold = g.Sum(od => od.Quantity) 
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(5)
                .ToListAsync();
            ViewBag.TopProducts = topProducts;

            // Monthly revenue (last 6 months)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var monthlyRevenue = await _context.Orders
                .Where(o => o.Created >= sixMonthsAgo && o.OrderStatus == OrderStatus.Completed)
                .GroupBy(o => new { o.Created.Year, o.Created.Month })
                .Select(g => new { 
                    Year = g.Key.Year,
                    Month = g.Key.Month, 
                    Revenue = g.Sum(o => o.TotalAmount) 
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToListAsync();
            ViewBag.MonthlyRevenue = monthlyRevenue;

            return View();
        }
    }
}
