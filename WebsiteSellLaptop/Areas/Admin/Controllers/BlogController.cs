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
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        public BlogController(AppDbContext context) => _context = context;

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            var query = _context.Blogs.Where(x => x.Status != StatusEntity.Deleted).AsQueryable();
            if (!string.IsNullOrEmpty(keyword)) query = query.Where(x => x.Title.Contains(keyword));
            ViewBag.Keyword = keyword;
            return View(query.OrderByDescending(x => x.Created).ToPagedList(page, 10));
        }

        public IActionResult Create() => View(new Blog());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog model)
        {
            if (!ModelState.IsValid) return View(model);
            model.Slug = model.Title.ToLower().Replace(" ", "-");
            _context.Blogs.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(Guid id) { var i = await _context.Blogs.FindAsync(id); return i == null ? NotFound() : View(i); }
        public async Task<IActionResult> Edit(Guid id) { var i = await _context.Blogs.FindAsync(id); return i == null ? NotFound() : View(i); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Blog model)
        {
            if (!ModelState.IsValid) return View(model);
            var i = await _context.Blogs.FindAsync(model.Id); if (i == null) return NotFound();
            i.Title = model.Title; i.Summary = model.Summary; i.Content = model.Content;
            i.ThumbnailUrl = model.ThumbnailUrl; i.Category = model.Category;
            i.Slug = model.Title.ToLower().Replace(" ", "-"); i.LastModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost] public async Task<IActionResult> Approve(Guid id) { var i = await _context.Blogs.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Approved; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost] public async Task<IActionResult> Reject(Guid id) { var i = await _context.Blogs.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Rejected; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost] public async Task<IActionResult> Delete(Guid id) { var i = await _context.Blogs.FindAsync(id); if (i == null) return NotFound(); i.Status = StatusEntity.Deleted; i.LastModified = DateTime.Now; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    }
}
