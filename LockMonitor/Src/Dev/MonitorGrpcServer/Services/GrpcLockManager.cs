using Grpc.Core;
using LockMonitor.Interfaces;

namespace MonitorGrpcServer.Services;

public sealed class GrpcLockManager(ICommitManager commitManager) : LockManager.LockManagerBase
{
    public override async Task<LockResponse> AcquireLock(LockRequest request, ServerCallContext context)
    {
        var success =
            await commitManager.PreparePhaseAsync([request.ResourceId], (LockMonitor.Enums.LockMode)request.LockMode);
        return new LockResponse { Success = success };
    }
}