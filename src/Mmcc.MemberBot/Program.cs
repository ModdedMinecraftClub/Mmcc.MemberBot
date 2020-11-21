using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Mmcc.ApplicationParser;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure;
using Mmcc.MemberBot.Infrastructure.Commands.Applications;
using Mmcc.MemberBot.Infrastructure.HostedServices;
using Mmcc.MemberBot.Infrastructure.Queries.Applications;
using Mmcc.MemberBot.Infrastructure.Services;
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
                    builder.AddJsonFile("simplecommands.json", optional: true);
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
                    services.Configure<PolychatSettings>(context.Configuration.GetSection("Polychat"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<PolychatSettings>>().Value);
                    services.Configure<SimpleCommandsSettings>(context.Configuration.GetSection("SimpleCommands"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<SimpleCommandsSettings>>().Value);
                    
                    // add application parser;
                    services.AddSingleton(provider => new MmccApplicationSerializer(true));
                    
                    // add db connection;
                    services.AddTransient(provider =>
                    {
                        var config = provider.GetRequiredService<MySqlSettings>();
                        var connString =
                            $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password};Allow User Variables=True";
                        return new MySqlConnection(connString);
                    });
                    
                    // add TcpClient for communication with Polychat;
                    services.AddTransient(provider =>
                    {
                        var config = provider.GetRequiredService<PolychatSettings>();
                        var tcpClient = new TcpClient(config.ServerIp, config.Port)
                        {
                            SendBufferSize = config.BufferSize
                        };
                        return tcpClient;
                    });
                    services.AddTransient<ITcpCommunicationService, TcpCommunicationService>();
                    
                    // add MediatR;
                    services.AddMediatR(typeof(Program), typeof(CreateNewApplication), typeof(GetApplicationById));
                    
                    // add Discord services;
                    services.AddSingleton<IBotService, BotHostedService>();
                    services.AddSingleton(provider =>
                    {
                        var commandService = new CommandService(new CommandServiceConfig
                        {
                            CaseSensitiveCommands = false,
                            DefaultRunMode = RunMode.Sync,
                            LogLevel = LogSeverity.Verbose
                        });
                        
                        // add modules;
                        commandService.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
                        
                        // add simple commands;
                        var simpleCommands = provider.GetService<SimpleCommandsSettings>()?.Commands;

                        if (simpleCommands is not null)
                        {
                            commandService.AddSimpleCommands(simpleCommands);
                        }
                        
                        return commandService;
                    });
                    services.AddTransient<IModerationService, ModerationService>();

                    // add hosted services;
                    services.AddHostedService<LifetimeEventsHostedService>();
                    services.AddHostedService(provider => provider.GetRequiredService<IBotService>());
                    services.AddHostedService<CommandHostedService>();
                });

            using var builtHost = host.Build();
            await builtHost.StartAsync();
            await builtHost.WaitForShutdownAsync();
        }
    }
}
