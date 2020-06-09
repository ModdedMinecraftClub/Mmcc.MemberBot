$ErrorActionPreference = "Stop"

Write-Output "Building mmcc-member-bot...`n`n"

.\build.ps1

Write-Output "`n`nBuilt. Starting...`n`n"

try {
    Set-Location .\Bot
    dotnet ModdedMinecraftClub.MemberBot.Bot.dll
}
finally {
    Set-Location ..
}
