using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Attendance.Domain.Entities;

namespace Attendance.Infrastructure.Data;

public class TenantDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public new DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });
    }
}