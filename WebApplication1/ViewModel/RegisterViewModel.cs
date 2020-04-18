using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Електронна пошта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Рік народження")]
        [Range(1820, 2020)]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage ="Паролі не співпадають")]
        [DataType(DataType.Password)]

        public string PasswordConfirm { get; set; }
    }
}
