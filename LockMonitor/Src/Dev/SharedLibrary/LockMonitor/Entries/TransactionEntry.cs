using System.Collections.Concurrent;
using LockMonitor.Abstractions;
using LockMonitor.Enums;

namespace LockMonitor.Entries;

public sealed class TransactionEntry : TransactionEntryAbstract
{
    private readonly ConcurrentDictionary<int, LockMode> _locks = new();

    public override async Task AddLock(int resourceId, LockMode lockMode)
    {
        try
        {
            await Task.Run(() => _locks[resourceId] = lockMode);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TransactionEntry] Error adding lock for resource {resourceId}. Exception: {e}");
        }
    }

    public override async Task RemoveLock(int resourceId)
    {
        try
        {
            await Task.Run(() => _locks.TryRemove(resourceId, out _));
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TransactionEntry] Error removing lock for resource {resourceId}. Exception: {e}");
        }
    }
}