# SQL Server -> MySQL Sync

This folder contains repeatable scripts for migrating legacy SQL Server data into the current MySQL schema used by `NewCarBills`.

## 中文说明

本目录提供可重复执行的数据同步脚本，用于将旧版 SQL Server 数据迁移到 `NewCarBills` 当前使用的 MySQL 表结构。

### 脚本文件

- `sync_sqlserver_to_mysql.ps1`: 核心同步脚本
- `run_sync.ps1`: 推荐入口，支持通过环境变量读取连接信息（避免明文密码）

### 脚本功能

1. 从 SQL Server 读取源表数据:
   - `CarInfo`
   - `AddressInFo`
   - `WLinFo`
   - `XmInFo`
   - `BuinessRecod`
   - `PriceRuleInfo` (可选)
2. 映射到 MySQL 目标表:
   - `car_info`
   - `address_info`
   - `material_info`
   - `project_info`
   - `business_record`
   - `price_rule`
3. 生成分批 `INSERT ... ON DUPLICATE KEY UPDATE` SQL（支持重复执行）
4. 执行到 MySQL（加 `-GenerateOnly` 时只生成 SQL 不落库）

### 前置条件

- PowerShell（Windows PowerShell 5.1+）
- 可用 `Invoke-Sqlcmd`（`SQLPS` 模块）
- MySQL 命令行工具 `mysql.exe`（在 `PATH` 中，或通过 `-MySqlCliPath` 指定）
- 可访问 SQL Server 与 MySQL 的网络

### 推荐用法（环境变量）

#### 1) 设置环境变量（当前会话）

```powershell
$env:CARSBILL_SQLSERVER_HOST = "192.168.31.146"
$env:CARSBILL_SQLSERVER_PORT = "1433"
$env:CARSBILL_SQLSERVER_DATABASE = "shis2067"
$env:CARSBILL_SQLSERVER_USER = "shis"
$env:CARSBILL_SQLSERVER_PASSWORD = "你的SQLServer密码"

$env:CARSBILL_MYSQL_HOST = "127.0.0.1"
$env:CARSBILL_MYSQL_PORT = "3306"
$env:CARSBILL_MYSQL_DATABASE = "carsbill"
$env:CARSBILL_MYSQL_USER = "root"
$env:CARSBILL_MYSQL_PASSWORD = "你的MySQL密码"
```

#### 2) 仅生成 SQL（建议先做）

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/run_sync.ps1" -GenerateOnly
```

生成文件:
- `scripts/migration/output/sync_to_mysql.sql`

#### 3) 生成并执行同步

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/run_sync.ps1"
```

#### 4) 指定 mysql.exe 路径

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/run_sync.ps1" `
  -MySqlCliPath "D:/soft/Mysql/Save/mysql-8.0.42-winx64/mysql-8.0.42-winx64/bin/mysql.exe"
```

### 兼容用法（直接调用核心脚本）

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/sync_sqlserver_to_mysql.ps1" `
  -SqlServer "192.168.31.146,1433" `
  -SqlDatabase "shis2067" `
  -SqlUser "shis" `
  -SqlPassword "<sql_password>" `
  -MySqlHost "127.0.0.1" `
  -MySqlPort 3306 `
  -MySqlDatabase "carsbill" `
  -MySqlUser "root" `
  -MySqlPassword "<mysql_password>"
```

### 注意事项

- `PriceRuleInfo` 作为可选表处理，不存在时会跳过。
- `BuinessRecod` 的文本型 ID 字段通过 `CASE WHEN ISNUMERIC(...) = 1 THEN CAST(...)` 转整数。
- 为兼容 SQL Server 2008/旧版本，脚本不依赖 `TRY_CONVERT`。
- `business_record.car_id` 回退逻辑：
  - `CASE WHEN ISNUMERIC(BuinessRecod.CarID)=1 THEN CAST(...)`
  - 按 `LicensePlate` 匹配 `CarInfo.CarID`
  - 最终回退为 `0`
- 输出 SQL 采用 `UTF-8 with BOM`，避免中文乱码。

## English

This folder provides migration scripts from SQL Server to MySQL for `NewCarBills`.

### Scripts

- `sync_sqlserver_to_mysql.ps1`: core sync script
- `run_sync.ps1`: recommended entrypoint using environment variables to avoid plain-text passwords

### Recommended Usage (Environment Variables)

1) Set environment variables (current session):

```powershell
$env:CARSBILL_SQLSERVER_HOST = "192.168.31.146"
$env:CARSBILL_SQLSERVER_PORT = "1433"
$env:CARSBILL_SQLSERVER_DATABASE = "shis2067"
$env:CARSBILL_SQLSERVER_USER = "shis"
$env:CARSBILL_SQLSERVER_PASSWORD = "<sql_password>"

$env:CARSBILL_MYSQL_HOST = "127.0.0.1"
$env:CARSBILL_MYSQL_PORT = "3306"
$env:CARSBILL_MYSQL_DATABASE = "carsbill"
$env:CARSBILL_MYSQL_USER = "root"
$env:CARSBILL_MYSQL_PASSWORD = "<mysql_password>"
```

2) Generate SQL only:

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/run_sync.ps1" -GenerateOnly
```

3) Generate and execute:

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/run_sync.ps1"
```

4) Specify mysql.exe path:

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/run_sync.ps1" `
  -MySqlCliPath "D:/path/to/mysql.exe"
```
