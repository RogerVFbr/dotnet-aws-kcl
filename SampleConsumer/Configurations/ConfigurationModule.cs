using System;
using System.IO;
using ClientLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleConsumer.Controllers;
using SampleConsumer.Services;
using Serilog;

namespace SampleConsumer.Configurations;

public class ConfigurationModule
{
    private const string ApplicationName = "ApplicationName"; 
    public static IHost BuildHost(string[] args) => Host
        .CreateDefaultBuilder(args)
        .ConfigureServices(AddServices)
        .ConfigureAppConfiguration((_, config) =>
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var workingDirectory = Environment.CurrentDirectory;
            config
                .SetBasePath(Directory.GetParent(workingDirectory)!.Parent!.Parent!.Parent!.FullName)
                .AddJsonFile($"{nameof(SampleConsumer)}/bin/Debug/netcoreapp6.0/appsettings.json", optional: false);
        })
        .ConfigureLogging((_, builder) =>
        {
            var logModeString = Environment.GetEnvironmentVariable("LOG_MODE") == "string";
            Log.Logger = SerilogConfiguration.Get(logModeString, ApplicationName);
            builder.ClearProviders();
            builder.AddSerilog();
        })
        .Build();

    private static void AddServices(IServiceCollection services)
    {
        services.AddHostedService<ApplicationInitializerHostedService>();

        services.AddSingleton<IShardRecordProcessor, ApplicationController>();
    }
}