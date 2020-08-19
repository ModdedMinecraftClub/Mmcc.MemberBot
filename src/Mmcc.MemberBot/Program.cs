using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure.Commands;
using MySqlConnector;

namespace Mmcc.MemberBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddDebug();
                    }

                    builder.AddConsole(o =>
                    {
                        if (context.HostingEnvironment.IsProduction())
                        {
                            o.DisableColors = true;
                        }

                        o.Format = ConsoleLoggerFormat.Systemd;
                        o.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
                    });
                })
                .ConfigureServices((context, services) =>
                {
                    // add config;
                    services.Configure<MySqlSettings>(context.Configuration.GetSection("MySql"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<MySqlSettings>>().Value);
                    services.Configure<DiscordSettings>(context.Configuration.GetSection("Discord"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<DiscordSettings>>().Value);
                    
                    // add db connection;
                    services.AddTransient(provider =>
                    {
                        var config = provider.GetRequiredService<MySqlSettings>();
                        var connString =
                            $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password};Allow User Variables=True";
                        return new MySqlConnection(connString);
                    });
                    
                    // add MediatR;
                    services.AddMediatR(typeof(Program), typeof(CreateNewApplication));
                    
                    // add hosted services;
                    
                });

            using var builtHost = host.Build();
            await builtHost.StartAsync();
            await builtHost.WaitForShutdownAsync();
        }
    }
}
