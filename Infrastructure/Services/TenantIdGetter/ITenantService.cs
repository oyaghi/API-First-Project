using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Services.TenantIdGetter
{
    public interface ITenantService
    {
        int GetTenantId();
    }

    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetTenantId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // Return 0 if there is no user or tenantId claim is not found
            if (user == null)
            {
                return 0; // No user context, so return default tenant ID (0)
            }

            var tenantIdClaim = user.Claims.FirstOrDefault(c => c.Type == "tenantId");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                return tenantId;
            }

            return 0; // Return default tenant ID (0) if tenantId claim is not found or invalid
        }

    }

}
