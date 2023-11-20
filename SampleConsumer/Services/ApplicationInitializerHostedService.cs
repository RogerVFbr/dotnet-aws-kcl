using System;
using System.Threading;
using System.Threading.Tasks;
using ClientLibrary;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleConsumer.Services;

public class ApplicationInitializerHostedService : IHostedService
{
    private readonly ILogger<ApplicationInitializerHostedService> _logger;
    private readonly IShardRecordProcessor _shardRecordProcessor;

    public ApplicationInitializerHostedService(
        IShardRecordProcessor shardRecordProcessor, 
        ILogger<ApplicationInitializerHostedService> logger)
    {
        _shardRecordProcessor = shardRecordProcessor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing Consumer ...");
        try
        {
            KclProcess.Create(_shardRecordProcessor).Run();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("ERROR: " + e);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping  Consumer ...");
    }
}