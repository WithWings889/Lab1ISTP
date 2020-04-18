using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1
{
    public partial class FactoryGarbageType
    {
        public int Id { get; set; }
        [Display(Name = "Тип сміття")]
        public int IdGarbageType { get; set; }
        [Display(Name = "Назва фабрики")]
        public int IdFactory { get; set; }
        
        public virtual Factory IdFactoryNavigation { get; set; }
        
        public virtual GarbageType IdGarbageTypeNavigation { get; set; }
    }
}
