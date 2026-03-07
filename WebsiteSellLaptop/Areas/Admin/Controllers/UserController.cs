using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;

namespace WebsiteSellLaptop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            int pageSize = 15;
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword) || u.PhoneNumber.Contains(keyword));
                ViewBag.Keyword = keyword;
            }

            var totalItems = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get roles for each user
            var usersWithRoles = new List<(AppUser User, string Role)>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add((user, roles.FirstOrDefault() ?? "User"));
            }

            ViewBag.UsersWithRoles = usersWithRoles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(users);
        }

        // GET: Admin/User/Detail/5
        public async Task<IActionResult> Detail(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.Created)
                .Take(10)
                .ToListAsync();
            ViewBag.Orders = orders;

            return View(user);
        }

        // GET: Admin/User/GetById
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            var roles = await _userManager.GetRolesAsync(user);
            var isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now;

            return Json(new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    role = roles.FirstOrDefault() ?? "User",
                    isLocked = isLocked,
                    created = user.Created
                }
            });
        }

        // POST: Admin/User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string fullName, string? phoneNumber)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            // Check if admin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return Json(new { success = false, message = "Không thể sửa thông tin tài khoản Admin" });

            if (string.IsNullOrWhiteSpace(fullName))
                return Json(new { success = false, message = "Họ tên không được để trống" });

            user.FullName = fullName.Trim();
            user.PhoneNumber = phoneNumber?.Trim();
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã cập nhật thông tin người dùng" });
        }

        // POST: Admin/User/ToggleLockout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            if (user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.Now)
            {
                // Lock user
                user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã khóa tài khoản", isLocked = true });
            }
            else
            {
                // Unlock user
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã mở khóa tài khoản", isLocked = false });
            }
        }

        // POST: Admin/User/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            // Don't allow deleting yourself
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
                return Json(new { success = false, message = "Không thể xóa chính mình" });

            // Check if admin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return Json(new { success = false, message = "Không thể xóa tài khoản Admin" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa người dùng" });
        }

        // POST: Admin/User/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            // Check if admin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return Json(new { success = false, message = "Không thể đổi mật khẩu tài khoản Admin" });

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                return Json(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự" });

            // Remove old password and set new one
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
                return Json(new { success = true, message = "Đã đổi mật khẩu thành công" });
            else
                return Json(new { success = false, message = "Lỗi: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
        }
    }
}
