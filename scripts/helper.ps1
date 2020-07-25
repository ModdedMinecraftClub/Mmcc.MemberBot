$csprojLocation = "./ModdedMinecraftClub.MemberBot/ModdedMinecraftClub.MemberBot.Bot/ModdedMinecraftClub.MemberBot.Bot.csproj"
$platforms = @("win-x64", "linux-x64")

function Get-OutputLocation {
    param([String]$platform)

    return "./out/" + $platform
}

function Build-Sc {    
    foreach ($platform in $platforms) {
        $output = Get-OutputLocation($platform)
        dotnet publish $csprojLocation -c Release -r $platform --output $output
    }
}

function Build-Portable {
    $output = Get-OutputLocation("portable-fxdependent")
    dotnet publish $csprojLocation -c Release --output $output
}