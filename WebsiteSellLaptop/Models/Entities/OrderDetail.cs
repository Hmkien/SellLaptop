using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellLaptop.Models.Entities
{
    public class OrderDetail
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalPrice { get; set; }

        // FK
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
