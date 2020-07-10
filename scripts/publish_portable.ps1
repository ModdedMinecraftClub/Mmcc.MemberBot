# Publishes portable runtime dependent version of the app
. ./helper.ps1

$ErrorActionPreference = "Stop"

$starterLocation = Get-Location

try {
    Set-Location ..
    Build-Portable
} finally {
    Set-Location $starterLocation
}
