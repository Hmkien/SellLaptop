using WebsiteSellLaptop.Models.ViewModels;

namespace WebsiteSellLaptop.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
