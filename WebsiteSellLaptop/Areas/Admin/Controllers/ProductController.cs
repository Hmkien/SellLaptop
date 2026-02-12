using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using X.PagedList.Extensions;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context) => _context = context;

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            var query = _context.Products.Include(p => p.Category).Include(p => p.Brand)
                .Where(x => x.Status != StatusEntity.Deleted).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Name.Contains(keyword));
            ViewBag.Keyword = keyword;
            var data = query.OrderByDescending(x => x.Created).ToPagedList(page, 10);
            return View(data);
        }

        private void LoadDropdowns()
        {
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status == StatusEntity.Approved), "Id", "Name");
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status == StatusEntity.Approved), "Id", "Name");
        }

        public IActionResult Create()
        {
            LoadDropdowns();
            return View(new Product());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(model); }
            model.Slug = model.Name.ToLower().Replace(" ", "-");
            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var item = await _context.Products.Include(p => p.Category).Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return NotFound();
            LoadDropdowns();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(model); }
            var item = await _context.Products.FindAsync(model.Id);
            if (item == null) return NotFound();
            item.Name = model.Name; item.ShortDescription = model.ShortDescription; item.Description = model.Description;
            item.Price = model.Price; item.DiscountPrice = model.DiscountPrice; item.StockQuantity = model.StockQuantity;
            item.ThumbnailUrl = model.ThumbnailUrl; item.CPU = model.CPU; item.RAM = model.RAM;
            item.Storage = model.Storage; item.Screen = model.Screen; item.GPU = model.GPU;
            item.Battery = model.Battery; item.Weight = model.Weight; item.OS = model.OS;
            item.IsFeatured = model.IsFeatured; item.CategoryId = model.CategoryId; item.BrandId = model.BrandId;
            item.Slug = model.Name.ToLower().Replace(" ", "-");
            item.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost] public async Task<IActionResult> Approve(Guid id) { var i = await _context.Products.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Approved; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost] public async Task<IActionResult> Reject(Guid id) { var i = await _context.Products.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Rejected; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost] public async Task<IActionResult> Delete(Guid id) { var i = await _context.Products.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Deleted; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    }
}
