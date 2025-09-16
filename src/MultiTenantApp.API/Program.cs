using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiTenantApp.Application.Interfaces;
using MultiTenantApp.Application.Services;
using MultiTenantApp.Domain.Interfaces;
using MultiTenantApp.Infrastructure.Data;
using MultiTenantApp.Infrastructure.Repositories;
using MultiTenantApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContexts
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterConnection")));

builder.Services.AddScoped<TenantDbContext>(provider =>
{
    var httpContext = provider.GetService<IHttpContextAccessor>()?.HttpContext;
    var tenantId = httpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault();
    
    if (string.IsNullOrEmpty(tenantId))
    {
        throw new InvalidOperationException("Tenant ID is required");
    }

    var masterContext = provider.GetService<MasterDbContext>();
    var tenant = masterContext?.Tenants.FirstOrDefault(t => t.Id.ToString() == tenantId);
    
    if (tenant == null)
    {
        throw new InvalidOperationException("Invalid tenant");
    }

    var tenantService = provider.GetService<ITenantService>();
    var connectionString = tenantService?.BuildConnectionString(tenant) ?? string.Empty;

    var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
    optionsBuilder.UseSqlServer(connectionString);
    
    return new TenantDbContext(optionsBuilder.Options);
});

// Add services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITenantApplicationService, TenantApplicationService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MultiTenant API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure master database is created
using (var scope = app.Services.CreateScope())
{
    var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
    masterContext.Database.EnsureCreated();
}

app.Run();