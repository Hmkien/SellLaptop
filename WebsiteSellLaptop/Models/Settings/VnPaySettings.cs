namespace WebsiteSellLaptop.Models.Settings
{
    public class VnPaySettings
    {
        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Command { get; set; } = "pay";
        public string CurrCode { get; set; } = "VND";
        public string Version { get; set; } = "2.1.0";
        public string Locale { get; set; } = "vn";
        public string PaymentBackReturnUrl { get; set; } = string.Empty;
        public string TimeZoneId { get; set; } = "SE Asia Standard Time";
    }
}
