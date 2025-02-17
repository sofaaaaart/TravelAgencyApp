:: 1. Проверяем, установлен ли SQL Server Express 
$instanceName = "SQLEXPRESS"
$instancePath = "C:\Program Files\Microsoft SQL Server\"

# Проверка наличия SQL Server
if (!(Test-Path "$instancePath\MSSQL$instanceName")) {
    Write-Host "🔹 SQL Server Express не найден, начинаем установку..."
    
    :: 2. Скачиваем и устанавливаем SQL Server Express
    $sqlInstaller = "https://go.microsoft.com/fwlink/?linkid=866658"
    $downloadPath = "$env:TEMP\SQLServerExpress.exe"
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

:: 3. Загружаем базу данных из database.sql 
Write-Host "🔹 Загружаем базу данных..."
$sqlcmdPath = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe"
if (!(Test-Path $sqlcmdPath)) {
    Write-Host "❌ SQLCMD не найден, убедитесь, что установлен SQL Server."
    exit 1
}

$databaseScript = "database.sql"
try {
    & $sqlcmdPath -S "(localdb)\MSSQLLocalDB" -i $databaseScript
    Write-Host "✅ База данных успешно загружена!"
} catch {
    Write-Host "❌ Ошибка при загрузке базы данных: $_"
    exit 1
}