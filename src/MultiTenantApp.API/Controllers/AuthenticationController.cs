using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Attendance.Application.DTOs;
using Attendance.Application.DTOs.Authentication;
using Attendance.Application.Interfaces;
using Attendance.Domain.Entities;
using Attendance.Domain.Interfaces;
using Attendance.Infrastructure.Identity;
using Attendance.Infrastructure.Services;

namespace Attendance.API.Controllers
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
            var tenantId = HttpContext.Request.Headers["X-Tenant-ID"].FirstOrDefault();
            User? user = null;
            IList<string> roles = new List<string>();
            string context = "master";

            if (string.IsNullOrEmpty(tenantId))
            {
                // Try master database authentication
                user = await _userManager.FindByNameAsync(loginDto.UserName) ?? await _userManager.FindByEmailAsync(loginDto.UserName);
                if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password) && user.IsActive)
                {
                    roles = await _userManager.GetRolesAsync(user);
                    context = "master";
                }
            }
            else
            {
                // Try tenant database authentication
                try
                {
                    var tenantContext = HttpContext.RequestServices.GetRequiredService<ITenantContextService>();
                    var tenantDbContext = tenantContext.GetTenantDbContext();
                    
                    if (tenantDbContext != null)
                    {
                        // Ensure tenant database exists and is migrated
                        await tenantDbContext.Database.EnsureCreatedAsync();
                        
                        // Create tenant-specific UserManager
                        var tenantUserStore = new TenantUserStore(tenantContext);
                        var options = HttpContext.RequestServices.GetRequiredService<IOptions<IdentityOptions>>();
                        var passwordHasher = HttpContext.RequestServices.GetRequiredService<IPasswordHasher<User>>();
                        var userValidators = HttpContext.RequestServices.GetRequiredService<IEnumerable<IUserValidator<User>>>();
                        var passwordValidators = HttpContext.RequestServices.GetRequiredService<IEnumerable<IPasswordValidator<User>>>();
                        var keyNormalizer = HttpContext.RequestServices.GetRequiredService<ILookupNormalizer>();
                        var errors = HttpContext.RequestServices.GetRequiredService<IdentityErrorDescriber>();
                        var services = HttpContext.RequestServices;
                        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<UserManager<User>>>();
                        
                        var tenantUserManager = new UserManager<User>(tenantUserStore, options, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger);
                        
                        user = await tenantUserManager.FindByNameAsync(loginDto.UserName) ?? await tenantUserManager.FindByEmailAsync(loginDto.UserName);
                        if (user != null && await tenantUserManager.CheckPasswordAsync(user, loginDto.Password) && user.IsActive)
                        {
                            roles = await tenantUserManager.GetRolesAsync(user);
                            context = "tenant";
                        }
                    }
                }
                catch (Exception)
                {
                    // If tenant authentication fails, try master database
                    user = await _userManager.FindByNameAsync(loginDto.UserName) ?? await _userManager.FindByEmailAsync(loginDto.UserName);
                    if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password) && user.IsActive)
                    {
                        roles = await _userManager.GetRolesAsync(user);
                        context = "master";
                    }
                }
            }

            if (user != null && roles.Any())
            {
                var Token = _jwtProvider.Generate(user, roles, tenantId);
                var refreshToken = _jwtProvider.GenerateRefreshToken();
                user.RefreshToken = refreshToken;

                var userDto = new GetUserDto
                {
                    Name = user.FirstName,
                    Email = user.Email!,
                };
                
                var response = new
                {
                    userDto,
                    Token,
                    role = roles,
                    context = context,
                    tenantId = tenantId
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
