
namespace BusinessObject.Payload.Response.VnPay
{
    public class VnPayQueryApiResponse
    {
        public string VnpReponseId { get; set; }
        public string VnpTmnCode { get; set; }
        public string VnpTxnRef { get; set; }
        public string VnpAmount { get; set; }
        public string VnpOrderInfo { get; set; }
        public string VnpResponseCode { get; set; }
        public string VnpMessage { get; set; }
        public string VnpBankCode { get; set; }
        public string VnpTransactionNo { get; set; }
        public string VnpTransactionType { get; set; }
        public string VnpTransactionStatus { get; set; }
        public string VnpSecureHash { get; set; }
    }
}
