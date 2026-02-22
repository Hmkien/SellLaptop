using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private readonly AppDbContext _context;
        public BannerController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index(int page = 1, string? keyword = null, int pageSize = 10)
        {
            var query = _context.Banners.Where(x => x.Status != StatusEntity.Deleted).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var data = await query.OrderByDescending(x => x.Created).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(data);
        }

        public IActionResult Create() => View(new Banner());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner model)
        {
            if (!ModelState.IsValid) return View(model);
            var codeExists = await _context.Banners.AnyAsync(x => x.Code.ToLower() == model.Code.Trim().ToLower() && x.Status != StatusEntity.Deleted);
            if (codeExists) { ModelState.AddModelError("Code", "Mã banner đã tồn tại"); return View(model); }
            model.Code = model.Code.Trim();
            model.CreatedBy = User.Identity?.Name;
            _context.Banners.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(Guid id) { var i = await _context.Banners.FindAsync(id); return i == null ? NotFound() : View(i); }
        public async Task<IActionResult> Edit(Guid id) { var i = await _context.Banners.FindAsync(id); return i == null ? NotFound() : View(i); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Banner model)
        {
            if (!ModelState.IsValid) return View(model);
            var i = await _context.Banners.FindAsync(model.Id); if (i == null) return NotFound();
            if (i.Status == StatusEntity.Approved) { ModelState.AddModelError("", "Không thể sửa banner đã duyệt"); return View(model); }
            var codeExists = await _context.Banners.AnyAsync(x => x.Code.ToLower() == model.Code.Trim().ToLower() && x.Id != model.Id && x.Status != StatusEntity.Deleted);
            if (codeExists) { ModelState.AddModelError("Code", "Mã banner đã tồn tại"); return View(model); }
            i.Code = model.Code.Trim(); i.Title = model.Title; i.SubTitle = model.SubTitle; i.ImageUrl = model.ImageUrl; i.LinkUrl = model.LinkUrl; i.SortOrder = model.SortOrder; i.LastModified = DateTime.Now; i.ModifiedBy = User.Identity?.Name;
            await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Approve(Guid id) { var i = await _context.Banners.FindAsync(id); if (i == null) return Json(new { success = false, message = "Không tìm thấy" }); i.Status = StatusEntity.Approved; i.LastModified = DateTime.Now; i.ModifiedBy = User.Identity?.Name; await _context.SaveChangesAsync(); return Json(new { success = true, message = "Duyệt thành công" }); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Reject(Guid id) { var i = await _context.Banners.FindAsync(id); if (i == null) return Json(new { success = false, message = "Không tìm thấy" }); i.Status = StatusEntity.Rejected; i.LastModified = DateTime.Now; i.ModifiedBy = User.Identity?.Name; await _context.SaveChangesAsync(); return Json(new { success = true, message = "Hủy duyệt thành công" }); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Delete(Guid id) { var i = await _context.Banners.FindAsync(id); if (i == null) return Json(new { success = false, message = "Không tìm thấy" }); if (i.Status == StatusEntity.Approved) return Json(new { success = false, message = "Không thể xóa đã duyệt" }); i.Status = StatusEntity.Deleted; i.LastModified = DateTime.Now; i.ModifiedBy = User.Identity?.Name; await _context.SaveChangesAsync(); return Json(new { success = true, message = "Xóa thành công" }); }
    }
}
