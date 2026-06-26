# Unblock files downloaded from Git (fixes "access denied" on .exe / build output).
# Run once from the repo root in PowerShell:
#   Set-ExecutionPolicy -Scope Process Bypass; .\scripts\unblock-windows.ps1

$root = Split-Path -Parent $PSScriptRoot
Write-Host "Unblocking files under $root ..."

Get-ChildItem -Path $root -Recurse -Force | Unblock-File

Write-Host "Done. Now run: dotnet run --project src/PostHogSample.Api"
