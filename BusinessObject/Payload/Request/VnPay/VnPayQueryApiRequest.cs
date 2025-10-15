using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Payload.Request.VnPay
{
    public class VnPayQueryApiRequest
    {
        public string VnpTxnRef { get; set; }

        public string VnpTransactionDate { get; set; }
    }
}
