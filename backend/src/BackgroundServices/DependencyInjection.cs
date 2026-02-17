using BackgroundServices.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace BackgroundServices;

/// <summary>
/// Dependency injection extensions for background services.
/// Registers Quartz.NET scheduler and all background jobs.
/// </summary>
public static class DependencyInjection
{
    // Job configuration metadata - maps job types to their cron schedules and descriptions
    private static readonly Dictionary<Type, (string Description, string CronExpression)> JobConfiguration = new()
    {
        { typeof(TokenCleanupJob), (TokenCleanupJob.Description, TokenCleanupJob.CronExpression) }
    };

    /// <summary>
    /// Adds background services (Quartz.NET scheduled jobs) to the dependency injection container.
    /// Configures Quartz.NET with SQL Server persistence, clustering, and JSON serialization.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // Get connection string from configuration
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("SqlConnectionString");

        // Add Quartz.NET with SQL Server persistence and clustering
        services.AddQuartz(q =>
        {
            // Configure thread pool for job execution
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });

            // Use SQL Server for persistence with clustering
            q.UsePersistentStore(options =>
            {
                // Configure SQL Server provider
                options.UseSqlServer(cs =>
                {
                    cs.ConnectionString = connectionString!;
                    cs.TablePrefix = "[quartz].QRTZ_";
                });

                // Use JSON serialization for job data (instead of binary)
                options.UseNewtonsoftJsonSerializer();

                // Enable clustering for distributed job scheduling across multiple instances
                options.UseClustering(c =>
                {
                    // Check for misfired triggers on cluster checkin
                    c.CheckinInterval = TimeSpan.FromSeconds(10);
                    c.CheckinMisfireThreshold = TimeSpan.FromSeconds(30);
                });
            });

            // Register background jobs
            RegisterJob<TokenCleanupJob>(q);
        });

        // Add Quartz.NET hosted service for running the scheduler
        services.AddQuartzHostedService(options =>
        {
            // Wait for all running jobs to complete before shutting down
            options.WaitForJobsToComplete = true;
            // Ensure scheduler starts only after application is fully initialized
            options.AwaitApplicationStarted = true;
        });

        return services;
    }

    /// <summary>
    /// Registers a background job with Quartz.NET using the job's configuration.
    /// </summary>
    /// <typeparam name="TJob">The job type to register (must implement IBackgroundJob)</typeparam>
    /// <param name="q">The Quartz configuration builder</param>
    private static void RegisterJob<TJob>(IServiceCollectionQuartzConfigurator q)
        where TJob : class, IBackgroundJob
    {
        var jobType = typeof(TJob);
        var jobKey = new JobKey(jobType.Name, "background-jobs");

        // Get job configuration from the static dictionary
        if (!JobConfiguration.TryGetValue(jobType, out var config))
        {
            throw new InvalidOperationException(
                $"Job {jobType.Name} is not registered in JobConfiguration. " +
                $"Add its description and cron expression to the JobConfiguration dictionary.");
        }

        var (description, cronExpression) = config;

        // Register the job itself
        q.AddJob<TJob>(opts =>
            opts.WithIdentity(jobKey)
                .WithDescription(description));

        // Add trigger with the cron schedule
        q.AddTrigger(opts =>
            opts.ForJob(jobKey)
                .WithIdentity($"{jobType.Name}_trigger", "background-jobs")
                .WithCronSchedule(cronExpression)
                .WithDescription($"Trigger for: {description}"));
    }
}
