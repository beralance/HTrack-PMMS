using System.Reflection;
using FluentValidation;
using PMMS.Server.Common.Behaviors;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Exceptions;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Common.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<PmmsDbSeeder>();
        
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddEndpoints(Assembly.GetExecutingAssembly());

        services.AddCors(options =>
        {
            options.AddPolicy("Client", policy =>
                policy.WithOrigins(config["AllowedOrigins"] ?? "http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        return services;
    }
}