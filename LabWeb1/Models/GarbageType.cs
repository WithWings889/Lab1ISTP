using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabWeb1
{
    public partial class GarbageType
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Довжина назви повинна бути від 2-х до 50-и символів")]
        [RegularExpression(@"([а-щА-ЩЬьЮюЯяЇїІіЄєҐґ]+\s*){1,}", 
            ErrorMessage = "Ви можете ввести тільки літери кирилиці")]
        [Display(Name = "Назва")]
        public string Name { get; set; }

        public virtual ICollection<Material> Materials { get; set; }
    }
}
