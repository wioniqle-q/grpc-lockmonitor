using System.Diagnostics;
using Grpc.Net.Client;

namespace MonitorGrpcClient;

internal static class Program
{
    private const int NumberOfRequests = 1000 * 1000;
    private const string ServerAddress = "http://localhost:5062";
    private const int MaxDegreeOfParallelism = 100; 
    private static readonly object LockObject = new();

    private static async Task Main()
    {
        var httpClientHandler = new SocketsHttpHandler
        {
            MaxConnectionsPerServer = MaxDegreeOfParallelism,
            EnableMultipleHttp2Connections = true
        };

        var httpClient = new HttpClient(httpClientHandler);

        var channelOptions = new GrpcChannelOptions
        {
            HttpClient = httpClient,
            MaxReceiveMessageSize = 10 * 1024 * 1024, // 10 MB
            MaxSendMessageSize = 10 * 1024 * 1024, // 10 MB
            MaxRetryAttempts = 5
        };

        using var channel = GrpcChannel.ForAddress(ServerAddress, channelOptions);
        var client = new LockManager.LockManagerClient(channel);
        var semaphore = new SemaphoreSlim(MaxDegreeOfParallelism);
        const LockMode lockMode = LockMode.Nl;
        
        var stopwatch = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, NumberOfRequests).Select(async i =>
        {
            await semaphore.WaitAsync();

            try
            {
                var resourceId = i % 1000;

                var acquired = false;
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        var acquireLockResponse = await client.AcquireLockAsync(new LockRequest
                        {
                            TransactionId = i,
                            ResourceId = resourceId,
                            LockMode = lockMode
                        });
           
                        Console.WriteLine(
                            $"TransactionId: {i}, ResourceId: {resourceId}, AcquireLockResponse: {acquireLockResponse.Success}");

                        if (acquireLockResponse.Success)
                        {
                            acquired = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Acquire Lock Attempt {attempt + 1} failed for TransactionId: {i}, ResourceId: {resourceId}. Exception: {ex.Message}");
                    }

                    await Task.Delay(10);
                }

                if (acquired is not false)
                    lock (LockObject)
                    { }
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        stopwatch.Stop();
        Console.WriteLine($"Completed {NumberOfRequests} requests in {stopwatch.Elapsed.TotalSeconds} seconds.");
    }
}