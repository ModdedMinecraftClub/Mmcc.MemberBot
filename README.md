# mmcc-member-bot
[![Build Status](https://travis-ci.org/ModdedMinecraftClub/mmcc-member-bot.svg?branch=master)](https://travis-ci.org/ModdedMinecraftClub/mmcc-member-bot)

A Discord bot for managing MMCC Member Applications.

## Commands
**Commands for everyone**
- `pending` see currently pending applications
- `approved` see last 20 approved applications
- `rejected` see last 20 rejected applications
- `view <application id>` view a particular application

**Staff-only commands**
- `approve <application id> <serverPrefix> <ign>` approve a particular application
- `reject <application id>` reject a particular application

## Dependencies
- polychat
- polychat-client
- .NET Core SDK

## Installation
1. Deploy [polychat](https://github.com/ModdedMinecraftClub/polychat) and [polychat-client](https://github.com/ModdedMinecraftClub/polychat-client).
2. Download .NET Core SDK from:
    - Linux: [here](https://dotnet.microsoft.com/download/linux-package-manager/rhel/sdk-current)
    - Windows: [here](https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.401-windows-x64-installer)
3. Clone this repo.
4. Run `build.sh` (Linux) or `build.ps1` (Windows). This will create a directory named `Bot` within the root directory of this repo with compiled C# binaries.
5. Rename `sample_config.yml` to `config.yml` and fill it in.
6. To run the bot navigate to the `Bot` directory and run the following command:
`dotnet ModdedMinecraftClub.MemberBot.Bot.dll`
