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
    // Access the User object from the current HTTP Request
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    // Extract the "sub" or "NameIdentifier" claim (Standard for User ID)
    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    // Extract the email claim
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    // Helper to check if the user is logged in
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => User?
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value) ?? [];

    public IEnumerable<int> AssignedProvinceIds => User?
        .FindAll("assigned_province")
        .Select(c => int.TryParse(c.Value, out var id) ? id : (int?)null)
        .Where(id => id.HasValue)
        .Select(id => id!.Value) ?? [];

    // A helper method that leverages the built-in logic of ClaimsPrincipal
    public bool IsInRole(string roleName) => User?.IsInRole(roleName) ?? false;
}