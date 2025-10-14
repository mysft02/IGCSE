using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Transactionhistory
{
    public int TransactionId { get; set; }

    public int CourseId { get; set; }

    public string ParentId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string VnpTxnRef { get; set; } = null!;

    public string VnpTransactionDate { get; set; } = null!;
}
