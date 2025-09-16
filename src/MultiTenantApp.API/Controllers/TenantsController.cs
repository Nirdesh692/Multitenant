using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MultiTenantApp.Application.DTOs;
using MultiTenantApp.Application.Interfaces;

namespace MultiTenantApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantApplicationService _tenantService;

    public TenantsController(ITenantApplicationService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantDto dto)
    {
        var result = await _tenantService.CreateTenantAsync(dto);
        return CreatedAtAction(nameof(GetTenants), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetTenants()
    {
        var result = await _tenantService.GetAllTenantsAsync();
        return Ok(result);
    }

    [HttpGet("by-frontend-url")]
    public async Task<ActionResult<TenantDto>> GetTenantByFrontendUrl([FromQuery] string frontendUrl)
    {
        var result = await _tenantService.GetTenantByFrontendUrlAsync(frontendUrl);
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}