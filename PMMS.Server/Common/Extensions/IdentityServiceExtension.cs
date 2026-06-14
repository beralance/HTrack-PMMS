using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Common.Constants;

namespace PMMS.Server.Common.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PmmsDbContext>(options => 
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.User.RequireUniqueEmail = true;
        }) 
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<PmmsDbContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin))
            .AddPolicy("PermanentOnly", policy => policy.RequireRole(UserRoles.Permanent))
            .AddPolicy("TemporaryOnly", policy => policy.RequireRole(UserRoles.Temporary))
            .AddPolicy("Employee", policy => policy.RequireRole(UserRoles.Permanent, UserRoles.Temporary));

        return services;
    }
}