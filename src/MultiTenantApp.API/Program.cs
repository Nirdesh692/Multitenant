using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Attendance.API.Middleware;
using Attendance.Application.Interfaces;
using Attendance.Application.Services;
using Attendance.Domain.Entities;
using Attendance.Domain.Interfaces;
using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.DataSeeder;
using Attendance.Infrastructure.Repositories;
using Attendance.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContexts
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterConnection")));

// Register tenant context service
builder.Services.AddScoped<ITenantContextService, TenantContextService>();

// Configure Identity for Master database
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<MasterDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddScoped<IJWTProvider, JWTProvider>();
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUnitOfWork, TenantUnitOfWork>();
builder.Services.AddScoped<ITenantApplicationService, TenantApplicationService>();

// Register DataSeeder 
builder.Services.AddScoped<DataSeeder>();

// Register TenantDbContext as transient since it's created per request
builder.Services.AddTransient<TenantDbContext>(provider =>
{
    var tenantContextService = provider.GetRequiredService<ITenantContextService>();
    var context = tenantContextService.GetTenantDbContext();
    if (context == null)
        throw new InvalidOperationException("Tenant context is required");
    return context;
});

// Add services

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
app.UseMiddleware<TenantMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure master database is created
using (var scope = app.Services.CreateScope())
{
    var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
    await masterContext.Database.EnsureCreatedAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedDatabase();
}

app.Run();