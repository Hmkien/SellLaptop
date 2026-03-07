using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using WebsiteSellLaptop.Services;
using X.PagedList.Extensions;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUpload;

        public BlogController(AppDbContext context, IFileUploadService fileUpload)
        {
            _context = context;
            _fileUpload = fileUpload;
        }

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            IQueryable<Blog> query = _context.Blogs.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Title.Contains(keyword));
            }

            ViewBag.Keyword = keyword;
            return View(query.OrderByDescending(x => x.Created).ToPagedList(page, 10));
        }

        public IActionResult Create()
        {
            return View(new Blog());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog model, IFormFile? thumbnailFile)
        {
            _ = ModelState.Remove(nameof(Blog.Code));
            _ = ModelState.Remove(nameof(Blog.Slug));
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                model.ThumbnailUrl = await _fileUpload.UploadImageAsync(thumbnailFile, "uploads/blogs");
            }

            model.Code = $"BLOG{DateTime.Now:yyyyMMddHHmmss}";
            model.Slug = model.Title.ToLower().Replace(" ", "-");
            model.CreatedBy = User.Identity?.Name;

            _ = _context.Blogs.Add(model);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(Guid id) { Blog? i = await _context.Blogs.FindAsync(id); return i == null ? NotFound() : View(i); }
        public async Task<IActionResult> Edit(Guid id) { Blog? i = await _context.Blogs.FindAsync(id); return i == null ? NotFound() : View(i); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Blog model, IFormFile? thumbnailFile)
        {
            _ = ModelState.Remove(nameof(Blog.Code));
            _ = ModelState.Remove(nameof(Blog.Slug));
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Blog? i = await _context.Blogs.FindAsync(model.Id);
            if (i == null)
            {
                return NotFound();
            }

            i.Title = model.Title;
            i.Summary = model.Summary;
            i.Content = model.Content;
            i.Category = model.Category;
            i.Slug = model.Title.ToLower().Replace(" ", "-");
            i.LastModified = DateTime.Now;
            i.ModifiedBy = User.Identity?.Name;

            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                i.ThumbnailUrl = await _fileUpload.UploadImageAsync(thumbnailFile, "uploads/blogs");
            }
            else if (!string.IsNullOrEmpty(model.ThumbnailUrl))
            {
                i.ThumbnailUrl = model.ThumbnailUrl;
            }

            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Approve(Guid id) { Blog? i = await _context.Blogs.FindAsync(id); if (i == null) { return Json(new { success = false }); } i.Status = StatusEntity.Approved; i.LastModified = DateTime.Now; _ = await _context.SaveChangesAsync(); return Json(new { success = true, message = "Đã duyệt bài viết" }); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Reject(Guid id) { Blog? i = await _context.Blogs.FindAsync(id); if (i == null) { return Json(new { success = false }); } i.Status = StatusEntity.Rejected; i.LastModified = DateTime.Now; _ = await _context.SaveChangesAsync(); return Json(new { success = true, message = "Đã hủy duyệt bài viết" }); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            Blog? i = await _context.Blogs.FindAsync(id);
            if (i == null)
            {
                return Json(new { success = false });
            }
            _ = _context.Blogs.Remove(i);
            _ = await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa bài viết" });
        }
    }
}
