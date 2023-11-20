using System;
using ClientLibrary;
using ClientLibrary.inputs;
using Microsoft.Extensions.Logging;

namespace SampleConsumer.Controllers
{
    public class ApplicationController : ShardRecordsProcessorBase<ApplicationController>, IShardRecordProcessor
    {
        public ApplicationController(ILogger<ApplicationController> logger) : base(logger)
        {
        }
        
        protected override void Process(ProcessRecordsInput input)
        {
            foreach (var rec in input.Records)
            {
                var data = string.Empty;
                
                try
                {
                    data = System.Text.Encoding.UTF8.GetString(rec.Data);
                    _logger.LogInformation("IMIN");
                    _logger.LogInformation(data);

                    // Your own logic to process a record goes here.
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception processing record data: {Data}", data);
                }
            }
        }
    }
}