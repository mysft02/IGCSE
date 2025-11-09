namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSPaymentReturnData
    {
        public string Id { get; set; }
        
        public int OrderCode { get; set; }

        public int Amount { get; set; }

        public int AmountPaid { get; set; }
        
        public int AmountRemaining { get; set; }

        public string Status { get; set; }

        public string CreatedAt { get; set; }

        public List<PayOSTransaction> Transactions { get; set; }

        public string CanceledAt { get; set; }

        public string CancellationReason { get; set; }
    }
}
