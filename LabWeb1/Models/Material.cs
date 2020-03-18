﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb1
{
    public partial class Material
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Назва")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Довжина назви повинна бути від 3-х до 50-и символів")]
        [RegularExpression(@"([а-щА-ЩЬьЮюЯяЇїІіЄєҐґ]+\s*){1,}",
           ErrorMessage = "Ви можете ввести тільки літери кирилиці")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле повинно бути заповненим")]
        [Display(Name = "Ідентифікатор матеріалу (ISO 1043)")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Довжина назви повинна бути від 3-х до 50-и символів")]
        [RegularExpression(@"#[0-9]+[A-Z0-9-]+", 
            ErrorMessage = "Ідентифікатор має мати вигляд #*номер**тип*. Тип вказується латинськими літерами.\nНаприклад #72GLS")]
        public string MaterialCard { get; set; }

        [Display(Name = "Тип")]
        [ForeignKey("GarbageType")]
        public int? IdGarbageType { get; set; }
        [Display(Name = "Опис")]
        public string Info { get; set; }

        public virtual GarbageType GarbageType { get; set; }
    }
}
