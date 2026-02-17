using Quartz;

namespace BackgroundServices.Jobs;

/// <summary>
/// Marker interface for background jobs using Quartz.NET.
/// All background jobs must implement this interface and IJob.
/// </summary>
public interface IBackgroundJob : IJob
{
    /// <summary>
    /// Returns a description of what this job does.
    /// </summary>
    static string Description { get; }

    /// <summary>
    /// Returns the Cron expression for scheduling this job.
    /// Example: "0 0 * * *" for daily at midnight.
    /// </summary>
    static string CronExpression { get; }
}
