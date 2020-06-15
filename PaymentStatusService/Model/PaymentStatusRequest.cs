using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentStatusService.Model
{
    public class PaymentStatusRequest
    {
        public string tokenId { get; set; }
        public string programCode { get; set; }
        public string storeCode { get; set; }
        public string billDateTime { get; set; }
        public string terminalId { get; set; }
        public string merchantTxnID { get; set; }
        public string mobile { get; set; }

    }
}
