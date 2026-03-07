using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using WebsiteSellLaptop.Services;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUpload;

        public BannerController(AppDbContext context, IFileUploadService fileUpload)
        {
            _context = context;
            _fileUpload = fileUpload;
        }

        public async Task<IActionResult> Index(int page = 1, string? keyword = null, int pageSize = 10)
        {
            IQueryable<Banner> query = _context.Banners.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            List<Banner> data = await query.OrderByDescending(x => x.Created).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            Banner? banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return Json(new { success = false, message = "Không tìm thấy banner" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    id = banner.Id,
                    code = banner.Code,
                    title = banner.Title,
                    subTitle = banner.SubTitle,
                    imageUrl = banner.ImageUrl,
                    linkUrl = banner.LinkUrl,
                    sortOrder = banner.SortOrder,
                    status = banner.Status.ToString(),
                    created = banner.Created.ToString("dd/MM/yyyy HH:mm"),
                    lastModified = banner.LastModified.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Vui lòng kiểm tra lại thông tin", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            bool codeExists = await _context.Banners.AnyAsync(x => x.Code.ToLower() == model.Code.Trim().ToLower());
            if (codeExists)
            {
                return Json(new { success = false, message = "Mã banner đã tồn tại" });
            }

            if (imageFile != null)
            {
                try
                {
                    model.ImageUrl = await _fileUpload.UploadImageAsync(imageFile, "uploads/banners");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            model.Code = model.Code.Trim();
            model.CreatedBy = User.Identity?.Name;
            model.Status = StatusEntity.Approved;
            _ = _context.Banners.Add(model);
            _ = await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm banner thành công" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Banner model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Vui lòng kiểm tra lại thông tin", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            Banner? banner = await _context.Banners.FindAsync(model.Id);
            if (banner == null)
            {
                return Json(new { success = false, message = "Không tìm thấy banner" });
            }

            if (banner.Status == StatusEntity.Approved)
            {
                return Json(new { success = false, message = "Không thể sửa banner đã duyệt" });
            }

            bool codeExists = await _context.Banners.AnyAsync(x => x.Code.ToLower() == model.Code.Trim().ToLower() && x.Id != model.Id);
            if (codeExists)
            {
                return Json(new { success = false, message = "Mã banner đã tồn tại" });
            }

            if (imageFile != null)
            {
                try
                {
                    banner.ImageUrl = await _fileUpload.UploadImageAsync(imageFile, "uploads/banners");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }
            else
            {
                banner.ImageUrl = model.ImageUrl;
            }

            banner.Code = model.Code.Trim();
            banner.Title = model.Title;
            banner.SubTitle = model.SubTitle;
            banner.LinkUrl = model.LinkUrl;
            banner.SortOrder = model.SortOrder;
            banner.LastModified = DateTime.Now;
            banner.ModifiedBy = User.Identity?.Name;

            _ = await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật banner thành công" });
        }

        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Approve(Guid id) { Banner? i = await _context.Banners.FindAsync(id); if (i == null) { return Json(new { success = false, message = "Không tìm thấy" }); } i.Status = StatusEntity.Approved; i.LastModified = DateTime.Now; i.ModifiedBy = User.Identity?.Name; _ = await _context.SaveChangesAsync(); return Json(new { success = true, message = "Duyệt thành công" }); }
        [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Reject(Guid id) { Banner? i = await _context.Banners.FindAsync(id); if (i == null) { return Json(new { success = false, message = "Không tìm thấy" }); } i.Status = StatusEntity.Rejected; i.LastModified = DateTime.Now; i.ModifiedBy = User.Identity?.Name; _ = await _context.SaveChangesAsync(); return Json(new { success = true, message = "Hủy duyệt thành công" }); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            Banner? i = await _context.Banners.FindAsync(id);
            if (i == null)
            {
                return Json(new { success = false, message = "Không tìm thấy" });
            }
            _ = _context.Banners.Remove(i);
            _ = await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa thành công" });
        }
    }
}
