using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Product : BaseEntity
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(300)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Slug { get; set; }

        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        // Specs
        [StringLength(100)]
        public string? CPU { get; set; }

        [StringLength(100)]
        public string? RAM { get; set; }

        [StringLength(100)]
        public string? Storage { get; set; }

        [StringLength(100)]
        public string? Screen { get; set; }

        [StringLength(100)]
        public string? GPU { get; set; }

        [StringLength(100)]
        public string? Battery { get; set; }

        [StringLength(100)]
        public string? Weight { get; set; }

        [StringLength(100)]
        public string? OS { get; set; }

        public bool IsFeatured { get; set; }

        // FK
        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }

        // Navigation
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
