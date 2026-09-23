# Start the Placement Tracker NGINX reverse proxy (single entry point :8080).
# Run from any directory:  powershell -ExecutionPolicy Bypass -File nginx\start-nginx.ps1
$nginxDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -LiteralPath $nginxDir
& ".\nginx.exe" -t
if ($LASTEXITCODE -ne 0) { exit 1 }
$running = Get-Process -Name "nginx" -ErrorAction SilentlyContinue
if ($running) {
    & ".\nginx.exe" -s reload
    Write-Host "NGINX reloaded: http://localhost:8080"
} else {
    Start-Process -FilePath "$nginxDir\nginx.exe" -WorkingDirectory $nginxDir
    Start-Sleep -Seconds 1
    Write-Host "NGINX started: http://localhost:8080"
}
