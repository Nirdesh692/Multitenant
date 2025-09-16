using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;

namespace MultiTenantApp.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly MasterDbContext _masterContext;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<User> _userManager;

    public TenantService(MasterDbContext masterContext, IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _masterContext = masterContext;
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Tenant> CreateTenantAsync(string name, string server, string database, string? userId, string? password, bool useWindowsAuth, string frontendUrl)
    {
        var tenant = new Tenant
        {
            Name = name,
            Server = server,
            Database = database,
            UserId = userId,
            Password = password,
            UseWindowsAuth = useWindowsAuth,
            FrontendUrl = frontendUrl
        };

        _masterContext.Tenants.Add(tenant);
        await _masterContext.SaveChangesAsync();

        return tenant;
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
    {
        return await _masterContext.Tenants.FindAsync(tenantId);
    }

    public async Task<Tenant?> GetTenantByFrontendUrlAsync(string frontendUrl)
    {
        return await _masterContext.Tenants.FirstOrDefaultAsync(t => t.FrontendUrl == frontendUrl && t.IsActive);
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
    {
        return await _masterContext.Tenants.Where(t => t.IsActive).ToListAsync();
    }

    public async Task MigrateTenantDatabaseAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new TenantDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        await SeedUserAndRoleAsync(context);
    }

    public string BuildConnectionString(Tenant tenant)
    {
        var templateKey = tenant.UseWindowsAuth ? "TenantTemplateWindows" : "TenantTemplateSql";
        var template = _configuration.GetConnectionString(templateKey) ?? string.Empty;
        
        var connectionString = template
            .Replace("{Server}", tenant.Server)
            .Replace("{Database}", tenant.Database);
            
        if (!tenant.UseWindowsAuth)
        {
            connectionString = connectionString
                .Replace("{UserId}", tenant.UserId ?? string.Empty)
                .Replace("{Password}", tenant.Password ?? string.Empty);
        }
        
        return connectionString;
    }
    public async Task SeedUserAndRoleAsync(TenantDbContext context)
    {
        if(await context.Users.AnyAsync())
        {
            return;
        }
        var roles = new List<IdentityRole<Guid>>()
        {
            new IdentityRole<Guid>() { Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<Guid>() { Name = "User", NormalizedName = "USER" }
        };
        foreach (var item in roles)
        {
            var roleExist = await _roleManager.RoleExistsAsync(item.Name!);

            if (!roleExist)
            {
                var roleResult= await _roleManager.CreateAsync(item);
                if (!roleResult.Succeeded)
                {
                    throw new Exception($"Failed to create role {item.Name}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        var admin = new User()
        {
            Email = "admin@gmail.com",
            UserName = "admin",
            NormalizedUserName = "ADMIN",

        };
        var password = "Admin@123"; 
        var result = await _userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            var addRoleResult = await _userManager.AddToRoleAsync(admin, "Admin");
            if (!addRoleResult.Succeeded)
            {
                throw new Exception($"Failed to assign role to admin user: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

    }

}