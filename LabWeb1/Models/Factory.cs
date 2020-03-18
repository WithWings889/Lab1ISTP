using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb1
{
    public partial class Factory
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Довжина назви повинна бути від 2-х до 50-и символів")]
        [RegularExpression(@"([а-щА-ЩЬьЮюЯяЇїІіЄєҐґ]+\s*){1,}", 
            ErrorMessage = "Ви можете ввести тільки літери латиниці, цифри та пробіл. Перша буква повинна бути прописною")]
        [Display(Name = "Назва")]
        public string Name { get; set; }
        [Display(Name = "Адреса")]
        [RegularExpression(@"[м].{1}\s*([а-щА-ЩЬьЮюЯяЇїІіЄєҐґ]+\s*){1,}",
            ErrorMessage = "Ви можете ввести тільки літери кирилиці. Першим ви обов'язково маєте вказати місто (м. Харків)")]
        public string Address { get; set; }
        [Display(Name = "Вебсайт")]
        [Url(ErrorMessage = "Неправильний формат сайту")]
        public string Website { get; set; }

        //public virtual GarbageType GarbageType { get; set; }
        public virtual FactoryGarbageType FactoryGarbageType { get; set; }
        //public virtual ICollection<GarbageType> GarbageTypes { get; set; }
        [NotMapped]
        public virtual ICollection<FactoryGarbageType> FactoryGarbageTypes { get; set; }
    }
}
