using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attendance.Application.DTOs;
using Attendance.Application.Interfaces;
using Attendance.Domain.Interfaces;

namespace Attendance.Application.Services;

public class TenantApplicationService : ITenantApplicationService
{
    private readonly ITenantService _tenantService;

    public TenantApplicationService(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<TenantDto> CreateTenantAsync(CreateTenantDto dto)
    {
        var tenant = await _tenantService.CreateTenantAsync(dto.Name, dto.Server, dto.Database, dto.UserId, dto.Password, dto.UseWindowsAuth, dto.FrontendUrl);
        var connectionString = _tenantService.BuildConnectionString(tenant);
        await _tenantService.MigrateTenantDatabaseAsync(connectionString);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            FrontendUrl = tenant.FrontendUrl,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<TenantDto?> GetTenantByFrontendUrlAsync(string frontendUrl)
    {
        var tenant = await _tenantService.GetTenantByFrontendUrlAsync(frontendUrl);
        if (tenant == null) return null;
        
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            FrontendUrl = tenant.FrontendUrl,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
    {
        var tenants = await _tenantService.GetAllTenantsAsync();
        
        return tenants.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            FrontendUrl = t.FrontendUrl,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        });
    }
}