using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using X.PagedList.Extensions;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private readonly AppDbContext _context;
        public BannerController(AppDbContext context) => _context = context;

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            var query = _context.Banners.Where(x => x.Status != StatusEntity.Deleted).AsQueryable();
            if (!string.IsNullOrEmpty(keyword)) query = query.Where(x => x.Title.Contains(keyword));
            ViewBag.Keyword = keyword;
            return View(query.OrderByDescending(x => x.Created).ToPagedList(page, 10));
        }

        public IActionResult Create() => View(new Banner());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner model) { if (!ModelState.IsValid) return View(model); _context.Banners.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }

        public async Task<IActionResult> Detail(Guid id) { var i = await _context.Banners.FindAsync(id); return i == null ? NotFound() : View(i); }
        public async Task<IActionResult> Edit(Guid id) { var i = await _context.Banners.FindAsync(id); return i == null ? NotFound() : View(i); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Banner model)
        {
            if (!ModelState.IsValid) return View(model);
            var i = await _context.Banners.FindAsync(model.Id); if (i == null) return NotFound();
            i.Title = model.Title; i.SubTitle = model.SubTitle; i.ImageUrl = model.ImageUrl; i.LinkUrl = model.LinkUrl; i.SortOrder = model.SortOrder; i.LastModified = DateTime.Now;
            await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
        }

        [HttpPost] public async Task<IActionResult> Approve(Guid id) { var i = await _context.Banners.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Approved; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost] public async Task<IActionResult> Reject(Guid id) { var i = await _context.Banners.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Rejected; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost] public async Task<IActionResult> Delete(Guid id) { var i = await _context.Banners.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Deleted; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    }
}
