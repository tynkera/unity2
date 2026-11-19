@echo off
chcp 65001 >nul
setlocal

rem 기획 원본 엑셀을 게임용 CSV 로 내보냅니다.
rem 탐색기에서 이 파일을 더블클릭하면 가상환경 생성, openpyxl 설치, 변환, 결과 표시까지 진행합니다.
rem 한글 안내는 if 괄호 안에 넣지 않고, 아래 label 로 이동한 뒤 출력합니다.

cd /d "%~dp0.."
set "VENV_DIR=tools\.venv"
set "VENV_PY=tools\.venv\Scripts\python.exe"
set "PYTHONIOENCODING=utf-8"
set "PY_CMD="
set "EXIT_CODE=0"

rem 1. 파이썬 찾기: py -3 를 먼저 시도하고, 없으면 python 을 시도합니다.
rem    python 은 설치하지 않아도 Microsoft Store 별칭이 있을 수 있어서 --version 종료 코드로 확인합니다.
py -3 --version >nul 2>&1
if not errorlevel 1 set "PY_CMD=py -3"
if defined PY_CMD goto check_version
python --version >nul 2>&1
if not errorlevel 1 set "PY_CMD=python"
if defined PY_CMD goto check_version
goto no_python

:check_version
%PY_CMD% -c "import sys; sys.exit(0 if sys.version_info >= (3, 9) else 1)" >nul 2>&1
if errorlevel 1 goto old_python

rem 2. 가상환경이 없으면 만듭니다.
if exist "%VENV_PY%" goto check_openpyxl
echo 가상환경을 만듭니다: %VENV_DIR%
%PY_CMD% -m venv "%VENV_DIR%"
if errorlevel 1 goto venv_failed
if not exist "%VENV_PY%" goto venv_failed

:check_openpyxl
rem 3. openpyxl 이 없을 때만 설치합니다.
"%VENV_PY%" -c "import openpyxl" >nul 2>&1
if not errorlevel 1 goto run_export
echo openpyxl 을 설치합니다.
"%VENV_PY%" -m pip install --disable-pip-version-check -r "tools\requirements.txt"
if errorlevel 1 goto pip_failed

:run_export
rem 4. 변환 도구를 실행합니다. 이 파일에 넘긴 인자는 그대로 전달합니다.
"%VENV_PY%" "tools\export_tables.py" %*
set "EXIT_CODE=%ERRORLEVEL%"
if "%EXIT_CODE%"=="0" goto success
goto export_failed

:success
echo.
echo 변환 완료. Unity로 돌아가 Play 해서 확인하세요.
goto finish

:export_failed
echo.
echo 변환 실패. 위 메시지의 엑셀 위치를 고친 뒤 다시 실행하세요.
goto finish

:no_python
echo 파이썬을 찾지 못했습니다. 사전 과제 안내의 파이썬 설치를 확인하세요.
set "EXIT_CODE=1"
goto finish

:old_python
echo 파이썬 3.9 이상이 필요합니다. 지금 버전:
%PY_CMD% --version
set "EXIT_CODE=1"
goto finish

:venv_failed
echo 가상환경을 만들지 못했습니다.
set "EXIT_CODE=1"
goto finish

:pip_failed
echo openpyxl 설치에 실패했습니다. 인터넷 연결을 확인하세요.
set "EXIT_CODE=1"
goto finish

:finish
echo.
pause
endlocal & exit /b %EXIT_CODE%
