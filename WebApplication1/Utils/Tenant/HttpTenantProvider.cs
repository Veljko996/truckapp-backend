using System.Security.Claims;

namespace WebApplication1.Utils.Tenant;

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int CurrentTenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out var tenantId))
                return tenantId;

            return 1;
        }
    }
}
