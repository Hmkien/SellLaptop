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
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUpload;

        public CategoryController(AppDbContext context, IFileUploadService fileUpload)
        {
            _context = context;
            _fileUpload = fileUpload;
        }

        #region Index - Danh sách
        public async Task<IActionResult> Index(int page = 1, string? keyword = null, int pageSize = 10)
        {
            IQueryable<Category> query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            List<Category> data = await query
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
            Category? item = await _context.Categories.FindAsync(id);
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
                    item.Slug,
                    item.ImageUrl,
                    item.SortOrder,
                    item.IsAccessory,
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
        public async Task<IActionResult> Create([FromForm] Category model, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                return Json(new { success = false, message = "Mã danh mục không được để trống" });
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Tên danh mục không được để trống" });
            }

            // Chuẩn hóa Code
            string normalizedCode = model.Code.Trim().ToUpper();

            // Kiểm tra trùng mã - dùng ToUpper để so sánh không phân biệt hoa thường
            bool codeExists = await _context.Categories
                .AnyAsync(x => x.Code.ToUpper() == normalizedCode);

            if (codeExists)
            {
                return Json(new { success = false, message = $"Mã danh mục '{normalizedCode}' đã tồn tại trong hệ thống" });
            }

            // Upload image if provided
            string? imageUrl = null;
            if (imageFile != null)
            {
                try
                {
                    imageUrl = await _fileUpload.UploadImageAsync(imageFile, "uploads/categories");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Lỗi upload ảnh: {ex.Message}" });
                }
            }

            Category entity = new()
            {
                Code = normalizedCode, // Lưu Code đã chuẩn hóa
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Slug = model.Name.Trim().ToLower().Replace(" ", "-"),
                ImageUrl = imageUrl ?? model.ImageUrl?.Trim(),
                // Use boolean IsAccessory to determine SortOrder (0 = Laptop, 1 = Accessory)
                SortOrder = model.IsAccessory ? 1 : 0,
                CreatedBy = User.Identity?.Name
            };

            try
            {
                _ = _context.Categories.Add(entity);
                _ = await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm mới danh mục thành công" });
            }
            catch (DbUpdateException ex)
            {
                // Bắt lỗi duplicate key từ database
                if (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    // Xóa ảnh đã upload nếu có lỗi
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        _ = _fileUpload.DeleteImage(imageUrl);
                    }

                    return Json(new { success = false, message = $"Mã danh mục '{normalizedCode}' đã tồn tại. Vui lòng chọn mã khác." });
                }

                // Xóa ảnh đã upload nếu có lỗi khác
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    _ = _fileUpload.DeleteImage(imageUrl);
                }

                return Json(new { success = false, message = $"Lỗi lưu dữ liệu: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }
        #endregion

        #region Edit - Cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] Category model, IFormFile? imageFile)
        {
            Category? item = await _context.Categories.FindAsync(model.Id);
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
                return Json(new { success = false, message = "Mã danh mục không được để trống" });
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Tên danh mục không được để trống" });
            }

            // Chuẩn hóa Code
            string normalizedCode = model.Code.Trim().ToUpper();

            // Kiểm tra trùng mã (loại trừ bản ghi hiện tại)
            bool codeExists = await _context.Categories
                .AnyAsync(x => x.Code.ToUpper() == normalizedCode
                            && x.Id != model.Id);

            if (codeExists)
            {
                return Json(new { success = false, message = $"Mã danh mục '{normalizedCode}' đã tồn tại trong hệ thống" });
            }

            string? newImageUrl = null;

            // Upload new image if provided
            if (imageFile != null)
            {
                try
                {
                    newImageUrl = await _fileUpload.UploadImageAsync(imageFile, "uploads/categories");

                    // Delete old image only after successful upload
                    if (!string.IsNullOrEmpty(item.ImageUrl))
                    {
                        _ = _fileUpload.DeleteImage(item.ImageUrl);
                    }

                    item.ImageUrl = newImageUrl;
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Lỗi upload ảnh: {ex.Message}" });
                }
            }

            item.Code = normalizedCode;
            item.Name = model.Name.Trim();
            item.Description = model.Description?.Trim();
            item.Slug = model.Name.Trim().ToLower().Replace(" ", "-");

            // Chỉ update ImageUrl nếu không upload file mới
            if (imageFile == null && !string.IsNullOrEmpty(model.ImageUrl))
            {
                item.ImageUrl = model.ImageUrl.Trim();
            }

            // Map boolean IsAccessory to SortOrder
            item.SortOrder = model.IsAccessory ? 1 : 0;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;

            try
            {
                _ = await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật danh mục thành công" });
            }
            catch (DbUpdateException ex)
            {
                // Rollback: xóa ảnh mới upload nếu có lỗi
                if (!string.IsNullOrEmpty(newImageUrl))
                {
                    _ = _fileUpload.DeleteImage(newImageUrl);
                }

                if (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    return Json(new { success = false, message = $"Mã danh mục '{normalizedCode}' đã tồn tại. Vui lòng chọn mã khác." });
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
            Category? item = await _context.Categories.FindAsync(id);
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
            Category? item = await _context.Categories.FindAsync(id);
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

        #region Delete - Xóa vĩnh viễn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            Category? item = await _context.Categories.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy danh mục" });
            }

            // Kiểm tra xem có sản phẩm nào thuộc danh mục này không
            bool hasProducts = await _context.Products
                .AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                int productCount = await _context.Products
                    .CountAsync(p => p.CategoryId == id);

                return Json(new
                {
                    success = false,
                    message = $"Không thể xóa danh mục '{item.Name}' vì đang có {productCount} sản phẩm. Vui lòng xóa hoặc chuyển sản phẩm sang danh mục khác trước."
                });
            }

            try
            {
                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    _ = _fileUpload.DeleteImage(item.ImageUrl);
                }

                // Xóa vĩnh viễn khỏi database
                _ = _context.Categories.Remove(item);
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã xóa danh mục '{item.Name}' vĩnh viễn khỏi hệ thống" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi khi xóa: {ex.InnerException?.Message ?? ex.Message}"
                });
            }
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
