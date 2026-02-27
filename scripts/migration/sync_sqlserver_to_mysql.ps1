[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SqlServer,

    [Parameter(Mandatory = $true)]
    [string]$SqlDatabase,

    [string]$SqlUser = "",
    [string]$SqlPassword = "",

    [string]$MySqlHost = "localhost",
    [int]$MySqlPort = 3306,

    [Parameter(Mandatory = $true)]
    [string]$MySqlDatabase,

    [Parameter(Mandatory = $true)]
    [string]$MySqlUser,

    [Parameter(Mandatory = $true)]
    [string]$MySqlPassword,

    [string]$MySqlCliPath = "mysql",
    [string]$OutputSqlFile = "scripts/migration/output/sync_to_mysql.sql",
    [int]$BatchSize = 500,

    [switch]$GenerateOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Convert-ToMySqlLiteral {
    param([object]$Value)

    if ($null -eq $Value -or $Value -is [System.DBNull]) {
        return "NULL"
    }

    if ($Value -is [DateTime]) {
        return "'" + $Value.ToString("yyyy-MM-dd HH:mm:ss") + "'"
    }

    if ($Value -is [bool]) {
        return $(if ($Value) { "1" } else { "0" })
    }

    if ($Value -is [byte] -or $Value -is [int16] -or $Value -is [int32] -or $Value -is [int64] -or
        $Value -is [uint16] -or $Value -is [uint32] -or $Value -is [uint64] -or
        $Value -is [single] -or $Value -is [double] -or $Value -is [decimal]) {
        return [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0}", $Value)
    }

    $text = [string]$Value
    $text = $text.Replace("\", "\\")
    $text = $text.Replace("'", "''")
    $text = $text.Replace("`r`n", "\n")
    $text = $text.Replace("`r", "\n")
    $text = $text.Replace("`n", "\n")
    return "'" + $text + "'"
}

function Invoke-SourceQuery {
    param(
        [string]$Query,
        [string]$Server,
        [string]$Database,
        [string]$User,
        [string]$Password
    )

    $params = @{
        ServerInstance = $Server
        Database       = $Database
        Query          = $Query
        QueryTimeout   = 0
        ErrorAction    = "Stop"
    }

    if (-not [string]::IsNullOrWhiteSpace($User)) {
        $params.Username = $User
        $params.Password = $Password
    }

    return @(Invoke-Sqlcmd @params)
}

function Write-UpsertBatches {
    param(
        [System.IO.StreamWriter]$Writer,
        [string]$Table,
        [string[]]$Columns,
        [string[]]$UpdateColumns,
        [object[]]$Rows,
        [int]$ChunkSize
    )

    if ($Rows.Count -eq 0) {
        return
    }

    $tick = [char]96
    $quotedColumns = ($Columns | ForEach-Object { "$tick$_$tick" }) -join ", "
    $updateSql = ($UpdateColumns | ForEach-Object { "$tick$_$tick = VALUES($tick$_$tick)" }) -join ", "

    for ($offset = 0; $offset -lt $Rows.Count; $offset += $ChunkSize) {
        $end = [Math]::Min($offset + $ChunkSize - 1, $Rows.Count - 1)
        $chunk = @($Rows[$offset..$end])
        $valueLines = New-Object System.Collections.Generic.List[string]

        foreach ($row in $chunk) {
            $values = foreach ($col in $Columns) {
                Convert-ToMySqlLiteral -Value $row.$col
            }
            $valueLines.Add("(" + ($values -join ", ") + ")")
        }

        $Writer.WriteLine("INSERT INTO $tick$Table$tick ($quotedColumns)")
        $Writer.WriteLine("VALUES")
        $Writer.WriteLine(($valueLines -join ",`n"))
        $Writer.WriteLine("ON DUPLICATE KEY UPDATE $updateSql;")
        $Writer.WriteLine("")
    }
}

function Ensure-Directory {
    param([string]$FilePath)
    $dir = Split-Path -Path $FilePath -Parent
    if (-not [string]::IsNullOrWhiteSpace($dir) -and -not (Test-Path -Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

$tablePlans = @(
    @{
        Name = "car_info"
        Optional = $false
        Columns = @("car_id", "license_plate", "owner_name", "phone_number", "created_at", "updated_at")
        UpdateColumns = @("license_plate", "owner_name", "phone_number", "created_at", "updated_at")
        Query = @"
SELECT
    CarID AS car_id,
    LicensePlate AS license_plate,
    CarOwnerName AS owner_name,
    PhoneNumber AS phone_number,
    ISNULL(CreatedAt, GETDATE()) AS created_at,
    ISNULL(UpdatedAt, GETDATE()) AS updated_at
FROM dbo.CarInfo
ORDER BY CarID;
"@
    },
    @{
        Name = "address_info"
        Optional = $false
        Columns = @("space_id", "space_name", "lookup_code", "created_at", "updated_at")
        UpdateColumns = @("space_name", "lookup_code", "created_at", "updated_at")
        Query = @"
SELECT
    SpaceID AS space_id,
    SpaceName AS space_name,
    sLookup AS lookup_code,
    ISNULL(CreatedAt, GETDATE()) AS created_at,
    ISNULL(UpdatedAt, GETDATE()) AS updated_at
FROM dbo.AddressInFo
ORDER BY SpaceID;
"@
    },
    @{
        Name = "material_info"
        Optional = $false
        Columns = @("material_id", "material_name", "lookup_code", "created_at", "updated_at")
        UpdateColumns = @("material_name", "lookup_code", "created_at", "updated_at")
        Query = @"
SELECT
    wLid AS material_id,
    wLsName AS material_name,
    sLookup AS lookup_code,
    ISNULL(CreatedAt, GETDATE()) AS created_at,
    ISNULL(UpdatedAt, GETDATE()) AS updated_at
FROM dbo.WLinFo
ORDER BY wLid;
"@
    },
    @{
        Name = "project_info"
        Optional = $false
        Columns = @("project_id", "project_name", "lookup_code", "created_at", "updated_at")
        UpdateColumns = @("project_name", "lookup_code", "created_at", "updated_at")
        Query = @"
SELECT
    XmID AS project_id,
    XmName AS project_name,
    sLookup AS lookup_code,
    ISNULL(CreatedAt, GETDATE()) AS created_at,
    ISNULL(UpdatedAt, GETDATE()) AS updated_at
FROM dbo.XmInFo
ORDER BY XmID;
"@
    },
    @{
        Name = "business_record"
        Optional = $false
        Columns = @(
            "business_id", "car_id", "license_plate", "business_date",
            "start_addr_id", "start_addr", "end_addr_id", "end_addr",
            "material_id", "material_name", "project_id", "project_name",
            "car_count", "price", "total_amount", "is_paid", "is_cancelled",
            "memo", "created_at", "updated_at"
        )
        UpdateColumns = @(
            "car_id", "license_plate", "business_date",
            "start_addr_id", "start_addr", "end_addr_id", "end_addr",
            "material_id", "material_name", "project_id", "project_name",
            "car_count", "price", "total_amount", "is_paid", "is_cancelled",
            "memo", "created_at", "updated_at"
        )
        Query = @"
SELECT
    b.businessID AS business_id,
    COALESCE(CASE WHEN ISNUMERIC(NULLIF(b.CarID, '')) = 1 THEN CAST(NULLIF(b.CarID, '') AS INT) ELSE NULL END, c.CarID, 0) AS car_id,
    b.LicensePlate AS license_plate,
    CAST(b.DateAct AS DATE) AS business_date,
    CASE WHEN ISNUMERIC(NULLIF(b.addr_startID, '')) = 1 THEN CAST(NULLIF(b.addr_startID, '') AS INT) ELSE NULL END AS start_addr_id,
    b.addr_start AS start_addr,
    CASE WHEN ISNUMERIC(NULLIF(b.addr_endID, '')) = 1 THEN CAST(NULLIF(b.addr_endID, '') AS INT) ELSE NULL END AS end_addr_id,
    b.addr_end AS end_addr,
    CASE WHEN ISNUMERIC(NULLIF(b.wLid, '')) = 1 THEN CAST(NULLIF(b.wLid, '') AS INT) ELSE NULL END AS material_id,
    b.wLsName AS material_name,
    CASE WHEN ISNUMERIC(NULLIF(b.XmID, '')) = 1 THEN CAST(NULLIF(b.XmID, '') AS INT) ELSE NULL END AS project_id,
    b.XmName AS project_name,
    ISNULL(b.CarCount, 0) AS car_count,
    ISNULL(b.price, 0) AS price,
    ISNULL(b.fsum, 0) AS total_amount,
    ISNULL(b.isDeal, 0) AS is_paid,
    ISNULL(b.isCanecl, 0) AS is_cancelled,
    b.sMemo AS memo,
    ISNULL(b.CreatedAt, GETDATE()) AS created_at,
    ISNULL(b.UpdatedAt, GETDATE()) AS updated_at
FROM dbo.BuinessRecod b
LEFT JOIN dbo.CarInfo c
    ON c.LicensePlate = b.LicensePlate
ORDER BY b.businessID;
"@
    },
    @{
        Name = "price_rule"
        Optional = $true
        Columns = @(
            "rule_id", "start_addr_id", "start_addr", "end_addr_id", "end_addr",
            "material_id", "material_name", "project_id", "project_name",
            "memo_keyword", "price", "is_active", "priority", "remark",
            "created_at", "updated_at"
        )
        UpdateColumns = @(
            "start_addr_id", "start_addr", "end_addr_id", "end_addr",
            "material_id", "material_name", "project_id", "project_name",
            "memo_keyword", "price", "is_active", "priority", "remark",
            "created_at", "updated_at"
        )
        Query = @"
SELECT
    RuleID AS rule_id,
    CASE WHEN ISNUMERIC(NULLIF(addr_startID, '')) = 1 THEN CAST(NULLIF(addr_startID, '') AS INT) ELSE NULL END AS start_addr_id,
    addr_start AS start_addr,
    CASE WHEN ISNUMERIC(NULLIF(addr_endID, '')) = 1 THEN CAST(NULLIF(addr_endID, '') AS INT) ELSE NULL END AS end_addr_id,
    addr_end AS end_addr,
    CASE WHEN ISNUMERIC(NULLIF(wLid, '')) = 1 THEN CAST(NULLIF(wLid, '') AS INT) ELSE NULL END AS material_id,
    wLsName AS material_name,
    CASE WHEN ISNUMERIC(NULLIF(xMid, '')) = 1 THEN CAST(NULLIF(xMid, '') AS INT) ELSE NULL END AS project_id,
    XmName AS project_name,
    sMemoKeyword AS memo_keyword,
    ISNULL(price, 0) AS price,
    ISNULL(IsActive, 1) AS is_active,
    ISNULL(Priority, 100) AS priority,
    Remark AS remark,
    ISNULL(CreatedAt, GETDATE()) AS created_at,
    ISNULL(UpdatedAt, GETDATE()) AS updated_at
FROM dbo.PriceRuleInfo
ORDER BY RuleID;
"@
    }
)

Write-Step "Loading SQL Server PowerShell module..."
Import-Module SQLPS -DisableNameChecking -ErrorAction Stop | Out-Null

Ensure-Directory -FilePath $OutputSqlFile
$outputFullPath = [System.IO.Path]::GetFullPath($OutputSqlFile)

Write-Step "Generating MySQL upsert script: $outputFullPath"
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
$writer = New-Object System.IO.StreamWriter($outputFullPath, $false, $utf8Bom)

try {
    $writer.WriteLine("-- Auto-generated SQL for SQL Server -> MySQL sync")
    $writer.WriteLine("-- Generated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $writer.WriteLine("SET NAMES utf8mb4;")
    $writer.WriteLine("SET FOREIGN_KEY_CHECKS=0;")
    $writer.WriteLine("START TRANSACTION;")
    $writer.WriteLine("")

    foreach ($plan in $tablePlans) {
        $name = $plan.Name
        Write-Step "Querying source table for $name..."
        $rows = @()
        try {
            $rows = @(Invoke-SourceQuery -Query $plan.Query -Server $SqlServer -Database $SqlDatabase -User $SqlUser -Password $SqlPassword)
        }
        catch {
            if ($plan.Optional) {
                Write-Warning "Optional table [$name] was skipped: $($_.Exception.Message)"
                continue
            }
            throw
        }

        $rowCount = @($rows).Count
        Write-Step "Rows fetched from source for ${name}: $rowCount"
        if ($rowCount -eq 0) {
            $writer.WriteLine("-- ${name}: 0 rows")
            $writer.WriteLine("")
            continue
        }

        $writer.WriteLine("-- ${name}: $rowCount rows")
        Write-UpsertBatches -Writer $writer `
            -Table $name `
            -Columns $plan.Columns `
            -UpdateColumns $plan.UpdateColumns `
            -Rows @($rows) `
            -ChunkSize $BatchSize
    }

    $writer.WriteLine("COMMIT;")
    $writer.WriteLine("SET FOREIGN_KEY_CHECKS=1;")
}
finally {
    $writer.Flush()
    $writer.Dispose()
}

Write-Step "MySQL script generated successfully."

if ($GenerateOnly) {
    Write-Step "GenerateOnly enabled. Skip MySQL execution."
    exit 0
}

Write-Step "Executing generated SQL on MySQL..."
$mysqlCommand = Get-Command $MySqlCliPath -ErrorAction SilentlyContinue
if (-not $mysqlCommand) {
    throw "mysql client not found. Please install MySQL client or specify -MySqlCliPath."
}

$mysqlArgs = @(
    "--default-character-set=utf8mb4",
    "--host=$MySqlHost",
    "--port=$MySqlPort",
    "--user=$MySqlUser",
    "--password=$MySqlPassword",
    $MySqlDatabase
)

$scriptText = Get-Content -Path $outputFullPath -Raw -Encoding UTF8
$scriptText | & $mysqlCommand.Source @mysqlArgs

if ($LASTEXITCODE -ne 0) {
    throw "MySQL execution failed with exit code $LASTEXITCODE."
}

Write-Step "Sync completed successfully."
