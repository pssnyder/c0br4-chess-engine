@echo off
REM C0BR4 Chess Engine v3.2 Launcher for Arena GUI
REM This batch file launches the C0BR4 v3.2 engine with proper initialization

REM Change to the directory where the engine executable is located
cd /d "%~dp0src\bin\Release\net6.0\win-x64"

REM Launch the C0BR4 v3.2 engine
C0BR4_v3.2.exe
