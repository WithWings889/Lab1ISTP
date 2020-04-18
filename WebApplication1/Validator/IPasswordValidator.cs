using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Validator
{
    public interface IPasswordValidator<T> where T : class
    {
        Task<IdentityResult> ValidateAsync(UserManager<T> manager, T user, string password);
    }
}
