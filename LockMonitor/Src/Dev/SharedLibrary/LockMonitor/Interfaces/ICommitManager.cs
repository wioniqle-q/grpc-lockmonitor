using LockMonitor.Enums;

namespace LockMonitor.Interfaces;

public interface ICommitManager
{
    public Task<bool> PreparePhaseAsync(HashSet<int> resourceIds, LockMode lockMode);
    public Task CommitPhaseAsync(HashSet<int> resourceIds, long transactionId, LockMode lockMode);
}