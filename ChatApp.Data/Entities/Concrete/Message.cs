using ChatApp.Data.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Data.Entities.Concrete
{
    public class Message : BaseEntity
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public Guid ConversationId { get; set; }
    }
}
