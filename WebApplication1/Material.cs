using System;
using System.Collections.Generic;

namespace WebApplication1
{
    public partial class Material
    {
        public Material()
        {
            GarbageMaterial = new HashSet<GarbageMaterial>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string MaterialCard { get; set; }
        public int? IdGarbageType { get; set; }
        public string Info { get; set; }

        public virtual GarbageType IdGarbageTypeNavigation { get; set; }
        public virtual ICollection<GarbageMaterial> GarbageMaterial { get; set; }
    }
}
