using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantApp.Domain.Entities;
using MultiTenantApp.Infrastructure.DataSeeder;

namespace MultiTenantApp.Infrastructure.Data
{
    public class MasterDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        private readonly IServiceProvider _serviceProvider;

        public MasterDbContext() { }

        // Add IServiceProvider parameter to constructor
        public MasterDbContext(DbContextOptions<MasterDbContext> options, IServiceProvider serviceProvider = null)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
}