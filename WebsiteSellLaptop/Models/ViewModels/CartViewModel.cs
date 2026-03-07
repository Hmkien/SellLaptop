namespace WebsiteSellLaptop.Models.ViewModels
{
    public class CartItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Specifications { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.Subtotal);
        public decimal ShippingFee { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public string? CouponCode { get; set; }
        public decimal FinalAmount => TotalAmount + ShippingFee - DiscountAmount;
        public int TotalItems => Items.Sum(x => x.Quantity);
    }
}
