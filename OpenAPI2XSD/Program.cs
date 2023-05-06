using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenAPI2XSD.Parser;

namespace OpenAPI2XSD
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IHost host = CreateHostBuilder(args).Build();
                OpenAPI2XSD openAPI2XSDService = host.Services.GetRequiredService<OpenAPI2XSD>();
                openAPI2XSDService.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<OpenAPI2XSD>();
                    services.AddSingleton<ParserFactory>();
                    services.AddTransient<OpenApi2XsdParser>();
                })
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder
                        .AddFilter("Default", LogLevel.Debug)
                        .AddFilter("Microsoft", LogLevel.Debug)
                        .AddFilter("System", LogLevel.Warning)
                        .SetMinimumLevel(LogLevel.Debug)
                        .AddSimpleConsole(options =>
                        {
                            options.SingleLine = true;
                            options.TimestampFormat = "HH:mm:ss |";
                        });
                })
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddCommandLine(args);
                });
    }
}