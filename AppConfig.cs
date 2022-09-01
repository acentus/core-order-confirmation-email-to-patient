using System;
using System.Collections.Generic;
using System.Text;

namespace EmailSenderService
{
    public class AppConfig
    {
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;   
    }
}
