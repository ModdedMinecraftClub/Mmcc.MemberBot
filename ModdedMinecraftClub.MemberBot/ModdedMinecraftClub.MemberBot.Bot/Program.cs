using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using ModdedMinecraftClub.MemberBot.Bot.Services;
using ModdedMinecraftClub.MemberBot.Bot.Services.Regular;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    internal class Program
    {
        private static async Task Main()
            => await new Program().MainAsync();

        private async Task MainAsync()
        {
            var services = ConfigureServices();
            var config = services.GetRequiredService<ConfigRoot>();
            
            await Startup(config);
            
            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;
            
            await client.LoginAsync(TokenType.Bot, config.Discord.Token);
            await client.StartAsync();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await Task.Delay(-1);
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<ConfigRoot>(Helper.LoadConfigFile())
                .AddTransient<DatabaseConnectionService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
        
        /// <summary>
        /// Runs startup checks and creates table if needed
        /// </summary>
        /// <param name="config">Application config</param>
        private static async Task Startup(ConfigRoot config)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            
            Console.WriteLine("MMCC Member Bot v4.0\n");
            
            using var db = new DatabaseConnectionService(config);
            var exists = await db.DoesTableExistAsync();
            
            Console.WriteLine($"[{DateTime.Now}] Checking if \"applications\" table exists...\n");

            if (!exists)
            {
                Console.WriteLine($"[{DateTime.Now}] Couldn't find the table. Creating...");
                
                await db.CreateTableAsync();
                
                Console.WriteLine($"[{DateTime.Now}] Successfully created the table. Starting the bot...\n");
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now}] Found the table. Starting the bot...\n");
            }
            
            Console.ResetColor();
        }
    }
}