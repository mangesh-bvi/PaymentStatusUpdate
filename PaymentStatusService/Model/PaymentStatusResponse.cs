using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentStatusService.Model
{
    public class PaymentStatusResponse
    {
        public string returnCode { get; set; }
        public string returnMessage { get; set; }
        public string tokenId { get; set; }
        public string shortURL { get; set; }
        public string status { get; set; }
    }
}
