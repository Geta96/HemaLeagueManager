@echo off
setlocal

taskkill /IM HemaLeagueManager.exe /F >nul 2>&1
if exist publish rmdir /S /Q publish

dotnet publish -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o publish

if errorlevel 1 (
    echo.
    echo *** Build FAILED ***
    pause
    exit /b 1
)

echo.
echo Built: %cd%\publish\HemaLeagueManager.exe
pause