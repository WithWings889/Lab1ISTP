using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb1
{
    public partial class FactoryGarbageType
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("GarbageType")]
        public int IdGarbageType { get; set; }
        [ForeignKey("Factory")]
        public int IdFactory { get; set; }
        [Display(Name = "Фабрики")]

        public virtual Factory Factory { get; set; }
        [Display(Name = "Типи Сміття")]
        public virtual GarbageType GarbageType { get; set; }
        [NotMapped]
        public virtual ICollection<GarbageType> GarbageTypes { get; set; }
        [NotMapped]
        public virtual ICollection<Factory> Factories { get; set; }
    }
}
