using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using ModdedMinecraftClub.MemberBot.Bot.Services.Hosted;
using ModdedMinecraftClub.MemberBot.Bot.Services.Regular;

namespace ModdedMinecraftClub.MemberBot.Bot
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
                    builder.AddYamlFile("appsettings.yml", optional: false);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddDebug();
                    }

                    builder.AddConsole(c =>
                    {
                        c.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
                    });
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<BotSettings>(context.Configuration.GetSection("BotSettings"));
                    
                    services.AddScoped<IDatabaseConnectionService, DatabaseConnectionService>();

                    services.AddHostedService<StartupChecksHostedService>();
                    
                    services.AddSingleton<IBotService, BotHostedService>();
                    services.AddHostedService(provider => provider.GetRequiredService<IBotService>());
                    
                    services.AddSingleton(provider => new CommandService(new CommandServiceConfig
                    {
                        CaseSensitiveCommands = false,
                        DefaultRunMode = RunMode.Sync,
                        LogLevel = LogSeverity.Verbose
                    }));
                    
                    services.AddHostedService<CommandHostedService>();
                });

            using var builtHost = host.Build();
            await builtHost.StartAsync();
            await builtHost.WaitForShutdownAsync();
        }
    }
}