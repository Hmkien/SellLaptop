using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellLaptop.Models.Entities
{
    public class Coupon : BaseEntity
    {
        [Required(ErrorMessage = "Mã giảm giá không được để trống")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal DiscountAmount { get; set; }

        public bool IsPercent { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? MinOrderAmount { get; set; }

        public int? MaxUsage { get; set; }

        public int UsedCount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
