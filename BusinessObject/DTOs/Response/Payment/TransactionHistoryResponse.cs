﻿using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.Payment
{
    public class TransactionHistoryResponse
    {
        public Course Course { get; set; }

        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
