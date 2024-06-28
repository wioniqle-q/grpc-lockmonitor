using System.Collections.Concurrent;
using LockMonitor.Abstractions;
using LockMonitor.Enums;
using LockMonitor.Interfaces;
using LockMonitor.Utilities;

namespace LockMonitor.Entries;

internal sealed class LockEntry : LockEntryAbstract
{
    private static readonly bool[,] CompatibilityMatrix =
    {
        //    NL   IS   IX    S    SIX   X
        { true, true, true, true, true, false }, // NL
        { true, true, false, true, false, false }, // IS
        { true, false, false, false, false, false }, // IX
        { true, true, false, true, false, false }, // S
        { true, false, false, false, false, false }, // SIX
        { false, false, false, false, false, false } // X
    };

    private readonly SemaphoreSlim _entrySemaphore = new(1, 1);

    private readonly ConcurrentDictionary<LockMode, int> _grantedLocks = new();
    private readonly ConcurrentQueue<ILockRequest> _waitingQueue = new();

    public override async Task<bool> CanGrantLockAsync(LockMode lockMode)
    {
        await _entrySemaphore.WaitAsync();
        try
        {
            return _grantedLocks.All(grantedLock => CompatibilityMatrix[(int)grantedLock.Key, (int)lockMode]);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[LockEntry] CanGrantLockAsync failed. Exception: {e.Message}");
            return false;
        }
        finally
        {
            _entrySemaphore.Release();
        }
    }

    public override async Task GrantLockAsync(LockMode lockMode)
    {
        await _entrySemaphore.WaitAsync();
        try
        {
            if (_grantedLocks.TryAdd(lockMode, 1) is not false) _grantedLocks[lockMode]++;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[LockEntry] GrantLockAsync failed. Exception: {e.Message}");
        }
        finally
        {
            _entrySemaphore.Release();
        }
    }

    public override async Task ReleaseLockAsync(LockMode lockMode)
    {
        await _entrySemaphore.WaitAsync();
        try
        {
            if (_grantedLocks.TryGetValue(lockMode, out var value))
            {
                _grantedLocks[lockMode] = --value;

                if (value is 0) _grantedLocks.TryRemove(lockMode, out _);
            }

            await GrantWaitingLocksAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[LockEntry] ReleaseLockAsync failed. Exception: {e.Message}");
        }
        finally
        {
            _entrySemaphore.Release();
        }
    }

    public override bool WaitingQueueTimeout(TimeSpan timeout)
    {
        if (_waitingQueue.TryPeek(out var request) is not false) return false;
        return DateTime.UtcNow - request!.EnqueuedTime > timeout;
    }

    public override async Task<bool> EnqueueLockRequestAsync(long transactionId, LockMode lockMode)
    {
        try
        {
            var request = new LockRequest(transactionId, lockMode, DateTime.UtcNow);
            _waitingQueue.Enqueue(request);

            await GrantWaitingLocksAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[LockEntry] EnqueueLockRequestAsync failed. Exception: {e.Message}");
            return false;
        }
    }

    public override async Task GrantWaitingLocksAsync()
    {
        try
        {
            while (_waitingQueue.TryPeek(out var request) && await CanGrantLockAsync(request.LockMode))
            {
                _waitingQueue.TryDequeue(out request);
                await GrantLockAsync(request!.LockMode);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[LockEntry] GrantWaitingLocksAsync failed. Exception: {e.Message}");
        }
    }
}