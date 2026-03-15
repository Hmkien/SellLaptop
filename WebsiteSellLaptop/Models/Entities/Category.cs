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

        // CategoryType: 0 = Laptop, 1 = Accessory (Phụ kiện)
        // Keep SortOrder for ordering; expose IsAccessory for easier binding in UI
        public int CategoryType => SortOrder;

        // For admin forms it's more convenient to use a boolean checkbox.
        // Map IsAccessory to SortOrder: true => 1, false => 0
        public bool IsAccessory
        {
            get => SortOrder == 1;
            set => SortOrder = value ? 1 : 0;
        }

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
