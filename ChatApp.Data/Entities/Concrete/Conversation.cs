using ChatApp.Data.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Data.Entities.Concrete
{
    public class Conversation : BaseEntity
    {
        public string Name { get; set; }
        public IList<Message> Messages { get; set; } = new List<Message>();
        public bool PrivateChat { get; set; }
        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual DateTime UpdateDate { get; set; } = DateTime.Now;
    }
}
