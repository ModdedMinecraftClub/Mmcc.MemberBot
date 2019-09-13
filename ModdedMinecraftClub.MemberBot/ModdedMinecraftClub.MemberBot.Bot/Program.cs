using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Hangfire;
using Hangfire.MySql.Core;
using ModdedMinecraftClub.MemberBot.Bot.ConfigModels;
using ModdedMinecraftClub.MemberBot.Bot.Services;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    class Program
    {
        private BackgroundJobServer _server;
        internal static readonly ConfigRoot Config = Yaml.GetConfig();
        
        internal static DiscordSocketClient Client { get; private set; }

        private static async Task Main()
            => await new Program().MainAsync();

        private async Task MainAsync()
        {
            Console.WriteLine("Starting ModdedMinecraftClub.MemberBot.Bot...\n");
            
            StartupChecks();

            using (var services = ConfigureServices())
            {
                Client = services.GetRequiredService<DiscordSocketClient>();

                Client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                await Client.LoginAsync(TokenType.Bot, Config.Discord.Token);
                await Client.StartAsync();
                
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                
                Client.Ready += () =>
                {
                    GlobalConfiguration.Configuration
                        .UseColouredConsoleLogProvider()
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseStorage(new MySqlStorage(Helper.GetMySqlConnectionString()));

                    _server = new BackgroundJobServer();
                    
                    return Task.CompletedTask;
                };

                await Task.Delay(-1);
            }
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }

        private static void StartupChecks()
        {
            using (var c = new DatabaseConnection())
            {
                var exists = c.DoesTableExist();
                
                Console.WriteLine("Checking if \"applications\" table exists...\n");

                if (!exists)
                {
                    Console.WriteLine("Couldn't find the table. Creating...");
                    
                    c.CreateTable();
                    
                    Console.WriteLine("Successfully created the table. Starting the bot...\n");
                }
                else
                {
                    Console.WriteLine("Found the table. Starting the bot...\n");
                }
            }
        }
    }
}