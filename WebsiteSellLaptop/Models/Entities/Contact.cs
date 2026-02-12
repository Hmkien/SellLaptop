using System.ComponentModel.DataAnnotations;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Contact : BaseEntity
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(300)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }
    }
}
