using System;
using System.Collections.Generic;

namespace WebApplication1
{
    public partial class GarbageType
    {
        public GarbageType()
        {
            FactoryGarbageType = new HashSet<FactoryGarbageType>();
            Material = new HashSet<Material>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<FactoryGarbageType> FactoryGarbageType { get; set; }
        public virtual ICollection<Material> Material { get; set; }
    }
}
