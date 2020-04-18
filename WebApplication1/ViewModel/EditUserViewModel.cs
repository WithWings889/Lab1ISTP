using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModel
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        [Range(1820, 2020)]
        public int Year { get; set; }
    }
}
