using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;
using X.PagedList;
using X.PagedList.Extensions;

namespace WebsiteSellLaptop.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;

        public BlogController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 9)
        {
            List<Blog> blogs = await _context.Blogs
                .Where(b => b.Status == StatusEntity.Approved)
                .OrderByDescending(b => b.Created)
                .ToListAsync();

            IPagedList<Blog> pagedBlogs = blogs.ToPagedList(page, pageSize);

            return View(pagedBlogs);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            Blog? blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && b.Status == StatusEntity.Approved);

            if (blog == null)
            {
                return NotFound();
            }

            // Get related blogs
            List<Blog> relatedBlogs = await _context.Blogs
                .Where(b => b.Id != id && b.Status == StatusEntity.Approved)
                .OrderByDescending(b => b.Created)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedBlogs = relatedBlogs;

            return View(blog);
        }
    }
}
