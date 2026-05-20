@echo off
dotnet publish -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o publish
echo.
echo Built: %cd%\publish\HemaLeagueManager.exe
pause