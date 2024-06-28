using LockMonitor.Enums;
using LockMonitor.Interfaces;

namespace LockMonitor.Abstractions;

public abstract class TransactionEntryAbstract : ITransactionEntry
{
    public abstract Task AddLock(int resourceId, LockMode lockMode);
    public abstract Task RemoveLock(int resourceId);
}