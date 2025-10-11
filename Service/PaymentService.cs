using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using Common.Constants;
using Common.Utils;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Http;
using Service.VnPay;

namespace Service
{
    public class PaymentService
    {
        private readonly VnPayApiService _apiService;
        private readonly VnPayApiRequest _vnpApiRequest;

        public PaymentService(VnPayApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<BaseResponse<PaymentResponse>> CreatePaymentUrlAsync(HttpContext context, PaymentRequest req)
        {
            try
            {
                var request = VnPayApiRequest.Builder()
                    .BaseUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html")
                    .AddParameter("vnp_Version", "2.1.0")
                    .AddParameter("vnp_Command", "pay")
                    .AddParameter("vnp_TmnCode", CommonUtils.GetApiKey("VNP_TMNCODE"))
                    .AddParameter("vnp_Amount", (req.Amount * 100).ToString())
                    .AddParameter("vnp_CurrCode", "VND")
                    .AddParameter("vnp_TxnRef", DateTime.Now.Ticks.ToString())
                    .AddParameter("vnp_OrderInfo", $"Thanh toan don hang {req.UserId}")
                    .AddParameter("vnp_OrderType", "other")
                    .AddParameter("vnp_Locale", "vn")
                    .AddParameter("vnp_ReturnUrl", "https://abcd1234.ngrok.io/index.html")
                    .AddParameter("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"))
                    .AddParameter("vnp_IpAddr", CommonUtils.GetIpAddress(context))
                    .HashSecret(CommonUtils.GetApiKey("VNP_HASHSECRET"))
                    .Build();

                Console.WriteLine(request.Secret);

                var paymentUrl = request.BuildVnPayUrl();
                var paymentQR = VnPayApiRequest.ToQrBase64(paymentUrl);

                return new BaseResponse<PaymentResponse>(
                    "Create url successfully",
                    StatusCodeEnum.OK_200,
                    new PaymentResponse
                    {
                        PaymentUrl = paymentUrl,
                        PaymentQR = paymentQR
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create vnpay url: {ex.Message}");
            }
        }
    }
}
