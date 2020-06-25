using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentStatusService.Model
{
    public class PaymentStatusRequest
    {
        /// <summary>
        /// tokenId
        /// </summary>
        public string tokenId { get; set; }

        /// <summary>
        /// programCode
        /// </summary>
        public string programCode { get; set; }

        /// <summary>
        /// storeCode
        /// </summary>
        public string storeCode { get; set; }

        /// <summary>
        /// billDateTime
        /// </summary>
        public string billDateTime { get; set; }

        /// <summary>
        /// terminalId
        /// </summary>
        public string terminalId { get; set; }

        /// <summary>
        /// merchantTxnID
        /// </summary>
        public string merchantTxnID { get; set; }

        /// <summary>
        /// mobile
        /// </summary>
        public string mobile { get; set; }

    }
}
