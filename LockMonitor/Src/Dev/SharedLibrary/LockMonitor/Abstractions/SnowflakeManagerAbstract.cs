using LockMonitor.Interfaces;

namespace LockMonitor.Abstractions;

public abstract class SnowflakeManagerAbstract : ISnowflake
{
    public abstract long NextId();
}