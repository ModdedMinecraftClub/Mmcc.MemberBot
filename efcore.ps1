param(
    [Parameter(Mandatory=$true)][string]$db_name,
    [Parameter(Mandatory=$true)][string]$username,
    [Parameter(Mandatory=$true)][SecureString]$password
)

$starter_location = Get-Location
$password_regular = ConvertFrom-SecureString $password -AsPlainText

try {
    # Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;
    Set-Location ./src/Mmcc.MemberBot.Core
    dotnet ef dbcontext scaffold "Server=localhost;Database=$db_name;Uid=$username;Pwd=$password_regular;" Pomelo.EntityFrameworkCore.MySql --context MemberBotContext --context-dir Data --output-dir Data.Models
}
finally {
    Set-Location $starter_location
}