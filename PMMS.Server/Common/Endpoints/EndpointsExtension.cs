using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PMMS.Server.Common.Endpoints
{
    public static class EndpointExtensions
    {
        // 1. Find and register all IEndpoint classes in Dependency Injection
        public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
        {
            var serviceDescriptors = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(IEndpoint)))
                .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type));

            services.TryAddEnumerable(serviceDescriptors);
            return services;
        }

        // 2. Map all registered IEndpoints to the WebApplication
        public static IApplicationBuilder MapEndpoints(this WebApplication app)
        {
            var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(app);
            }

            return app;
        }
    }
}