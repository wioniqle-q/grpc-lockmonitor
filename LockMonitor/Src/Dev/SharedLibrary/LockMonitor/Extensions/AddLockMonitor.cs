using LockMonitor.Interfaces;
using LockMonitor.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LockMonitor.Extensions;

public static class AddLockMonitor
{
    public static void AddLockService(this IServiceCollection services)
    {
        services.AddSingleton<ISnowflake, SnowflakeManager>(_ =>
        {
            const long workerId = 1L;
            const long dataCenterId = 1L;
            return new SnowflakeManager(workerId, dataCenterId);
        });
        services.TryAddSingleton<ILockManager, LockManager>();
        services.TryAddSingleton<ICommitManager, CommitManager>();
    }
}