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
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? keyword = null, OrderStatus? status = null, int pageSize = 10)
        {
            IQueryable<Order> query = _context.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x => x.OrderCode.ToLower().Contains(keyword) || x.FullName.ToLower().Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.OrderStatus == status.Value);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            List<Order> data = await query.OrderByDescending(x => x.Created).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.StatusFilter = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(data);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            Order? item = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status)
        {
            Order? item = await _context.Orders.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            item.OrderStatus = status;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();

            string message = status switch
            {
                OrderStatus.Confirmed => "Xác nhận đơn hàng thành công",
                OrderStatus.Shipping => "Đơn hàng đang giao",
                OrderStatus.Completed => "Hoàn thành đơn hàng",
                OrderStatus.Cancelled => "Đã hủy đơn hàng",
                _ => "Cập nhật trạng thái thành công"
            };

            return Json(new { success = true, message });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            Order? item = await _context.Orders.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            item.Status = StatusEntity.Approved;
            item.OrderStatus = OrderStatus.Confirmed;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Duyệt thành công" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            Order? item = await _context.Orders.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            item.Status = StatusEntity.Rejected;
            item.OrderStatus = OrderStatus.Cancelled;
            item.LastModified = DateTime.Now;
            item.ModifiedBy = User.Identity?.Name;
            _ = await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Hủy duyệt thành công" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            Order? item = await _context.Orders.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            if (item.Status == StatusEntity.Approved && item.OrderStatus != OrderStatus.Completed && item.OrderStatus != OrderStatus.Cancelled)
            {
                return Json(new { success = false, message = "Không thể xóa đơn hàng đang xử lý" });
            }
            _ = _context.Orders.Remove(item);
            _ = await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công" });
        }
    }
}
