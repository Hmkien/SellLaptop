using System.ComponentModel.DataAnnotations;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Blog : BaseEntity
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Slug { get; set; }

        public string? Summary { get; set; }

        public string? Content { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public int ViewCount { get; set; }
    }
}
