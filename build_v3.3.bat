@echo off
REM C0BR4 v3.3 Build and Deploy Script
REM Builds the engine with new adaptive time management

echo =======================================
echo C0BR4 Chess Engine v3.3 Build Script
echo Adaptive Time Management & Depth Search
echo =======================================
echo.

cd /d "%~dp0src"

echo [1/3] Building Release version...
dotnet build -c Release
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Publishing single-file executable...
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
if errorlevel 1 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

echo.
echo [3/3] Copying to deployment location...
if not exist "..\deployed\C0BR4_v3.3" mkdir "..\deployed\C0BR4_v3.3"
copy /Y "bin\Release\net6.0\win-x64\publish\C0BR4_v3.3.exe" "..\deployed\C0BR4_v3.3\"
copy /Y "..\docs\C0BR4_v3.3_Time_Management_Improvements.md" "..\deployed\C0BR4_v3.3\"

echo.
echo =======================================
echo Build completed successfully!
echo.
echo Executable location:
echo %~dp0deployed\C0BR4_v3.3\C0BR4_v3.3.exe
echo.
echo Key Features:
echo - Adaptive depth: 6-10 based on time control
echo - Conservative time management
echo - 30 min games = depth 10 target
echo - 3 min blitz = depth 6 target
echo =======================================
echo.

pause
