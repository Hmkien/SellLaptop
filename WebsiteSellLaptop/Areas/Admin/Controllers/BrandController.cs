using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Reflection;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using WebsiteSellLaptop.Services;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUpload;

        public BrandController(AppDbContext context, IFileUploadService fileUpload)
        {
            _context = context;
            _fileUpload = fileUpload;
        }

        #region Index - Danh sách
        public async Task<IActionResult> Index(int page = 1, string? keyword = null, int pageSize = 10)
        {
            IQueryable<Brand> query = _context.Brands.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            List<Brand> data = await query
                .OrderByDescending(x => x.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(data);
        }
        #endregion

        #region GetById - Lấy chi tiết để hiển thị modal
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            Brand? item = await _context.Brands.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    item.Id,
                    item.Code,
                    item.Name,
                    item.Description,
                    item.LogoUrl,
                    item.Website,
                    item.SortOrder,
                    item.Status,
                    StatusName = GetEnumDescription(item.Status),
                    Created = item.Created.ToString("dd/MM/yyyy HH:mm"),
                    LastModified = item.LastModified.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }
        #endregion

        #region Create - Thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Brand model, IFormFile? logoFile)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                return Json(new { success = false, message = "Mã thương hiệu không được để trống" });
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Tên thương hiệu không được để trống" });
            }

            // Chuẩn hóa Code
            string normalizedCode = model.Code.Trim().ToUpper();

            // Kiểm tra trùng mã
            bool codeExists = await _context.Brands
                .AnyAsync(x => x.Code.ToUpper() == normalizedCode);

            if (codeExists)
            {
                return Json(new { success = false, message = $"Mã thương hiệu '{normalizedCode}' đã tồn tại trong hệ thống" });
            }

            // Upload logo if provided
            string? logoUrl = null;
            if (logoFile != null)
            {
                try
                {
                    logoUrl = await _fileUpload.UploadImageAsync(logoFile, "uploads/brands");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Lỗi upload logo: {ex.Message}" });
                }
            }

            Brand entity = new()
            {
                Code = normalizedCode,
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                LogoUrl = logoUrl ?? model.LogoUrl?.Trim(),
                Website = model.Website?.Trim(),
                SortOrder = model.SortOrder,
                Status = StatusEntity.Approved,
                CreatedBy = User.Identity?.Name
            };

            try
            {
                _ = _context.Brands.Add(entity);
                _ = await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm mới thương hiệu thành công" });
            }
            catch (DbUpdateException ex)
            {
                // Xóa logo đã upload nếu có lỗi
                if (!string.IsNullOrEmpty(logoUrl))
                {
                    _ = _fileUpload.DeleteImage(logoUrl);
                }

                if (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    return Json(new { success = false, message = $"Mã thương hiệu '{normalizedCode}' đã tồn tại. Vui lòng chọn mã khác." });
                }

                return Json(new { success = false, message = $"Lỗi lưu dữ liệu: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }
        #endregion

        #region Edit - Cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] Brand model, IFormFile? logoFile)
        {
            Brand? item = await _context.Brands.FindAsync(model.Id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu" });
            }

            if (item.Status == StatusEntity.Approved)
            {
                return Json(new { success = false, message = "Không thể sửa bản ghi đã duyệt" });
            }

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                return Json(new { success = false, message = "Mã thương hiệu không được để trống" });
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Tên thương hiệu không được để trống" });
            }

            // Chuẩn hóa Code
            string normalizedCode = model.Code.Trim().ToUpper();

            // Kiểm tra trùng mã (loại trừ bản ghi hiện tại)
            bool codeExists = await _context.Brands
                .AnyAsync(x => x.Code.ToUpper() == normalizedCode
                            && x.Id != model.Id
                           );

            if (codeExists)
            {
                return Json(new { success = false, message = $"Mã thương hiệu '{normalizedCode}' đã tồn tại trong hệ thống" });
            }

            string? newLogoUrl = null;

            // Upload new logo if provided
            if (logoFile != null)
            {
                try
                {
                    newLogoUrl = await _fileUpload.UploadImageAsync(logoFile, "uploads/brands");

                    // Delete old logo only after successful upload
                    if (!string.IsNullOrEmpty(item.LogoUrl))
                    {
                        _ = _fileUpload.DeleteImage(item.LogoUrl);
                    }

                    item.LogoUrl = newLogoUrl;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Lỗi upload logo: {ex.Message}" });
                }
            }

            item.Code = normalizedCode;
            item.Name = model.Name.Trim();
            item.Description = model.Description?.Trim();
            item.Website = model.Website?.Trim();

            // Chỉ update LogoUrl nếu không upload file mới
            if (logoFile == null && !string.IsNullOrEmpty(model.LogoUrl))
            {
                item.LogoUrl = model.LogoUrl.Trim();
            }

            item.SortOrder = model.SortOrder;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;

            try
            {
                _ = await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật thương hiệu thành công" });
            }
            catch (DbUpdateException ex)
            {
                // Rollback: xóa logo mới upload nếu có lỗi
                if (!string.IsNullOrEmpty(newLogoUrl))
                {
                    _ = _fileUpload.DeleteImage(newLogoUrl);
                }

                if (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    return Json(new { success = false, message = $"Mã thương hiệu '{normalizedCode}' đã tồn tại. Vui lòng chọn mã khác." });
                }

                return Json(new { success = false, message = $"Lỗi lưu dữ liệu: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }
        #endregion

        #region Approve - Duyệt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            Brand? item = await _context.Brands.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu" });
            }

            item.Status = StatusEntity.Approved;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Duyệt thành công" });
        }
        #endregion

        #region Reject - Hủy duyệt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            Brand? item = await _context.Brands.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu" });
            }

            item.Status = StatusEntity.Rejected;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Hủy duyệt thành công" });
        }
        #endregion

        #region Delete - Xóa mềm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            Brand? item = await _context.Brands.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu" });
            }

            if (item.Status == StatusEntity.Approved)
            {
                return Json(new { success = false, message = "Không thể xóa bản ghi đã duyệt" });
            }
            _ = _context.Brands.Remove(item);
            _ = await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công" });
        }
        #endregion

        #region Helper - Lấy Description từ enum
        private static string GetEnumDescription(Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            DescriptionAttribute? attr = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
            return attr?.Description ?? value.ToString();
        }
        #endregion
    }
}
