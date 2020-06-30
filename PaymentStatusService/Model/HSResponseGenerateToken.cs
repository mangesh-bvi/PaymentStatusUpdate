using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentStatusService.Model
{
    public class HSResponseGenerateToken
    {
        //public string access_Token { get; set; }
        //public int expires_In { get; set; }
        //public string token_Type { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string error { get; set; }
    }
}
