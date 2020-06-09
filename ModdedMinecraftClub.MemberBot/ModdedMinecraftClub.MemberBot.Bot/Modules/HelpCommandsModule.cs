﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    public class HelpCommandsModule : ModuleBase<SocketCommandContext>
    {
        private readonly char _prefix;
        
        public HelpCommandsModule(ConfigRoot configRoot)
        {
            _prefix = configRoot.Discord.Prefix;
        }
        
        [Command("help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            var embedBuilder = new EmbedBuilder();
            
            // Basic info
            embedBuilder.WithTitle("Help");
            embedBuilder.WithDescription("MMCC Member Bot Help");
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithThumbnailUrl("https://www.moddedminecraft.club/data/icon.png");
            
            // Commands for everyone
            embedBuilder.AddField($"{_prefix}pending", "See currently pending applications");
            embedBuilder.AddField($"{_prefix}approved", "See last 20 approved applications");
            embedBuilder.AddField($"{_prefix}rejected", "See last 20 rejected applications");
            embedBuilder.AddField($"{_prefix}view <application id>", "View a particular application");
            
            // Staff only commands
            embedBuilder.AddField($"{_prefix}approve <application id> <server prefix> <ign>", "Approve a particular application");
            embedBuilder.AddField($"{_prefix}approve <application id> manual",
                "Force mark an application as approved (player will not be promoted automatically, you will have to promote them manually but the application will be marked as approved and will be removed from the pending list)");
            embedBuilder.AddField($"{_prefix}reject <application id> <reason>", "Reject a particular application");

            await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }
    }
}