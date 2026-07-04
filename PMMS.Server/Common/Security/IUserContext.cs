using System.Security.Claims;

namespace PMMS.Server.Common.Security;

public interface IUserContext
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<int> AssignedProvinceIds { get; }
    bool IsInRole(string roleName);
}

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => User?
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value) ?? [];

    public IEnumerable<int> AssignedProvinceIds => User?
        .FindAll("assigned_province")
        .Select(c => int.TryParse(c.Value, out var id) ? id : (int?)null)
        .Where(id => id.HasValue)
        .Select(id => id!.Value) ?? [];

    public bool IsInRole(string roleName) => User?.IsInRole(roleName) ?? false;
}