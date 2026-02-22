using System.ComponentModel.DataAnnotations;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Role : BaseEntity
    {
        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public UserRole RoleType { get; set; }

        public int SortOrder { get; set; }
    }
}
