using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantApp.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
            
            // Store tenant ID in HttpContext for later use
            if (!string.IsNullOrEmpty(tenantId))
            {
                context.Items["TenantId"] = tenantId;
            }

            await _next(context);
        }
    }
}