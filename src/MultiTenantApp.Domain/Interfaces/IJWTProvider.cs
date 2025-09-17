using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Domain.Interfaces
{
    public interface IJWTProvider
    {
        string Generate(User user, IList<string> roles, string? TenantId);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
