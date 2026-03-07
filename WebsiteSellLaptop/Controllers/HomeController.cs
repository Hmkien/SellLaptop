using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get featured products
        ViewBag.FeaturedProducts = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsFeatured && p.Status == StatusEntity.Approved && !p.IsAccessory)
            .OrderByDescending(p => p.Created)
            .Take(10)
            .ToListAsync();

        // Get latest products (non-featured)
        ViewBag.LatestProducts = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.Status == StatusEntity.Approved && !p.IsAccessory)
            .OrderByDescending(p => p.Created)
            .Take(10)
            .ToListAsync();

        // Get categories
        ViewBag.Categories = await _context.Categories
            .Where(c => c.Status == StatusEntity.Approved)
            .OrderBy(c => c.SortOrder)
            .Take(8)
            .ToListAsync();

        // Get banners
        ViewBag.Banners = await _context.Banners
            .Where(b => b.Status == StatusEntity.Approved)
            .OrderBy(b => b.SortOrder)
            .Take(5)
            .ToListAsync();

        // Get brands
        ViewBag.Brands = await _context.Brands
            .Where(b => b.Status == StatusEntity.Approved)
            .OrderBy(b => b.SortOrder)
            .Take(12)
            .ToListAsync();

        // Get partners
        ViewBag.Partners = await _context.Partners
            .Where(p => p.Status == StatusEntity.Approved)
            .OrderBy(p => p.SortOrder)
            .Take(6)
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
