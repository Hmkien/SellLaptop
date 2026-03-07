using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Controllers
{
    public class PartnerController : Controller
    {
        private readonly AppDbContext _context;

        public PartnerController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var partners = await _context.Partners
                .Where(p => p.Status == StatusEntity.Approved)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(partners);
        }
    }
}
