using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data.Entities.Concrete;
using ChatApp.Data.Mappings;

namespace ChatApp.Data.Context
{
    public class ChatAppWebContext : IdentityDbContext<ApplicationUser>
    {
        public ChatAppWebContext(DbContextOptions<ChatAppWebContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.ApplyConfiguration(new MessageMap());
            builder.ApplyConfiguration(new ConversationMap());
        }
    }
}
