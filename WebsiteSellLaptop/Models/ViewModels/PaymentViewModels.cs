namespace WebsiteSellLaptop.Models.ViewModels
{
    public class PaymentInformationModel
    {
        public Guid OrderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OrderDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderType { get; set; } = "order";
    }

    public class PaymentResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? OrderDescription { get; set; }
        public string? TransactionId { get; set; }
        public string? Token { get; set; }
        public string? VnPayResponseCode { get; set; }
        public decimal Amount { get; set; }
        public Guid? OrderId { get; set; }
    }
}
