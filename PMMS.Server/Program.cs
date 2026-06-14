using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Extensions;
using PMMS.Server.Domain.Entities;
using Microsoft.OpenApi;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Common.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Services Configuration
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
        });
        return Task.CompletedTask;  
    });
});

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddExpirationTrackingJobs();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Update with your frontend production URL later
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UsePathBase("/api/v1");
app.UseExceptionHandler();

// Development Tools
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "PMMS - PMMS API";
        options.Theme = ScalarTheme.Default;
    });
}

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<PmmsDbSeeder>();

    await seeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<PmmsDbContext>(),
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()
    );
}

// Middleware & Endpoints
app.UseCors("ApiCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");
app.MapEndpoints();

app.Run();