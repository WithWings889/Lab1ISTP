using System;
using System.Collections.Generic;

namespace WebApplication1
{
    public partial class FactoryGarbageType
    {
        public int Id { get; set; }
        public int IdGarbageType { get; set; }
        public int IdFactory { get; set; }

        public virtual Factory IdFactoryNavigation { get; set; }
        public virtual GarbageType IdGarbageTypeNavigation { get; set; }
    }
}
