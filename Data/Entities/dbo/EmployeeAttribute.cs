using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities.dbo
{
    public class EmployeeAttribute : Entity
    {
        public EmployeeAttributeType AttributeType { get; set; }
        public string Title { get; set; }
        public bool IsStatic { get; set; }
        public bool IsOther { get; set; } //diger secenegini yonetmek icin
        public string OtherText { get; set; }
        public bool Active { get; set; }    
    }

    public enum EmployeeAttributeType : Int16
    {
        PrevOccupation = 1,
        Educational = 2
    }
}