using System.Collections.Concurrent;
using LockMonitor.Abstractions;
using LockMonitor.Entries;
using LockMonitor.Enums;
using LockMonitor.Interfaces;

namespace LockMonitor.Managers;

public sealed class LockManager : LockManagerAbstract
{
    private readonly ConcurrentQueue<int> _deadlockDetectionList = new();

    private readonly ConcurrentDictionary<int, ILockEntry> _lockTable = new();
    private readonly ConcurrentDictionary<long, ITransactionEntry> _transactionTable = new();

    public override async Task<bool> AcquireLockAsync(long transactionId, int resourceId, LockMode lockMode)
    {
        try
        {
            var lockEntry = _lockTable.GetOrAdd(resourceId, _ => new LockEntry());

            if (await lockEntry.CanGrantLockAsync(lockMode))
            {
                Console.WriteLine($"Lock granted for transaction {transactionId} on resource {resourceId}.");
                await lockEntry.GrantLockAsync(lockMode);
                RegisterTransactionLock(transactionId, resourceId, lockMode);
                return true;
            }

            if (await lockEntry.EnqueueLockRequestAsync(transactionId, lockMode))
            {
                Console.WriteLine($"Lock request for transaction {transactionId} on resource {resourceId} enqueued.");
                _deadlockDetectionList.Enqueue(resourceId);
                return false;
            }

            Console.WriteLine($"Transaction {transactionId} enqueued for resource {resourceId}.");

            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"[LockManager] AcquireLockAsync failed for TransactionId: {transactionId}. Exception: {e.Message}");
            return false;
        }
    }

    public override async Task ReleaseLockAsync(long transactionId, int resourceId, LockMode lockMode)
    {
        try
        {
            if (_lockTable.TryGetValue(resourceId, out var lockEntry))
            {
                await lockEntry.ReleaseLockAsync(lockMode);
                UnregisterTransactionLock(transactionId, resourceId);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"[LockManager] ReleaseLockAsync failed for TransactionId: {transactionId}. Exception: {e.Message}");
        }
    }

    private void RegisterTransactionLock(long transactionId, int resourceId, LockMode lockMode)
    {
        try
        {
            var transactionEntry = _transactionTable.GetOrAdd(transactionId, _ => new TransactionEntry());
            transactionEntry.AddLock(resourceId, lockMode);
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"[LockManager] RegisterTransactionLock failed for TransactionId: {transactionId}. Exception: {e.Message}");
        }
    }

    private void UnregisterTransactionLock(long transactionId, int resourceId)
    {
        try
        {
            if (_transactionTable.TryGetValue(transactionId, out var transactionEntry))
                transactionEntry.RemoveLock(resourceId);
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"[LockManager] UnregisterTransactionLock failed for TransactionId: {transactionId}. Exception: {e.Message}");
        }
    }

    public override ConcurrentQueue<int> GetDeadlockDetectionList()
    {
        return _deadlockDetectionList;
    }

    public override ConcurrentDictionary<int, ILockEntry> GetLockTable()
    {
        return _lockTable;
    }
}