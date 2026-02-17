using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BackgroundServices.Jobs;

/// <summary>
/// Background job that performs cleanup operations on refresh tokens.
/// 
/// Operations:
/// 1. Revoke all expired refresh tokens that haven't been revoked yet
/// 2. Delete refresh tokens that expired more than 6 months ago
/// 
/// Schedule: Daily at 02:00 AM (UTC)
/// Configuration: See DependencyInjection.JobConfiguration for cron schedule and description
/// </summary>
[DisallowConcurrentExecution]
public sealed class TokenCleanupJob : IBackgroundJob
{
    public static string Description => "Cleanup and reconciliation of expired refresh tokens";
    public static string CronExpression => "0 0 2 * * ?"; // Daily at 02:00 AM UTC (Quartz format: SS MM HH D M DW)

    private readonly TimeSpan _oldTokenRetentionPeriod = TimeSpan.FromDays(180); // 6 months
    private readonly AuthDbContext _context;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(AuthDbContext context, ILogger<TokenCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("TokenCleanupJob: Starting token cleanup operation");

        try
        {
            var now = DateTime.UtcNow;
            
            // Step 1: Mark expired tokens as revoked (if not already)
            await RevokeExpiredTokensAsync(now);
            
            // Step 2: Delete very old tokens (expired more than 6 months ago)
            await DeleteOldTokensAsync(now);
            
            _logger.LogInformation("TokenCleanupJob: Completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TokenCleanupJob: Error occurred during token cleanup");
            throw;
        }
    }

    /// <summary>
    /// Revokes all expired refresh tokens that haven't been revoked yet.
    /// </summary>
    private async Task RevokeExpiredTokensAsync(DateTime now)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < now && rt.RevokedAt == null)
            .ToListAsync();

        if (expiredTokens.Count == 0)
        {
            _logger.LogInformation("TokenCleanupJob: No expired active tokens to revoke");
            return;
        }

        _logger.LogInformation(
            "TokenCleanupJob: Found {Count} expired active tokens to revoke",
            expiredTokens.Count);

        // Mark all expired tokens as revoked
        foreach (var token in expiredTokens)
        {
            token.RevokedAt = now;
            _context.Entry(token).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "TokenCleanupJob: Successfully revoked {Count} expired tokens",
            expiredTokens.Count);
    }

    /// <summary>
    /// Deletes refresh tokens that expired more than the retention period ago.
    /// This helps keep the database clean and performant.
    /// </summary>
    private async Task DeleteOldTokensAsync(DateTime now)
    {
        var cutoffDate = now.Subtract(_oldTokenRetentionPeriod);

        _logger.LogInformation(
            "TokenCleanupJob: Looking for tokens expired before {CutoffDate}",
            cutoffDate);

        // Find tokens that expired more than 6 months ago
        var oldTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoffDate)
            .ToListAsync();

        if (oldTokens.Count == 0)
        {
            _logger.LogInformation("TokenCleanupJob: No old tokens to delete");
            return;
        }

        _logger.LogInformation(
            "TokenCleanupJob: Found {Count} old tokens to delete (expired before {CutoffDate})",
            oldTokens.Count,
            cutoffDate);

        // Delete in batches to avoid memory issues with large datasets
        const int batchSize = 1000;
        var totalDeleted = 0;

        for (int i = 0; i < oldTokens.Count; i += batchSize)
        {
            var batch = oldTokens.Skip(i).Take(batchSize);
            _context.RefreshTokens.RemoveRange(batch);
            await _context.SaveChangesAsync();
            
            var batchCount = batch.Count();
            totalDeleted += batchCount;
            
            _logger.LogDebug(
                "TokenCleanupJob: Deleted batch of {Count} tokens ({Total}/{Total})",
                batchCount,
                totalDeleted,
                oldTokens.Count);
        }

        _logger.LogInformation(
            "TokenCleanupJob: Successfully deleted {Count} old tokens",
            totalDeleted);
    }
}
