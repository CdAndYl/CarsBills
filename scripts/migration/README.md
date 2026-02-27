# SQL Server -> MySQL Sync

This folder contains a repeatable sync script for migrating legacy SQL Server data into the current MySQL schema used by `NewCarBills`.

## 中文说明

本目录提供一个可重复执行的数据同步脚本，用于将旧版 SQL Server 数据迁移到 `NewCarBills` 当前使用的 MySQL 表结构。

### 脚本文件

- `sync_sqlserver_to_mysql.ps1`

### 脚本功能

1. 从 SQL Server 读取以下源表数据：
   - `CarInfo`
   - `AddressInFo`
   - `WLinFo`
   - `XmInFo`
   - `BuinessRecod`
   - `PriceRuleInfo`（可选）
2. 映射到 MySQL 目标表：
   - `car_info`
   - `address_info`
   - `material_info`
   - `project_info`
   - `business_record`
   - `price_rule`
3. 生成分批 `INSERT ... ON DUPLICATE KEY UPDATE` SQL（支持增量同步）
4. 在一个事务中执行到 MySQL（若加 `-GenerateOnly` 则只生成不执行）

### 前置条件

- PowerShell（Windows PowerShell 5.1 及以上）
- 可用 `Invoke-Sqlcmd`（`SQLPS` 模块）
- MySQL 命令行工具 `mysql.exe`（在 `PATH` 中，或通过 `-MySqlCliPath` 指定）
- 可访问 SQL Server 与 MySQL 的网络

### 使用方式

#### 1）仅生成 SQL（建议先做）

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/sync_sqlserver_to_mysql.ps1" `
  -SqlServer "127.0.0.1,1433" `
  -SqlDatabase "shis2067" `
  -SqlUser "sa" `
  -SqlPassword "你的SQL密码" `
  -MySqlHost "127.0.0.1" `
  -MySqlPort 3306 `
  -MySqlDatabase "carsbill" `
  -MySqlUser "root" `
  -MySqlPassword "root" `
  -GenerateOnly
```

生成文件：
- `scripts/migration/output/sync_to_mysql.sql`

#### 2）生成并直接执行同步

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/sync_sqlserver_to_mysql.ps1" `
  -SqlServer "127.0.0.1,1433" `
  -SqlDatabase "shis2067" `
  -SqlUser "sa" `
  -SqlPassword "你的SQL密码" `
  -MySqlHost "127.0.0.1" `
  -MySqlPort 3306 `
  -MySqlDatabase "carsbill" `
  -MySqlUser "root" `
  -MySqlPassword "root"
```

#### 3）指定本机 mysql.exe 路径

```powershell
-MySqlCliPath "D:/soft/Mysql/Save/mysql-8.0.42-winx64/mysql-8.0.42-winx64/bin/mysql.exe"
```

### 注意事项

- `PriceRuleInfo` 作为可选表处理：不存在时会跳过，不影响其他表同步。
- `BuinessRecod` 中文本型 ID 字段通过 `TRY_CONVERT(INT, ...)` 转为整数。
- `business_record.car_id` 的回退逻辑：
  - 先用 `TRY_CONVERT(INT, BuinessRecod.CarID)`
  - 失败则按 `LicensePlate` 匹配 `CarInfo.CarID`
  - 再失败则写入 `0`
- 输出 SQL 采用 `UTF-8 with BOM`，避免中文乱码。

## Script

- `sync_sqlserver_to_mysql.ps1`

## What it does

1. Reads source data from SQL Server tables:
   - `CarInfo`
   - `AddressInFo`
   - `WLinFo`
   - `XmInFo`
   - `BuinessRecod`
   - `PriceRuleInfo` (optional)
2. Maps source columns to current MySQL tables:
   - `car_info`
   - `address_info`
   - `material_info`
   - `project_info`
   - `business_record`
   - `price_rule`
3. Generates batched MySQL `INSERT ... ON DUPLICATE KEY UPDATE` SQL.
4. Executes it on MySQL in one transaction (unless `-GenerateOnly` is used).

## Prerequisites

- PowerShell (Windows PowerShell 5.1 or newer)
- `Invoke-Sqlcmd` command available (`SQLPS` module)
- MySQL CLI client (`mysql.exe`) available in `PATH`, or pass `-MySqlCliPath`
- Network access to both SQL Server and MySQL

## Usage

### 1) Generate SQL only (safe dry run)

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/sync_sqlserver_to_mysql.ps1" `
  -SqlServer "127.0.0.1,1433" `
  -SqlDatabase "shis2067" `
  -SqlUser "sa" `
  -SqlPassword "your_sqlserver_password" `
  -MySqlHost "127.0.0.1" `
  -MySqlPort 3306 `
  -MySqlDatabase "carsbill" `
  -MySqlUser "root" `
  -MySqlPassword "root" `
  -GenerateOnly
```

Generated file:
- `scripts/migration/output/sync_to_mysql.sql`

### 2) Generate + execute sync

```powershell
powershell -ExecutionPolicy Bypass -File "scripts/migration/sync_sqlserver_to_mysql.ps1" `
  -SqlServer "127.0.0.1,1433" `
  -SqlDatabase "shis2067" `
  -SqlUser "sa" `
  -SqlPassword "your_sqlserver_password" `
  -MySqlHost "127.0.0.1" `
  -MySqlPort 3306 `
  -MySqlDatabase "carsbill" `
  -MySqlUser "root" `
  -MySqlPassword "root"
```

### 3) Use specific mysql.exe path

```powershell
-MySqlCliPath "D:/soft/Mysql/Save/mysql-8.0.42-winx64/mysql-8.0.42-winx64/bin/mysql.exe"
```

## Notes

- `PriceRuleInfo` is treated as optional; sync continues if this table does not exist.
- `BuinessRecod` ID-like text fields are converted with `TRY_CONVERT(INT, ...)`.
- For `business_record.car_id`, fallback logic is:
  - `TRY_CONVERT(INT, BuinessRecod.CarID)`
  - else matched `CarInfo.CarID` by `LicensePlate`
  - else `0`
- The script writes output SQL in UTF-8 with BOM to preserve Chinese text.
