using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Data.DTOs
{
    public class ConversationDto
    {
        public bool PrivateChat { get; set; }
        public string Receipent { get; set; }
        public List<MessageDto> Messages { get; set; } = new List<MessageDto>();
    }
}
