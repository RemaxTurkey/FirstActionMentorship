using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Entity
    {
        public int Id { get; set; }
        public virtual bool IsActive { get; set; }
    }
}