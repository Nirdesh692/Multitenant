using System.Collections.Generic;
using System.Threading.Tasks;
using Attendance.Application.DTOs;

namespace Attendance.Application.Interfaces;

public interface ITenantApplicationService
{
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
    Task<TenantDto?> GetTenantByFrontendUrlAsync(string frontendUrl);
    Task<IEnumerable<TenantDto>> GetAllTenantsAsync();
}