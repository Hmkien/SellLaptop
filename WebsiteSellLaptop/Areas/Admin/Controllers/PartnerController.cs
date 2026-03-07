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
    public class PartnerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public PartnerController(AppDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            IQueryable<Partner> query = _context.Partners.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }

            ViewBag.Keyword = keyword;
            return View(query.OrderByDescending(x => x.Created).ToPagedList(page, 10));
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            Partner? partner = await _context.Partners.FindAsync(id);
            if (partner == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đối tác" });
            }

            string statusName = partner.Status switch
            {
                StatusEntity.Approved => "Đã duyệt",
                StatusEntity.Pending => "Chờ duyệt",
                StatusEntity.Rejected => "Hủy duyệt",
                StatusEntity.Draft => "Nháp",
                _ => "Không xác định"
            };

            return Json(new
            {
                success = true,
                data = new
                {
                    id = partner.Id,
                    name = partner.Name,
                    description = partner.Description,
                    logoUrl = partner.LogoUrl,
                    website = partner.Website,
                    sortOrder = partner.SortOrder,
                    statusName,
                    created = partner.Created.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Partner model, IFormFile? logoFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Tên đối tác không được để trống" });
                }

                if (logoFile != null && logoFile.Length > 0)
                {
                    model.LogoUrl = await _fileUploadService.UploadImageAsync(logoFile, "uploads/partners");
                }

                model.Status = StatusEntity.Draft;
                _ = _context.Partners.Add(model);
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm mới đối tác thành công" });
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(model.LogoUrl))
                {
                    _ = _fileUploadService.DeleteImage(model.LogoUrl);
                }

                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string name, string? website, string? description, int sortOrder, string? logoUrl, IFormFile? logoFile)
        {
            try
            {
                Partner? partner = await _context.Partners.FindAsync(id);
                if (partner == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đối tác" });
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { success = false, message = "Tên đối tác không được để trống" });
                }

                string? oldLogoUrl = partner.LogoUrl;

                if (logoFile != null && logoFile.Length > 0)
                {
                    partner.LogoUrl = await _fileUploadService.UploadImageAsync(logoFile, "uploads/partners");
                    if (!string.IsNullOrEmpty(oldLogoUrl))
                    {
                        _ = _fileUploadService.DeleteImage(oldLogoUrl);
                    }
                }

                partner.Name = name;
                partner.Website = website;
                partner.Description = description;
                partner.SortOrder = sortOrder;
                partner.LastModified = DateTime.Now;

                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật đối tác thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                Partner? partner = await _context.Partners.FindAsync(id);
                if (partner == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đối tác" });
                }

                partner.Status = StatusEntity.Approved;
                partner.LastModified = DateTime.Now;
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Duyệt đối tác thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            try
            {
                Partner? partner = await _context.Partners.FindAsync(id);
                if (partner == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đối tác" });
                }

                partner.Status = StatusEntity.Rejected;
                partner.LastModified = DateTime.Now;
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Hủy duyệt đối tác thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                Partner? partner = await _context.Partners.FindAsync(id);
                if (partner == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đối tác" });
                }

                if (!string.IsNullOrEmpty(partner.LogoUrl))
                {
                    _ = _fileUploadService.DeleteImage(partner.LogoUrl);
                }
                _ = _context.Partners.Remove(partner);
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa đối tác thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}
