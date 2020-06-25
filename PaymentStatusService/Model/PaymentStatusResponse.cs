using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentStatusService.Model
{
    public class PaymentStatusResponse
    {
        /// <summary>
        /// returnCode
        /// </summary>
        public string returnCode { get; set; }

        /// <summary>
        /// returnMessage
        /// </summary>
        public string returnMessage { get; set; }

        /// <summary>
        /// tokenId
        /// </summary>
        public string tokenId { get; set; }

        /// <summary>
        /// shortURL
        /// </summary>
        public string shortURL { get; set; }

        /// <summary>
        /// status
        /// </summary>
        public string status { get; set; }
    }
}
