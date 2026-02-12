using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteSellLaptop.Models.Enums;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Order : BaseEntity
    {
        [StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; } = string.Empty;

        public string? Note { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? DiscountAmount { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }

        // FK
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
