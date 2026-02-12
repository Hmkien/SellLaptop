using System.ComponentModel.DataAnnotations;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(200)]
        public string? Slug { get; set; }

        public string? ImageUrl { get; set; }

        public int SortOrder { get; set; }

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
