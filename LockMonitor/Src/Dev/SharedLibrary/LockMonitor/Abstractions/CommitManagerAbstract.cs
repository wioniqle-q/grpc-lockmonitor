using LockMonitor.Enums;
using LockMonitor.Interfaces;

namespace LockMonitor.Abstractions;

public abstract class CommitManagerAbstract : ICommitManager
{
    public abstract Task<bool> PreparePhaseAsync(HashSet<int> resourceIds, LockMode lockMode);
    public abstract Task CommitPhaseAsync(HashSet<int> resourceIds, long transactionId, LockMode lockMode);
}