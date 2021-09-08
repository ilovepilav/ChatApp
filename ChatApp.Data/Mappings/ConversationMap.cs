using ChatApp.Data.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Data.Mappings
{
    public class ConversationMap : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.CreateDate).IsRequired();
            builder.Property(b => b.PrivateChat).IsRequired();
            builder.HasIndex(b => b.Name).IsUnique();
        }
    }
}
