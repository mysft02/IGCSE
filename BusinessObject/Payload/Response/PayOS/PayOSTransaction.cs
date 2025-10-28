namespace BusinessObject.Payload.Response.PayOS
{
    public class PayOSTransaction
    {
        public int Amount { get; set; }

        public string Description { get; set; }

        public string AccountNumber { get; set; }

        public string Reference { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public string CounterAccountBankId { get; set; }

        public string CounterAccountBankName { get; set; }

        public string CounterAccountName { get; set; }

        public string CounterAccountNumber { get; set; }

        public string VirtualAccountName { get; set; }

        public string VirtualAccountNumber { get; set; }
    }
}
