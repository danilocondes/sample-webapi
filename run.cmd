@echo off
setlocal
cd /d "%~dp0"
dotnet run --project src\PostHogSample.Api
endlocal
