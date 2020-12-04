$starterLocation = Get-Location

If (Test-Path "./out") {
	Remove-Item "./out" -Recurse
}

dotnet publish ./src/Mmcc.MemberBot -r linux-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesInSingleFile=true --output ./out

Set-Location ./out
Get-ChildItem *.pdb -Recurse | ForEach-Object { Remove-Item -Path $_.FullName }
Set-Location $starterLocation