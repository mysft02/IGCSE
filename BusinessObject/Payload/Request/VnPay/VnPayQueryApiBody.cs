using BusinessObject.Payload.Response;
namespace BusinessObject.Payload.Request.VnPay
{
    public class VnPayQueryApiBody : IApiBodyProcess
    {
        public string VnpRequestId { get; set; }
        public string VnpVersion { get; set; }
        public string VnpCommand { get; set; }
        public string VnpTmnCode { get; set; }
        public string VnpTxnRef { get; set; }
        public string VnpOrderInfo { get; set; }
        public string VnpTransactionDate { get; set; }
        public string VnpCreateDate { get; set; }
        public string VnpIpAddr { get; set; }
        public string VnpSecureHash { get; set; }

        public IDictionary<string, IEnumerable<object>> ProcessBody()
        {
            return new Dictionary<string, IEnumerable<object>>
            {
                { "vnp_RequestId", new object[] { VnpRequestId } },
                { "vnp_Version", new object[] { VnpVersion } },
                { "vnp_Command", new object[] { VnpCommand } },
                { "vnp_TmnCode", new object[] { VnpTmnCode } },
                { "vnp_TxnRef", new object[] { VnpTxnRef } },
                { "vnp_OrderInfo", new object[] { VnpOrderInfo } },
                { "vnp_TransactionDate", new object[] { VnpTransactionDate } },
                { "vnp_CreateDate", new object[] { VnpCreateDate } },
                { "vnp_IpAddr", new object[] { VnpIpAddr } },
                { "vnp_SecureHash", new object[] { VnpSecureHash } }
            };
        }
    }
}
