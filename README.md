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
- `approve <application id>` approve a particular application
- `reject <application id>` reject a particular application

## Installation
1. Download .NET Core Runtime from:
    - Linux: [here](https://dotnet.microsoft.com/download/linux-package-manager/rhel/runtime-current)
    - Windows: [here](https://dotnet.microsoft.com/download/thank-you/dotnet-runtime-2.2.6-windows-hosting-bundle-installer)
2. Clone this repo.
3. Run `build.sh` (Linux) or `build.ps1` (Windows). This will create a directory named `Bot` within the root directory of this repo with compiled C# binaries.
4. Rename `sample_config.yml` to `config.yml` and fill it in.
5. To run the bot navigate to the `Bot` directory and run the following command:
`dotnet ModdedMinecraftClub.MemberBot.Bot.dll`
