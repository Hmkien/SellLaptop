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
    public class CouponController : Controller
    {
        private readonly AppDbContext _context;
        public CouponController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            IQueryable<Coupon> query = _context.Coupons.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Code.Contains(keyword) || (x.Description != null && x.Description.Contains(keyword)));
            }

            ViewBag.Keyword = keyword;
            return View(query.OrderByDescending(x => x.Created).ToPagedList(page, 10));
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            Coupon? coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Không tìm thấy mã giảm giá" });
            }

            string statusName = coupon.Status switch
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
                    id = coupon.Id,
                    code = coupon.Code,
                    description = coupon.Description,
                    discountAmount = coupon.DiscountAmount,
                    isPercent = coupon.IsPercent,
                    minOrderAmount = coupon.MinOrderAmount,
                    maxUsage = coupon.MaxUsage,
                    usedCount = coupon.UsedCount,
                    startDate = coupon.StartDate?.ToString("yyyy-MM-dd"),
                    endDate = coupon.EndDate?.ToString("yyyy-MM-dd"),
                    statusName,
                    created = coupon.Created.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Code))
                {
                    return Json(new { success = false, message = "Mã giảm giá không được để trống" });
                }

                if (_context.Coupons.Any(x => x.Code == model.Code))
                {
                    return Json(new { success = false, message = "Mã giảm giá đã tồn tại" });
                }

                if (model.DiscountAmount <= 0)
                {
                    return Json(new { success = false, message = "Giá trị giảm phải lớn hơn 0" });
                }

                if (model.IsPercent && model.DiscountAmount > 100)
                {
                    return Json(new { success = false, message = "Phần trăm giảm không được vượt quá 100%" });
                }

                if (model.EndDate.HasValue && model.StartDate.HasValue && model.EndDate < model.StartDate)
                {
                    return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu" });
                }

                model.Status = StatusEntity.Draft;
                model.UsedCount = 0;
                _ = _context.Coupons.Add(model);
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm mã giảm giá thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string code, string? description, decimal discountAmount, bool isPercent, decimal? minOrderAmount, int? maxUsage, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                Coupon? coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy mã giảm giá" });
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    return Json(new { success = false, message = "Mã giảm giá không được để trống" });
                }

                if (_context.Coupons.Any(x => x.Code == code && x.Id != id))
                {
                    return Json(new { success = false, message = "Mã giảm giá đã tồn tại" });
                }

                if (discountAmount <= 0)
                {
                    return Json(new { success = false, message = "Giá trị giảm phải lớn hơn 0" });
                }

                if (isPercent && discountAmount > 100)
                {
                    return Json(new { success = false, message = "Phần trăm giảm không được vượt quá 100%" });
                }

                if (endDate.HasValue && startDate.HasValue && endDate < startDate)
                {
                    return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu" });
                }

                coupon.Code = code;
                coupon.Description = description;
                coupon.DiscountAmount = discountAmount;
                coupon.IsPercent = isPercent;
                coupon.MinOrderAmount = minOrderAmount;
                coupon.MaxUsage = maxUsage;
                coupon.StartDate = startDate;
                coupon.EndDate = endDate;
                coupon.LastModified = DateTime.Now;

                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật mã giảm giá thành công" });
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
                Coupon? coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy mã giảm giá" });
                }

                coupon.Status = StatusEntity.Approved;
                coupon.LastModified = DateTime.Now;
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Duyệt mã giảm giá thành công" });
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
                Coupon? coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy mã giảm giá" });
                }

                coupon.Status = StatusEntity.Rejected;
                coupon.LastModified = DateTime.Now;
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Hủy duyệt mã giảm giá thành công" });
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
                Coupon? coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy mã giảm giá" });
                }
                _ = _context.Coupons.Remove(coupon);
                _ = await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa mã giảm giá thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}
