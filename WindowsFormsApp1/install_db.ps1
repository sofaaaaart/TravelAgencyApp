# 1. Проверяем, установлен ли SQL Server Express
$instanceName = "SQLEXPRESS"

# Проверяем, установлен ли сервис MSSQL$SQLEXPRESS
if (-not (Get-Service -Name "MSSQL$instanceName" -ErrorAction SilentlyContinue)) {
    Write-Host "🔹 SQL Server Express не найден, начинаем установку..."

    # 2. Скачиваем и устанавливаем SQL Server Express
    $sqlInstaller = "https://go.microsoft.com/fwlink/?linkid=866658"
    $downloadPath = "$env:TEMP\SQLServerExpress.exe"

    if (-not (Get-Command Invoke-WebRequest -ErrorAction SilentlyContinue)) {
        Write-Host "❌ Ваша версия PowerShell не поддерживает загрузку файлов."
        exit 1
    }

    Invoke-WebRequest -Uri $sqlInstaller -OutFile $downloadPath
    try {
        Start-Process -FilePath $downloadPath -ArgumentList "/QS /IACCEPTSQLSERVERLICENSETERMS /ACTION=Install /FEATURES=SQL /INSTANCENAME=$instanceName /TCPENABLED=1 /NPENABLED=1 /SECURITYMODE=SQL /SAPWD=YourStrongPassword" -Wait
        Write-Host "✅ SQL Server Express установлен!"
    } catch {
        Write-Host "❌ Ошибка при установке SQL Server Express: $_"
        exit 1
    }
} else {
    Write-Host "✅ SQL Server уже установлен."
}

# 3. Загружаем базу данных
Write-Host "🔹 Загружаем базу данных..."

# Поиск пути к sqlcmd.exe
$sqlcmdPath = (Get-Command sqlcmd -ErrorAction SilentlyContinue).Source
if (-not $sqlcmdPath) {
    Write-Host "❌ Ошибка: SQLCMD не найден. Убедитесь, что установлен SQL Server Management Studio (SSMS)."
    exit 1
}

$databaseScript = "database.sql"

try {
    & $sqlcmdPath -S "localhost\SQLEXPRESS" -i $databaseScript
    Write-Host "✅ База данных успешно загружена!"
} catch {
    Write-Host "❌ Ошибка при загрузке базы данных: $_"
    exit 1
}
