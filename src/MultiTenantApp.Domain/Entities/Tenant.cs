namespace MultiTenantApp.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public bool UseWindowsAuth { get; set; } = false;
    public string FrontendUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}