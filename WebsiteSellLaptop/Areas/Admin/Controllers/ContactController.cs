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
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, string? keyword = null)
        {
            IQueryable<Contact> query = _context.Contacts.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.FullName.Contains(keyword) || x.Subject.Contains(keyword));
            }

            ViewBag.Keyword = keyword;
            return View(query.OrderByDescending(x => x.Created).ToPagedList(page, 10));
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            Contact? i = await _context.Contacts.FindAsync(id);
            if (i == null)
            {
                return NotFound();
            }

            if (!i.IsRead) { i.IsRead = true; _ = await _context.SaveChangesAsync(); }
            return View(i);
        }

        [HttpPost] public async Task<IActionResult> Approve(Guid id) { Contact? i = await _context.Contacts.FindAsync(id); if (i == null) { return NotFound(); } i.Status = StatusEntity.Approved; i.LastModified = DateTime.Now; _ = await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost] public async Task<IActionResult> Reject(Guid id) { Contact? i = await _context.Contacts.FindAsync(id); if (i == null) { return NotFound(); } i.Status = StatusEntity.Rejected; i.LastModified = DateTime.Now; _ = await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            Contact? i = await _context.Contacts.FindAsync(id);
            if (i == null) { return NotFound(); }
            _ = _context.Contacts.Remove(i);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
