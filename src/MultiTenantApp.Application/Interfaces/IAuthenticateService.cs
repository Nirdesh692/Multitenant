using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs.Authentication;

namespace MultiTenantApp.Application.Interfaces
{
    public interface IAuthenticateService
    {
        Task<bool> RegisterUserAsync(RegisterDto registerDto);
        Task<bool> SignInAsync(LoginDto loginDto);
        Task<bool> SignOutAsync();  

    }
}
