using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteSellLaptop.Models.Entities;

namespace WebsiteSellLaptop.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Role> AppRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables
            builder.Entity<AppUser>(e => e.ToTable("Users"));

            // Role code unique
            builder.Entity<Role>()
                .HasIndex(r => r.Code)
                .IsUnique();

            // Brand code unique
            builder.Entity<Brand>()
                .HasIndex(b => b.Code)
                .IsUnique();

            // Category code unique
            builder.Entity<Category>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Product code unique
            builder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // Order code unique
            builder.Entity<Order>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            // Coupon code unique
            builder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Product -> Category
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> Brand
            builder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
