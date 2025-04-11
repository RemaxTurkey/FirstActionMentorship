using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Common
{
    public class AppSettings
    {
        public string PhotoUploadPath { get; set; }
        public string PhotoUploadUrl { get; set; }
        public string Token { get; set; }
    }

    public class RemaxySettings
    {
        public string ApiUrl { get; set; }
        public string Token { get; set; }
    }
}