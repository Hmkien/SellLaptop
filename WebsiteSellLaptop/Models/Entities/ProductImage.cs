using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellLaptop.Models.Entities
{
    public class ProductImage : BaseEntity
    {
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsMain { get; set; }

        // FK
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
