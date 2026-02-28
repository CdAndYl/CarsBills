[CmdletBinding()]
param(
    [string]$SqlServerHost = "",
    [int]$SqlServerPort = 0,
    [string]$SqlDatabase = "",
    [string]$SqlUser = "",
    [string]$SqlPassword = "",

    [string]$MySqlHost = "",
    [int]$MySqlPort = 0,
    [string]$MySqlDatabase = "",
    [string]$MySqlUser = "",
    [string]$MySqlPassword = "",

    [string]$MySqlCliPath = "mysql",
    [string]$OutputSqlFile = "",
    [int]$BatchSize = 500,
    [switch]$GenerateOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-Setting {
    param(
        [string]$ExplicitValue,
        [string]$EnvName,
        [string]$DefaultValue = "",
        [switch]$Required
    )

    if (-not [string]::IsNullOrWhiteSpace($ExplicitValue)) {
        return $ExplicitValue
    }

    $envValue = [Environment]::GetEnvironmentVariable($EnvName)
    if (-not [string]::IsNullOrWhiteSpace($envValue)) {
        return $envValue
    }

    if (-not [string]::IsNullOrWhiteSpace($DefaultValue)) {
        return $DefaultValue
    }

    if ($Required) {
        throw "Missing required value: '$EnvName'. Please set env var or pass parameter."
    }

    return ""
}

$explicitSqlServerPort = ""
if ($SqlServerPort -gt 0) {
    $explicitSqlServerPort = [string]$SqlServerPort
}

$explicitMySqlPort = ""
if ($MySqlPort -gt 0) {
    $explicitMySqlPort = [string]$MySqlPort
}

$resolvedSqlServerHost = Get-Setting -ExplicitValue $SqlServerHost -EnvName "CARSBILL_SQLSERVER_HOST" -DefaultValue "192.168.31.146" -Required
$resolvedSqlServerPortText = Get-Setting -ExplicitValue $explicitSqlServerPort -EnvName "CARSBILL_SQLSERVER_PORT" -DefaultValue "1433" -Required
$resolvedSqlDatabase = Get-Setting -ExplicitValue $SqlDatabase -EnvName "CARSBILL_SQLSERVER_DATABASE" -DefaultValue "shis2067" -Required
$resolvedSqlUser = Get-Setting -ExplicitValue $SqlUser -EnvName "CARSBILL_SQLSERVER_USER" -DefaultValue "shis" -Required
$resolvedSqlPassword = Get-Setting -ExplicitValue $SqlPassword -EnvName "CARSBILL_SQLSERVER_PASSWORD" -Required

$resolvedMySqlHost = Get-Setting -ExplicitValue $MySqlHost -EnvName "CARSBILL_MYSQL_HOST" -DefaultValue "127.0.0.1" -Required
$resolvedMySqlPortText = Get-Setting -ExplicitValue $explicitMySqlPort -EnvName "CARSBILL_MYSQL_PORT" -DefaultValue "3306" -Required
$resolvedMySqlDatabase = Get-Setting -ExplicitValue $MySqlDatabase -EnvName "CARSBILL_MYSQL_DATABASE" -DefaultValue "carsbill" -Required
$resolvedMySqlUser = Get-Setting -ExplicitValue $MySqlUser -EnvName "CARSBILL_MYSQL_USER" -DefaultValue "root" -Required
$resolvedMySqlPassword = Get-Setting -ExplicitValue $MySqlPassword -EnvName "CARSBILL_MYSQL_PASSWORD" -Required

[int]$resolvedSqlServerPort = [int]$resolvedSqlServerPortText
[int]$resolvedMySqlPort = [int]$resolvedMySqlPortText
$resolvedSqlServerInstance = "$resolvedSqlServerHost,$resolvedSqlServerPort"

if ([string]::IsNullOrWhiteSpace($OutputSqlFile)) {
    $OutputSqlFile = Join-Path $PSScriptRoot "output/sync_to_mysql.sql"
}

$syncScript = Join-Path $PSScriptRoot "sync_sqlserver_to_mysql.ps1"
if (-not (Test-Path -Path $syncScript)) {
    throw "Script not found: $syncScript"
}

$invokeParams = @{
    SqlServer     = $resolvedSqlServerInstance
    SqlDatabase   = $resolvedSqlDatabase
    SqlUser       = $resolvedSqlUser
    SqlPassword   = $resolvedSqlPassword
    MySqlHost     = $resolvedMySqlHost
    MySqlPort     = $resolvedMySqlPort
    MySqlDatabase = $resolvedMySqlDatabase
    MySqlUser     = $resolvedMySqlUser
    MySqlPassword = $resolvedMySqlPassword
    MySqlCliPath  = $MySqlCliPath
    OutputSqlFile = $OutputSqlFile
    BatchSize     = $BatchSize
}

if ($GenerateOnly) {
    $invokeParams.GenerateOnly = $true
}

& $syncScript @invokeParams
