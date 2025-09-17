using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Attendance.Domain.Entities;
using Attendance.Infrastructure.Data;

namespace Attendance.Infrastructure.DataSeeder
{
    public class DataSeeder
    {
        private readonly MasterDbContext _masterDbContext;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<User> _userManager;

        public DataSeeder(MasterDbContext context, RoleManager<IdentityRole<Guid>> roleManager, UserManager<User> userManager)
        {
            _masterDbContext = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task SeedDatabase()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    await SeedUsers();
                    scope.Complete();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task SeedUsers()
        {
            if(_masterDbContext.Users.Any())
                return;
            
            // Create SuperAdmin role
            var superAdminRole = new IdentityRole<Guid>() { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" };
            var superAdminRoleExist = await _roleManager.RoleExistsAsync(superAdminRole.Name);
            if (!superAdminRoleExist)
            {
                await _roleManager.CreateAsync(superAdminRole);
            }
            
            // Create User role
            var userRole = new IdentityRole<Guid>() { Name = "User", NormalizedName = "USER" };
            var userRoleExist = await _roleManager.RoleExistsAsync(userRole.Name);
            if (!userRoleExist)
            {
                await _roleManager.CreateAsync(userRole);
            }

            var superAdmin = new User()
            {
                Email = "superadmin@gmail.com",
                UserName = "superadmin",
                NormalizedUserName = "SUPERADMIN",
            };
            var password = "SuperAdmin@123";
            var result = await _userManager.CreateAsync(superAdmin, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(superAdmin, superAdminRole.Name);
                await _masterDbContext.SaveChangesAsync();
            }
        }
        
    }
}
