using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Data;
using WebsiteSellLaptop.Models.Entities;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? keyword,
            string? brand,
            string? category,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page = 1,
            int pageSize = 9)
        {
            // Query sản phẩm đã duyệt
            IQueryable<Product> query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.Status == StatusEntity.Approved && !p.IsAccessory)
                .AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword) ||
                                       (p.Description != null && p.Description.Contains(keyword)) ||
                                       p.CPU.Contains(keyword));
            }

            // Filter by brand
            if (!string.IsNullOrEmpty(brand))
            {
                List<Guid> brandIds = brand.Split(',').Select(Guid.Parse).ToList();
                query = query.Where(p => brandIds.Contains(p.BrandId));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                List<Guid> categoryIds = category.Split(',').Select(Guid.Parse).ToList();
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Filter by price range
            if (minPrice.HasValue && minPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Sort
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "bestseller" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Created),
                _ => query.OrderByDescending(p => p.Created) // Mới nhất
            };

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            List<Product> products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get brands and categories for filter
            List<Brand> brands = await _context.Brands
                .Where(b => b.Status == StatusEntity.Approved)
                .OrderBy(b => b.Name)
                .ToListAsync();

            List<Category> categories = await _context.Categories
                .Where(c => c.Status == StatusEntity.Approved)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Pass data to view
            ViewBag.Products = products;
            ViewBag.Brands = brands;
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.SelectedBrand = brand;
            ViewBag.SelectedCategory = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;

            return View();
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            Product? product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == StatusEntity.Approved);

            if (product == null)
            {
                return NotFound();
            }

            // Get related products (same category)
            List<Product> relatedProducts = await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == product.CategoryId &&
                           p.Id != id &&
                           p.Status == StatusEntity.Approved)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        public async Task<IActionResult> Accessories(string? search, string? brand, string? category,
            decimal? minPrice, decimal? maxPrice, string? sort, int page = 1)
        {
            ViewData["Title"] = "Phụ kiện Laptop";
            ViewBag.IsAccessory = true;

            IQueryable<Product> query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.Status == StatusEntity.Approved && p.IsAccessory)
                .AsQueryable();

            // Apply text search only when provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            }

            // brand and category parameters are comma-separated ids (same as Index)
            if (!string.IsNullOrEmpty(brand))
            {
                List<Guid> brandIds = brand.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList();
                query = query.Where(p => brandIds.Contains(p.BrandId));
            }

            if (!string.IsNullOrEmpty(category))
            {
                List<Guid> categoryIds = category.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList();
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (minPrice.HasValue && minPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "bestseller" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Created),
                _ => query.OrderByDescending(p => p.Created)
            };

            int pageSize = 12;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            List<Product> products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Brands = await _context.Brands.Where(b => b.Status == StatusEntity.Approved).ToListAsync();
            // Show only accessory categories on accessories page
            ViewBag.Categories = await _context.Categories.Where(c => c.Status == StatusEntity.Approved && c.SortOrder == 1).ToListAsync();
            ViewBag.Search = search;
            // Index view expects selected brand/category as comma-separated strings
            ViewBag.SelectedBrand = !string.IsNullOrEmpty(brand) ? brand : string.Empty;
            ViewBag.SelectedCategory = !string.IsNullOrEmpty(category) ? category : string.Empty;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;

            // Pass products via ViewBag (Index.cshtml reads ViewBag.Products)
            ViewBag.Products = products;

            // If AJAX request, return partial grid only
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductGridPartial", products);
            }

            return View("Index");
        }
    }
}
