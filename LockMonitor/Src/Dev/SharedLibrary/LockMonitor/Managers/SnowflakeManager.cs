using LockMonitor.Abstractions;

namespace LockMonitor.Managers;

public sealed class SnowflakeManager : SnowflakeManagerAbstract
{
    private const long Epoch = 1288834974657L; // (Nov 4, 2010)
    private const int WorkerIdBits = 5;
    private const int DataCenterIdBits = 5;
    private const int SequenceBits = 12;
    private const int WorkerIdMax = ~(-1 << WorkerIdBits);
    private const int DataCenterIdMax = ~(-1 << DataCenterIdBits);
    private const int SequenceMax = ~(-1 << SequenceBits);
    private const int WorkerIdShift = SequenceBits;
    private const int DataCenterIdShift = WorkerIdShift + WorkerIdBits;
    private const int TimestampShift = DataCenterIdShift + DataCenterIdBits;
    private readonly long _dataCenterId;
    private readonly long _workerId;
    private long _lastTimestamp;
    private long _sequence;

    public SnowflakeManager(long workerId, long dataCenterId)
    {
        if (workerId is < 0 or > WorkerIdMax)
            throw new ArgumentException("Worker ID must be between 0 and " + WorkerIdMax);

        if (dataCenterId is < 0 or > DataCenterIdMax)
            throw new ArgumentException("Data center ID must be between 0 and " + DataCenterIdMax);

        _workerId = workerId;
        _dataCenterId = dataCenterId;
        _sequence = 0;
        _lastTimestamp = -1;
    }

    public override long NextId()
    {
        var currentTimestamp = GetCurrentTimestamp();

        if (currentTimestamp < _lastTimestamp)
            throw new Exception("Clock moved backwards. Refusing to generate id for " +
                                (_lastTimestamp - currentTimestamp) + " milliseconds");

        if (currentTimestamp == _lastTimestamp)
        {
            _sequence = (_sequence + 1) & SequenceMax;

            if (_sequence is 0) currentTimestamp = WaitUntilNextMillis(currentTimestamp);
        }
        else
        {
            _sequence = 0;
        }

        _lastTimestamp = currentTimestamp;

        var id = ((currentTimestamp - Epoch) << TimestampShift) |
                 (_dataCenterId << DataCenterIdShift) |
                 (_workerId << WorkerIdShift) |
                 _sequence;

        return id;
    }

    private static long GetCurrentTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    private long WaitUntilNextMillis(long currentTimestamp)
    {
        var timestamp = currentTimestamp;
        while (timestamp <= _lastTimestamp) timestamp = GetCurrentTimestamp();
        return timestamp;
    }
}