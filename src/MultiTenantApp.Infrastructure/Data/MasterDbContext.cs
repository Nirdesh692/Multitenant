using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Domain.Entities;

namespace MultiTenantApp.Infrastructure.Data;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Server).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Database).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UseWindowsAuth).IsRequired();
            entity.Property(e => e.FrontendUrl).IsRequired().HasMaxLength(500);
        });
    }
}