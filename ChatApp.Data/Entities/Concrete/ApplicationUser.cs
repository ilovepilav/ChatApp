using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Data.Entities.Concrete
{
    public class ApplicationUser : IdentityUser
    {
        public IList<Conversation> Conversations { get; set; } = new List<Conversation>();
    }

}
