using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Довжина назви повинна бути від 2-х до 50-и символів")]
        [RegularExpression(@"([а-щА-ЩЬьЮюЯяЇїІіЄєҐґ]+\s*){1,}",
           ErrorMessage = "Ви можете ввести тільки літери кирилиці")]
        [Display(Name = "Назва")]
        public string Name { get; set; }

        public virtual ICollection<FactoryGarbageType> FactoryGarbageType { get; set; }
        public virtual ICollection<Material> Material { get; set; }
    }
}
