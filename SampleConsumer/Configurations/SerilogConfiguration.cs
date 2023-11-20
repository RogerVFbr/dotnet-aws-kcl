using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Templates;

namespace SampleConsumer.Configurations;

public class SerilogConfiguration
{
    public static Logger Get(bool isLocal, string applicationName)
    {
        var unstructuredTemplate = "[{Level:u3}] {Message:lj} {Exception}{NewLine}";
        var structuredTemplate = new ExpressionTemplate("{ {Msg: @m, @l, @p} }\n");
        
        var config = new LoggerConfiguration()
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: unstructuredTemplate);

        return config.CreateLogger();
    }

}