using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class EmployeeAcceptance : Entity
    {
        public int EmployeeId { get; set; }
        public DateTime AcceptanceDate { get; set; }
    }
}