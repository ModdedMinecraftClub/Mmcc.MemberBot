using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using ModdedMinecraftClub.MemberBot.Bot.Services;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    internal class Program
    {
        internal static ConfigRoot Config { get; private set; }

        private static async Task Main()
            => await new Program().MainAsync();

        private async Task MainAsync()
        {
            Config = Helper.LoadConfigFile();
            
            Startup();

            await using var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;
            
            await client.LoginAsync(TokenType.Bot, Config.Discord.Token);
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
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }

        private void Startup()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            
            Console.WriteLine("MMCC Member Bot v4.0\n");
            
            using var c = new DatabaseConnection();
            
            var exists = c.DoesTableExist();
            
            Console.WriteLine($"[{DateTime.Now}] Checking if \"applications\" table exists...\n");

            if (!exists)
            {
                Console.WriteLine($"[{DateTime.Now}] Couldn't find the table. Creating...");
                
                c.CreateTable();
                
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