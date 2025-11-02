namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSPaymentReturnResponse
    {
        public string Code { get; set; }
        
        public string Desc { get; set; }

        public PayOSPaymentReturnData Data { get; set; }

        public string Signature { get; set; }
    }
}
