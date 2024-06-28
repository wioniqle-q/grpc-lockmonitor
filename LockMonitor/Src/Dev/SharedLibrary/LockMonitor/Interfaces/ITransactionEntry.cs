using LockMonitor.Enums;

namespace LockMonitor.Interfaces;

internal interface ITransactionEntry
{
    public Task AddLock(int resourceId, LockMode lockMode);
    public Task RemoveLock(int resourceId);
}