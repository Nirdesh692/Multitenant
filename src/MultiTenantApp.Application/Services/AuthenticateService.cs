using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MultiTenantApp.Application.DTOs.Authentication;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Application.Services
{
    public class AuthenticateService: IAuthenticateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly SignInManager<User> _signInManager;
        // private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticateService(IUnitOfWork unitOfWork, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, SignInManager<User> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            //  _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> RegisterUserAsync(RegisterDto registerUserDto)
        {
            try
            {
                var emailExists = await _userManager.FindByEmailAsync(registerUserDto.Email);
                if (emailExists != null)
                {
                    throw new InvalidOperationException("Email already exists");
                }
                var user = new User
                {
                    FirstName = registerUserDto.FirstName,
                    LastName = registerUserDto.LastName,
                    Email = registerUserDto.Email,
                    UserName = registerUserDto.Email,
                    IsActive = true
                };
                var result = await _userManager.CreateAsync(user, registerUserDto.Password);
                await _userManager.AddToRoleAsync(user, "User");
                if (result.Succeeded)
                {
                    return true;
                }
                throw new InvalidOperationException("Error while creating the user");
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> SignOutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> SignInAsync(LoginDto signInDto)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(signInDto.UserName, signInDto.Password, false, false);
                return result.Succeeded;
            }
            catch
            {
                throw;
            }
        }

    }
}

