@echo off
set UNITY=C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe

cd /d "%~dp0"
if not exist Logs mkdir Logs

"%UNITY%" -batchmode -nographics -quit ^
  -projectPath "%CD%" ^
  -buildTarget Win64 ^
  -executeMethod ExpeditionCamp.EditorTools.BuildScript.BuildForTest ^
  -logFile Logs\build_windows.log

if %ERRORLEVEL% neq 0 (
  echo BUILD FAILED. Last lines of Logs\build_windows.log:
  powershell -NoProfile -Command "Get-Content Logs\build_windows.log -Tail 30"
  exit /b 1
)

echo BUILD SUCCEEDED: Builds\Test\ExpeditionCamp.exe
exit /b 0