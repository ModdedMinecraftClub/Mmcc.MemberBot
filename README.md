# MMCC Member Bot
[![License](https://img.shields.io/badge/license-GPL--3.0-blue?style=flat-square)](https://github.com/ModdedMinecraftClub/mmcc-member-bot/blob/master/LICENSE)
[![Framework](https://img.shields.io/badge/framework-.NET%205-blueviolet?style=flat-square)](https://dotnet.microsoft.com/download)

A Discord bot for managing MMCC Member Applications.

## Commands

To see all the available commands simply use the `help` command in a server with the bot.

## Dependencies

- [ModdedMinecraftClub/polychat2](https://github.com/ModdedMinecraftClub/polychat)
- [.NET 5 (only if using the `portable-fxdependent` version or building from source)](https://dotnet.microsoft.com/download)
- [MySQL >=8.0 or MariaDB equivalent](https://www.mysql.com/)

## Deployment

1. Deploy [polychat2](https://github.com/ModdedMinecraftClub/polychat2).
2. Create a MySQL database for the bot.
3. Download the latest compiled version of the bot from [here](https://github.com/ModdedMinecraftClub/Mmcc.MemberBot/releases). If you choose the `portable-fxdependent` version of the bot, you will need to also download the newest version of .NET from [here](https://dotnet.microsoft.com/download).
4. Unzip the compressed version of the bot you've downloaded.
5. Go to the folder with the unzipped binaries.
6. Rename `appsettings.default.json` to `appsettings.json` and edit it so that it contains correct values for your use-case scenario.
7. On Linux launch `Mmcc.MemberBot` executable, on Windows launch `Mmcc.MemberBot.exe`, or if you've chosen the portable version of the bot run the `dotnet Mmcc.MemberBot.dll` command.

## Building a production build from source

1. Make sure you have .NET installed, and if not get the latest build from [here](https://dotnet.microsoft.com/download).
2. Clone this repository.
3. In the main directory run `dotnet publish ./src/Mmcc.MemberBot -r linux-x64 -c Release --output ./out`. If you want a single file build instead, run the included `publish.ps1` script or run `dotnet publish ./src/Mmcc.MemberBot -r linux-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesInSingleFile=true --output ./out` and delete the `.pdb` files.

Your compiled binaries will be in `./out`.
