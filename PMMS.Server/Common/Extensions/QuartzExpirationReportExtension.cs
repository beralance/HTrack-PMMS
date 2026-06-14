using PMMS.Server.Features.Expirations;
using Quartz;

namespace PMMS.Server.Common.Extensions;

public static class QuartzExpirationReportExtension
{
    public static IServiceCollection AddExpirationTrackingJobs(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("ExpirationCheckJob");
            q.AddJob<ExpirationCheckJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithCronSchedule("0 0 1 * * ?", cron => cron
                    .WithMisfireHandlingInstructionFireAndProceed()));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}