using FluentValidation;
using MediatR;

namespace PMMS.Server.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) 
        : IPipelineBehavior<TRequest, TResponse> 
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken ct)
        {
            // 1. Check if there are any validators registered for this specific request
            if (!validators.Any())
            {
                return await next(ct);
            }

            // 2. Run all validators in parallel
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, ct)));

            // 3. Collect all errors
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // 4. If any errors exist, throw the FluentValidation Exception
            // This will be caught by our GlobalExceptionHandler
            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            // 5. If everything is fine, proceed to the Handler
            return await next(ct);
        }
    }
}