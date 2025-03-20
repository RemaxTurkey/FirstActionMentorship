using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities.dbo
{
    public class EmployeeDescription : Entity
    {
        public int EmployeeId { get; set; }
        public byte LanguageId { get; set; }
        public string Slogan { get; set; }
        public string Introduction { get; set; }
        public string Detail { get; set; }
        public bool Active { get; set; }
    }
}