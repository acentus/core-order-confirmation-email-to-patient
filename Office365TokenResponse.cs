using System;
using System.Collections.Generic;
using System.Text;

namespace EmailSenderService
{
    public class Office365TokenResponse
    {
        public string token_type { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }
    }
}
