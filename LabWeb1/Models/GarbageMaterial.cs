using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb1
{
    public partial class GarbageMaterial
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Garbage")]
        [Display(Name = "Сміття")]
        public int IdGarbage { get; set; }
        [ForeignKey("Material")]
        [Display(Name = "Матеріал")]
        public int IdMaterial { get; set; }
        [Display(Name = "Сміття")]

        public virtual Garbage Garbage { get; set; }
        [Display(Name = "Матеріал")]
        public virtual Material Material { get; set; }
    }
}
