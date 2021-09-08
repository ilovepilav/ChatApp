using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Data.Entities.Abstract
{
    public class BaseEntity : IEntity
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
