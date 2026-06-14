using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Constants;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence;

public sealed class PmmsDbSeeder(
    ILogger<PmmsDbSeeder> logger,
    IHostEnvironment env,
    IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<PmmsDbSeeder> _logger = logger;
    private readonly IHostEnvironment _env = env;
    private readonly IConfiguration _configuration = configuration;

    public async Task SeedAsync(
        PmmsDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        CancellationToken ct = default)
    {
        // Professional: schema by migrations, not EnsureCreated
        await context.Database.MigrateAsync(ct);

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedMunicipalitiesAsync(context, ct);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [UserRoles.Admin, UserRoles.Permanent, UserRoles.Temporary];

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed creating role '{role}': {errors}");
            }
        }
    }

    private async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = _configuration["Seed:Admin:Email"] ?? "admin@dhsud.hredrd.rv";
        var adminPassword = _configuration["Seed:Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Seed admin password is missing. Set Seed:Admin:Password.");

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsDeleted = false
            };

            var create = await userManager.CreateAsync(admin, adminPassword);
            if (!create.Succeeded)
            {
                var errors = string.Join("; ", create.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed creating admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
        {
            var roleAdd = await userManager.AddToRoleAsync(admin, UserRoles.Admin);
            if (!roleAdd.Succeeded)
            {
                var errors = string.Join("; ", roleAdd.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed assigning admin role: {errors}");
            }
        }
    }

    private async Task SeedMunicipalitiesAsync(PmmsDbContext context, CancellationToken ct)
    {
        if (await context.Municipalities.AnyAsync(ct)) return;

        var path = Path.Combine(_env.ContentRootPath, "Infrastructure", "Persistence", "SeedData", "ListOfMunicipalities.json");
        if (!File.Exists(path))
        {
            _logger.LogWarning("Municipality seed file not found: {Path}", path);
            return;
        }

        var json = await File.ReadAllTextAsync(path, ct);
        var municipalities = JsonSerializer.Deserialize<List<Municipality>>(json, JsonOptions);

        if (municipalities is null || municipalities.Count == 0) return;

        await context.Municipalities.AddRangeAsync(municipalities, ct);
        await context.SaveChangesAsync(ct);
    }
}