using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PMMS.Server.Common.Constants;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Authentication;


public sealed class Login : IEndpoint {

    public record Command(string Email, string Password) : IRequest<AppResult<Response>>;


    public record Response(
        string Token, 
        string Email, 
        string Message);


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Must(email => email.EndsWith("@dhsud.hredrd.rv", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Only official DHSUD-HREDRD emails are permitted.");

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("log-in", async (Command command, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);

                return result.IsSuccess 
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("Login")
            .WithTags("Authentication")
            .WithSummary("Login User")
            .RequireRateLimiting("20")
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }


    public class Handler(
        PmmsDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration) : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            Console.WriteLine($"ssssssssssssssss: Password={command.Password}, Email={command.Email}");

            var user = await userManager.FindByEmailAsync(command.Email);

            // 1. Check if email exist
            if (user == null) 
            {
                return AppResult<Response>.Failure("Invalid email or password.", ErrorType.Unauthorized);
            }

            // 2. Verify if the account is not soft deleted
            if (user.IsDeleted == true)
            {
                return AppResult<Response>.Failure("Invalid email or password.", ErrorType.Unauthorized);
            }

            Console.WriteLine($"ssssssssssssssss: Password={command.Password}, Email={command.Email}");
            
            // 3. Verify given password and lockout on failure
            var signInResult = await signInManager.CheckPasswordSignInAsync(
                user,
                command.Password,
                lockoutOnFailure: true);

            if (!signInResult.Succeeded)
            {
                return AppResult<Response>.Failure("Invalid email or password.", ErrorType.Unauthorized);
            }

            // 4. Check if User have an assigned province given by Admin
            var roles = await userManager.GetRolesAsync(user);

            var assignedProvinces = await context.Assignments
                .AsNoTracking()
                .Where(a => a.UserId == user.Id)
                .Select(a => a.ProvinceId)
                .ToListAsync(ct);

            if (assignedProvinces.Count == 0 &&
                roles.Any(r => r.Equals(UserRoles.Permanent, StringComparison.OrdinalIgnoreCase)))
            {
                return AppResult<Response>.Failure(
                    "No assignment found. An Admin assignment is required in order to proceed.", 
                    ErrorType.Forbidden);
            }

            // 5. Create a JWT token
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(assignedProvinces.Select(pid => new Claim("assigned_province", pid.ToString())));

            var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
            var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
            var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var res = new Response(
                Token: tokenString,
                Email: user.Email ?? string.Empty,
                Message: "Logged in successfully."
            );
            return AppResult<Response>.Success(res);
        }
    }
}