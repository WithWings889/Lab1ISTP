using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace WebApplication1
{
    public partial class Factory
    {
        public Factory()
        {
            FactoryGarbageType = new HashSet<FactoryGarbageType>();
        }

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

        public virtual ICollection<FactoryGarbageType> FactoryGarbageType { get; set; }
        
    }
}
