using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.DTOs.Authentication;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Services;

namespace MultiTenantApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IJWTProvider _jwtProvider;
        private readonly SignInManager<User> _signInManager;
        private readonly IAuthenticateService _authenticationServices;


        public AuthenticationController(RoleManager<IdentityRole<Guid>> roleManager, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IJWTProvider jWtProvider, SignInManager<User> signInManager, IAuthenticateService authenticationServices)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _jwtProvider = jWtProvider;
            _signInManager = signInManager;
            _authenticationServices = authenticationServices;
        }

        [HttpPost("Register-User")]
        public async Task<IActionResult> RegisterUser(RegisterDto RegisterDto)
        {
            var result = await _authenticationServices.RegisterUserAsync(RegisterDto);
            if (result)
            {
                return Ok(new ResponseModel<Object>(true, "UserRegisteredSuccessfully"));
            }
            else
            {
                return BadRequest(new ResponseModel<Object>(false, "UserRegistrationFailed"));
            }
        }

        [HttpPost("Login-User")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName) ?? await _userManager.FindByEmailAsync(loginDto.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password) && user.IsActive == true)
            {
                var role = await _userManager.GetRolesAsync(user);

                var Token = _jwtProvider.Generate(user, role);
                var refreshToken = _jwtProvider.GenerateRefreshToken();
                user.RefreshToken = refreshToken;


                var userDto = new GetUserDto
                {
                    Name = user.FirstName,
                    Email = user.Email,
                
                };
                var response = new
                {
                    userDto,
                    Token,
                    role
                };
                return Ok(new ResponseModel<Object>(true, response));
            }
            return Unauthorized(new ResponseModel<LoginDto>(false, Data: null!, "InvalidLoginCredentials"));
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authenticationServices.SignOutAsync();
            return Ok(new ResponseModel<Object>(true, "UserLoggedOutSuccessfully"));
        }

    }
}
