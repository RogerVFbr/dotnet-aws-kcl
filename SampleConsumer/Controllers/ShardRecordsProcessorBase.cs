using System;
using ClientLibrary;
using ClientLibrary.inputs;
using Microsoft.Extensions.Logging;
// ReSharper disable ContextualLoggerProblem

namespace SampleConsumer.Controllers;

public abstract class ShardRecordsProcessorBase<T>
{
    protected readonly ILogger<T> _logger;
    
    /// <value>The time to wait before this record processor
    /// reattempts either a checkpoint, or the processing of a record.</value>
    protected readonly TimeSpan Backoff = TimeSpan.FromSeconds(3);

    /// <value>The interval this record processor waits between
    /// doing two successive checkpoints.</value>
    protected readonly TimeSpan CheckpointInterval = TimeSpan.FromMinutes(1);

    /// <value>The maximum number of times this record processor retries either
    /// a failed checkpoint, or the processing of a record that previously failed.</value>
    protected readonly int NumRetries = 10;

    /// <value>The shard ID on which this record processor is working.</value>
    protected string _kinesisShardId;

    /// <value>The next checkpoint time expressed in milliseconds.</value>
    protected DateTime _nextCheckpointTime = DateTime.UtcNow;

    private Checkpointer _latestCheckpointer = null;
    
    public ShardRecordsProcessorBase(ILogger<T> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// This method is invoked by the Amazon Kinesis Client Library before records from the specified shard
    /// are delivered to this SampleRecordProcessor.
    /// </summary>
    /// <param name="input">
    /// InitializationInput containing information such as the name of the shard whose records this
    /// SampleRecordProcessor will process.
    /// </param>
    public void Initialize(InitializationInput input)
    {
        _logger.LogInformation("Initializing record processor for shard: {ShardId}", input.ShardId);
        this._kinesisShardId = input.ShardId;
    }
    
    /// <summary>
    /// This method processes the given records and checkpoints using the given checkpointer.
    /// </summary>
    /// <param name="input">
    /// ProcessRecordsInput that contains records, a Checkpointer and contextual information.
    /// </param>
    public void ProcessRecords(ProcessRecordsInput input)
    {
        // Process records and perform all exception handling.
        Process(input);

        // Checkpoint once every checkpoint interval.
        _latestCheckpointer = input.Checkpointer;
        if (DateTime.UtcNow < _nextCheckpointTime) return;
        Checkpoint();
    }

    protected abstract void Process(ProcessRecordsInput input);

    /// <summary>
    /// This checkpoints the specified checkpointer with retries.
    /// </summary>
    /// <param name="checkpointer">The checkpointer used to do checkpoints.</param>
    protected void Checkpoint()
    {
        _logger.LogInformation("Checkpointing shard {KinesisShardId}", _kinesisShardId);
        
        // You can optionally provide an error handling delegate to be invoked when checkpointing fails.
        // The library comes with a default implementation that retries for a number of times with a fixed
        // delay between each attempt. If you do not provide an error handler, the checkpointing operation
        // will not be retried, but processing will continue.
        _latestCheckpointer.Checkpoint(RetryingCheckpointErrorHandler.Create(NumRetries, Backoff));
        _nextCheckpointTime = DateTime.UtcNow + CheckpointInterval;
    }

    public void LeaseLost(LeaseLossInput leaseLossInput)
    {
        //
        // Perform any necessary cleanup after losing your lease.  Checkpointing is not possible at this point.
        //
        _logger.LogWarning("Lost lease on {KinesisShardId}", _kinesisShardId);
    }

    public void ShardEnded(ShardEndedInput shardEndedInput)
    {
        //
        // Once the shard has ended it means you have processed all records on the shard. To confirm completion the
        // KCL requires that you checkpoint one final time using the default checkpoint value.
        //
        _logger.LogInformation("All records for {KinesisShardId} have been processed, starting final checkpoint", _kinesisShardId);
        shardEndedInput.Checkpointer.Checkpoint();
    }

    public void ShutdownRequested(ShutdownRequestedInput shutdownRequestedInput)
    {
        _logger.LogInformation("Shutdown has been requested for {KinesisShardId}. Checkpointing", _kinesisShardId);
        shutdownRequestedInput.Checkpointer.Checkpoint();
    }
}