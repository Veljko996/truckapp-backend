namespace WebApplication1.Utils.Tenant;

public interface ITenantProvider
{
    int CurrentTenantId { get; }
}
