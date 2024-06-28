using LockMonitor.Abstractions;
using LockMonitor.Enums;
using LockMonitor.Interfaces;

namespace LockMonitor.Managers;

public sealed class CommitManager(ILockManager lockManager, ISnowflake snowflake) : CommitManagerAbstract
{
    public override async Task<bool> PreparePhaseAsync(HashSet<int> resourceIds, LockMode lockMode)
    {
        var transactionId = snowflake.NextId();

        try
        {
            var lockTasks = resourceIds
                .Select(resourceId => lockManager.AcquireLockAsync(transactionId, resourceId, lockMode)).ToList();
            await Task.WhenAll(lockTasks);

            var releaseTasks = resourceIds
                .Select(resourceId => lockManager.ReleaseLockAsync(transactionId, resourceId, lockMode)).ToList();
            await Task.WhenAll(releaseTasks);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Prepare Phase failed for TransactionId: {transactionId}. Exception: {ex.Message}");
            await CommitPhaseAsync(resourceIds, transactionId, lockMode);
            return false;
        }
    }

    public override async Task CommitPhaseAsync(HashSet<int> resourceIds, long transactionId, LockMode lockMode)
    {
        var releaseTasks = resourceIds
            .Select(resourceId => lockManager.ReleaseLockAsync(transactionId, resourceId, lockMode)).ToList();
        await Task.WhenAll(releaseTasks);
    }
}