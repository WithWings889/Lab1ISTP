using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Validator
{

    public class CustomUserValidator : UserValidator<User>
    {

        //UserManager<User> _userManager;
        //public CustomUserValidator(UserManager<User> userManager)
        //{
        //    _userManager = userManager;
        //}
        public override Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (user.Email.ToLower().EndsWith("@mail.ru") || user.Email.ToLower().EndsWith("@yandex.ru"))
            {
                errors.Add(new IdentityError
                {
                    Description = "Ця адреса знаходиться на ресурсі, який потрапив під заборону згідно Указу Президента від 15.05.2017 №133/2017." +
                                  "Оберіть інший поштовий сервіс"
                });
            }
            //var temp = _userManager.Users.Where(a => a.Email == user.Email).ToList();
            //if (temp.Count != 0)
            //{
            //    errors.Add(new IdentityError
            //    {
            //        Description = "Email уже використовується іншим користувачем"
            //    });
            //}
            return Task.FromResult(errors.Count == 0 ?
                IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
        }
    }
}
