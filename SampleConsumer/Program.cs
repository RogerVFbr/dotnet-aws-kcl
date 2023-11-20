using Microsoft.Extensions.Hosting;
using SampleConsumer.Configurations;

await ConfigurationModule.BuildHost(args).RunAsync();
