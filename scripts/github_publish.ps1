# Publishes self-contained and portable versions of the app for Linux and Windows and creates zip archives for GitHub Releases
param(
    [Parameter(Mandatory=$true)][string]$version
)

. ./helper.ps1

$ErrorActionPreference = "Stop"

$starterLocation = Get-Location

function Clear-Out {
    Remove-Item -Path "./out/*" -Recurse -Force
}

function Zip {
    $children = Get-ChildItem -Path ./out/

    foreach ($child in $children) {
        $dirname = $child.NameString
        $zipName = "member-bot_$version.$dirName.zip"

        Compress-Archive -Path "$child/*" -DestinationPath "./out/$zipName" -Force
    }
}

try {
    Set-Location ..
    Clear-Out
    Build-Sc
    Build-Portable
    Zip
} finally {
    Set-Location $starterLocation
}
