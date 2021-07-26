param (
	[string]$SqlServerDb=".\",
	
	[string]$localScriptRoot = ".\Upgrade",
	
	[switch]$sqlAuth=$true,
	
	[string]$sqlUserName="jenkins_sql_usr",
	
	[string]$sqlPwd,
	
	[string]$dbName="Wolf"
)

IF (!(Get-Module -Name SqlServer))
{
	Write-Host 'Loading SqlServer  Module' -ForegroundColor DarkYellow
    Import-Module SqlServer
}
if($sqlAuth)
{
	$auth=@{UserName=$sqlUserName;Password=$sqlPwd}
    Write-Host "SQL Authenticated with [$($sqlUserName)] user"
}
else
{
	$auth=@{}
    Write-Host "Windows Authenticated"
}

$scripts = Get-ChildItem $localScriptRoot | Where-Object {$_.Extension -eq ".sql"}
foreach ($s in $scripts)
{
	Write-Host "Running Script : " $s.Name -BackgroundColor DarkGreen -ForegroundColor White
    $script = $s.FullName
    #Invoke-Sqlcmd -ServerInstance $SqlServerDb -Database $dbName -Username $sqlUserName -Password $sqlPwd -InputFile $script
	Invoke-Sqlcmd -ServerInstance $SqlServerDb -Database $dbName -InputFile $script @Auth
}