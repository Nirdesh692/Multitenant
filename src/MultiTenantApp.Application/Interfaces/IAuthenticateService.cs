using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Attendance.Application.DTOs.Authentication;

namespace Attendance.Application.Interfaces
{
    public interface IAuthenticateService
    {
        Task<bool> RegisterUserAsync(RegisterDto registerDto);
        Task<bool> SignInAsync(LoginDto loginDto);
        Task<bool> SignOutAsync();  

    }
}
