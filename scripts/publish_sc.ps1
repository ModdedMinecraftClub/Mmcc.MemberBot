# Publishes self-contained versions of the app for Linux and Windows
. ./helper.ps1

$ErrorActionPreference = "Stop"

$starterLocation = Get-Location

try {
    Set-Location ..
    Build-Sc
} finally {
    Set-Location $starterLocation
}
