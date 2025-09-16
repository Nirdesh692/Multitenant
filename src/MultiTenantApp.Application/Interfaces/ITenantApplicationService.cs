using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantApp.Application.DTOs;

namespace MultiTenantApp.Application.Interfaces;

public interface ITenantApplicationService
{
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
    Task<TenantDto?> GetTenantByFrontendUrlAsync(string frontendUrl);
    Task<IEnumerable<TenantDto>> GetAllTenantsAsync();
}