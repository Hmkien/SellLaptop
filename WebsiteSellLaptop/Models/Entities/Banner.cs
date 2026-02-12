using System.ComponentModel.DataAnnotations;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Banner : BaseEntity
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? SubTitle { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? LinkUrl { get; set; }

        public int SortOrder { get; set; }
    }
}
