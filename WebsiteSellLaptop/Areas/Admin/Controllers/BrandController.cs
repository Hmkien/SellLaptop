using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using X.PagedList.Extensions;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;
        public BrandController(AppDbContext context) => _context = context;

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            var query = _context.Brands.Where(x => x.Status != StatusEntity.Deleted).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Name.Contains(keyword));
            ViewBag.Keyword = keyword;
            var data = query.OrderByDescending(x => x.Created).ToPagedList(page, 10);
            return View(data);
        }

        public IActionResult Create() => View(new Brand());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Brands.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Brand model)
        {
            if (!ModelState.IsValid) return View(model);
            var item = await _context.Brands.FindAsync(model.Id);
            if (item == null) return NotFound();
            item.Name = model.Name;
            item.Description = model.Description;
            item.LogoUrl = model.LogoUrl;
            item.Website = model.Website;
            item.SortOrder = model.SortOrder;
            item.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            item.Status = StatusEntity.Approved;
            item.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            item.Status = StatusEntity.Rejected;
            item.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            item.Status = StatusEntity.Deleted;
            item.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
