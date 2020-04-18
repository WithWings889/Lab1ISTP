using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1
{
    public partial class Garbage
    {
        public Garbage()
        {
            GarbageMaterial = new HashSet<GarbageMaterial>();
        }

        public int Id { get; set; }

        [Display(Name = "Назва")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Довжина назви повинна бути від 2-х до 50-и символів")]
        [RegularExpression(@"([а-щА-ЩЬьЮюЯяЇїІіЄєҐґ]+\s*){1,}",
           ErrorMessage = "Ви можете ввести тільки літери кирилиці та пробіл")]
        public string Name { get; set; }

        public virtual ICollection<GarbageMaterial> GarbageMaterial { get; set; }
    }
}
