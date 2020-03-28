using System;
using System.Collections.Generic;

namespace WebApplication1
{
    public partial class Factory
    {
        public Factory()
        {
            FactoryGarbageType = new HashSet<FactoryGarbageType>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }

        public virtual ICollection<FactoryGarbageType> FactoryGarbageType { get; set; }
    }
}
