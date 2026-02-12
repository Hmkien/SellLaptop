using System.ComponentModel.DataAnnotations;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Partner : BaseEntity
    {
        [Required(ErrorMessage = "Tên đối tác không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        public int SortOrder { get; set; }
    }
}
