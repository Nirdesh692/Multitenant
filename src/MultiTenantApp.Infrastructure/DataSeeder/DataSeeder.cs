using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Infrastructure.Data;

namespace MultiTenantApp.Infrastructure.DataSeeder
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
            var roles = new IdentityRole<Guid>() { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" };

            var roleExist = await _roleManager.RoleExistsAsync(roles.Name);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(roles);
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
                await _userManager.AddToRoleAsync(superAdmin, roles.Name);
                await _masterDbContext.SaveChangesAsync();
            }
        }
    }
}
