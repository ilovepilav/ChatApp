using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Data.DTOs
{
    public class MessageDto
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsPrivate { get; set; }
    }
}
