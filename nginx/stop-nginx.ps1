# Stop the Placement Tracker NGINX reverse proxy.
# Run from any directory:  powershell -ExecutionPolicy Bypass -File nginx\stop-nginx.ps1
$nginxDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -LiteralPath $nginxDir
& ".\nginx.exe" -s stop
Write-Host "NGINX stopped."
