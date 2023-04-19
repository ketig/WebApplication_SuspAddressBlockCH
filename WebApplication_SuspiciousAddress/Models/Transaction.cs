using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication_SuspiciousAddress.Models
{
    public class Transaction
    {
        public long? Amount { get; set; }
        public string OwnerAddress { get; set; }
        public string ToAddress { get; set; }
        public string Time { get; set; }
        public int Distance { get; set; }
    }
}