using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;

namespace MultiTenantApp.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly MasterDbContext _masterContext;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public TenantService(MasterDbContext masterContext, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _masterContext = masterContext;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
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
        if (await context.Users.AnyAsync())
        {
            return;
        }
        var userStore = new UserStore<User, IdentityRole<Guid>, TenantDbContext, Guid>(context);
        var roleStore = new RoleStore<IdentityRole<Guid>, TenantDbContext, Guid>(context);

        var userManager = new UserManager<User>(userStore,
            _serviceProvider.GetRequiredService<IOptions<IdentityOptions>>(),
            _serviceProvider.GetRequiredService<IPasswordHasher<User>>(),
            _serviceProvider.GetRequiredService<IEnumerable<IUserValidator<User>>>(),
            _serviceProvider.GetRequiredService<IEnumerable<IPasswordValidator<User>>>(),
            _serviceProvider.GetRequiredService<ILookupNormalizer>(),
            _serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            _serviceProvider,
            _serviceProvider.GetRequiredService<ILogger<UserManager<User>>>());
        var roleManager = new RoleManager<IdentityRole<Guid>>(roleStore,
            _serviceProvider.GetRequiredService<IEnumerable<IRoleValidator<IdentityRole<Guid>>>>(),
            _serviceProvider.GetRequiredService<ILookupNormalizer>(),
            _serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            _serviceProvider.GetRequiredService<ILogger<RoleManager<IdentityRole<Guid>>>>());
        var roles = new List<IdentityRole<Guid>>()
        {
            new IdentityRole<Guid>() { Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<Guid>() { Name = "User", NormalizedName = "USER" }
        };
        foreach (var item in roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(item.Name!);

            if (!roleExist)
            {
                var roleResult = await roleManager.CreateAsync(item);
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
        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            var addRoleResult = await userManager.AddToRoleAsync(admin, "Admin");
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