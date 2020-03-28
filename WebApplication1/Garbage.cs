using System;
using System.Collections.Generic;

namespace WebApplication1
{
    public partial class Garbage
    {
        public Garbage()
        {
            GarbageMaterial = new HashSet<GarbageMaterial>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<GarbageMaterial> GarbageMaterial { get; set; }
    }
}
