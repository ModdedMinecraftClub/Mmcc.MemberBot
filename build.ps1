$ErrorActionPreference = "Stop"

Set-Location .\ModdedMinecraftClub.MemberBot
dotnet publish -c Release
Set-Location ..

function PrepareDir {
    param (
        [string]$dirName
    )
    
    if (Test-Path .\$dirName) {
        Set-Location .\$dirName
        
        Remove-Item * -Force -Recurse

        Set-Location ..
    } else {
        mkdir Bot
    }
}

PrepareDir("Bot")

[string]$source = ".\ModdedMinecraftClub.MemberBot\ModdedMinecraftClub.MemberBot.Bot\bin\Release\netcoreapp2.2\publish\*"

[string]$destination = ".\Bot\"

Copy-Item -Force -Recurse $source -Destination $destination

Write-Output "`n`nDone."