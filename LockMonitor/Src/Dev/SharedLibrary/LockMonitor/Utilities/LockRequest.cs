using LockMonitor.Enums;
using LockMonitor.Interfaces;

namespace LockMonitor.Utilities;

public record LockRequest(long TransactionId, LockMode LockMode, DateTime EnqueuedTime) : ILockRequest
{
    public long TransactionId { get; } = TransactionId;
    public LockMode LockMode { get; } = LockMode;
    public DateTime EnqueuedTime { get; } = EnqueuedTime;
}