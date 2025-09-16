using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantApp.Application.DTOs.Authentication
{
    public class UpdateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
