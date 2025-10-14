using BusinessObject.Payload.Request.VnPay;
using BusinessObject.Payload.Response.VnPay;
using DTOs.Response.Accounts;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace IGCSE.Controller
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        
    }
}
