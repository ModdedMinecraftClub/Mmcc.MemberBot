# MMCC Member Bot
[![License](https://img.shields.io/badge/license-GPL--3.0-blue?style=flat-square)](https://github.com/ModdedMinecraftClub/mmcc-member-bot/blob/master/LICENSE)
[![Framework](https://img.shields.io/badge/framework-.NET%205-blueviolet?style=flat-square)](https://dotnet.microsoft.com/download)

A Discord bot for managing MMCC Member Applications.

## Commands

### Commands for everyone

- `help` shows all available commands
- `pending` see currently pending applications
- `approved` see last 20 approved applications
- `rejected` see last 20 rejected applications
- `view <application id>` view a particular application

### Staff-only commands

- `approve <application id> <server prefix> <ign>` approve a particular application
- `approve <application id> manual` force mark an application as approved (player will not be promoted automatically, you will have to promote them manually but the application will be marked as approved and will be removed from the pending list)
- `reject <application id> <reason>` reject a particular application

## Dependencies

- [ModdedMinecraftClub/polychat](https://github.com/ModdedMinecraftClub/polychat)
- [ModdedMinecraftClub/polychat-client](https://github.com/ModdedMinecraftClub/polychat)
- [.NET 5 (only if using the `portable-fxdependent` version)](https://dotnet.microsoft.com/download)
- [MySQL >=8.0](https://www.mysql.com/)

## Deployment

1. Deploy [polychat](https://github.com/ModdedMinecraftClub/polychat) and [polychat-client](https://github.com/ModdedMinecraftClub/polychat-client) and create a MySQL database for the bot.
2. Download the latest compiled version of the bot from [here](https://github.com/ModdedMinecraftClub/mmcc-member-bot/releases). If you choose the `portable-fxdependent` version of the bot, you will need to also download .NET 5 from [here](https://dotnet.microsoft.com/download/dotnet/5.0).
3. Unzip the compressed version of the bot you've downloaded.
4. Go to the folder with the unzipped binaries.
5. Edit `appconfig.yml` so that it contains correct values for your use-case scenario.
6. On Linux launch `ModdedMinecraftClub.MemberBot.Bot` executable, on Windows launch `ModdedMinecraftClub.MemberBot.Bot.exe`, or if you've chosen the portable version of the bot run the `dotnet ModdedMinecraftClub.MemberBot.Bot.dll` command.
