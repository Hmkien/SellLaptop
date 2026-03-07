using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using WebsiteSellLaptop.Services;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUpload;

        public ProductController(AppDbContext context, IFileUploadService fileUpload)
        {
            _context = context;
            _fileUpload = fileUpload;
        }

        public async Task<IActionResult> Index(int page = 1, string? keyword = null, int pageSize = 10)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.Category).Include(p => p.Brand)
                .AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            List<Product> data = await query.OrderByDescending(x => x.Created).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

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
        public async Task<IActionResult> Create(Product model, IFormFile? thumbnailFile, List<IFormFile>? productImages)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(model); }

            // Check duplicate code
            bool codeExists = await _context.Products.AnyAsync(x => x.Code.ToLower() == model.Code.Trim().ToLower());
            if (codeExists) { ModelState.AddModelError("Code", "Mã sản phẩm đã tồn tại"); LoadDropdowns(); return View(model); }

            List<string> uploadedImages = [];

            try
            {
                // Upload thumbnail
                if (thumbnailFile != null)
                {
                    model.ThumbnailUrl = await _fileUpload.UploadImageAsync(thumbnailFile, "uploads/products");
                    uploadedImages.Add(model.ThumbnailUrl);
                }

                model.Code = model.Code.Trim();
                model.Slug = model.Name.ToLower().Replace(" ", "-");
                model.CreatedBy = User.Identity?.Name;

                // Thêm sản phẩm vào context trước
                _ = _context.Products.Add(model);

                // Upload nhiều ảnh chi tiết và tạo ProductImage entities
                if (productImages != null && productImages.Count > 0)
                {
                    int sortOrder = 1;
                    foreach (IFormFile imageFile in productImages)
                    {
                        string imageUrl = await _fileUpload.UploadImageAsync(imageFile, "uploads/products");
                        uploadedImages.Add(imageUrl);

                        // Tạo ProductImage entity
                        ProductImage productImage = new()
                        {
                            ProductId = model.Id,
                            ImageUrl = imageUrl,
                            SortOrder = sortOrder++,
                            IsMain = false,
                            CreatedBy = User.Identity?.Name
                        };
                        _ = _context.ProductImages.Add(productImage);
                    }
                }

                _ = await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Rollback: Xóa tất cả ảnh đã upload nếu có lỗi
                foreach (string imageUrl in uploadedImages)
                {
                    _ = _fileUpload.DeleteImage(imageUrl);
                }

                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                LoadDropdowns();
                return View(model);
            }
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            Product? item = await _context.Products.Include(p => p.Category).Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == id);
            return item == null ? NotFound() : View(item);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            Product? item = await _context.Products.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            LoadDropdowns();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model)
        {
            if (!ModelState.IsValid) { LoadDropdowns(); return View(model); }
            Product? item = await _context.Products.FindAsync(model.Id);
            if (item == null)
            {
                return NotFound();
            }

            if (item.Status == StatusEntity.Approved) { ModelState.AddModelError("", "Không thể sửa sản phẩm đã duyệt"); LoadDropdowns(); return View(model); }

            // Check duplicate code
            bool codeExists = await _context.Products.AnyAsync(x => x.Code.ToLower() == model.Code.Trim().ToLower() && x.Id != model.Id);
            if (codeExists) { ModelState.AddModelError("Code", "Mã sản phẩm đã tồn tại"); LoadDropdowns(); return View(model); }

            item.Code = model.Code.Trim();
            item.Name = model.Name; item.ShortDescription = model.ShortDescription; item.Description = model.Description;
            item.Price = model.Price; item.DiscountPrice = model.DiscountPrice; item.StockQuantity = model.StockQuantity;
            item.ThumbnailUrl = model.ThumbnailUrl; item.CPU = model.CPU; item.RAM = model.RAM;
            item.Storage = model.Storage; item.Screen = model.Screen; item.GPU = model.GPU;
            item.Battery = model.Battery; item.Weight = model.Weight; item.OS = model.OS;
            item.IsFeatured = model.IsFeatured; item.IsAccessory = model.IsAccessory; item.CategoryId = model.CategoryId; item.BrandId = model.BrandId;
            item.Slug = model.Name.ToLower().Replace(" ", "-");
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            Product? item = await _context.Products.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            item.Status = StatusEntity.Approved;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Duyệt thành công" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            Product? item = await _context.Products.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            item.Status = StatusEntity.Rejected;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Hủy duyệt thành công" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            Product? item = await _context.Products.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            if (item.Status == StatusEntity.Approved)
            {
                return Json(new { success = false, message = "Không thể xóa sản phẩm đã duyệt" });
            }
            _ = _context.Products.Remove(item);
            _ = await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa thành công" });
        }
    }
}
