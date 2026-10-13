@echo off
setlocal
cd /d "%~dp0"

net session >nul 2>&1
if errorlevel 1 (
  echo Please right-click install.bat and choose "Run as administrator".
  pause
  exit /b 1
)

set FW=%windir%\Microsoft.NET\Framework64\v4.0.30319
if not exist "%FW%\csc.exe" (
  echo Could not find the built-in .NET Framework 4 compiler at %FW%
  pause
  exit /b 1
)

set DEST=%ProgramW6432%\PkgThumb
if "%ProgramW6432%"=="" set DEST=%ProgramFiles%\PkgThumb

echo ============================================================
echo  PkgThumb installer
echo ============================================================
echo.
echo  HEADS UP: near the end, Windows Explorer restarts. Your
echo  taskbar, desktop icons and open folder windows will vanish
echo  for a few seconds. This is normal and they come back by
echo  themselves.
echo.
echo  If they do NOT come back:
echo    1. Press Ctrl + Shift + Esc to open Task Manager
echo    2. Click "Run new task" (or File, then Run new task)
echo    3. Type  explorer.exe  and press Enter
echo.
echo  Please save any work in open folder windows first.
echo.
pause

echo.
echo Compiling...
"%FW%\csc.exe" /nologo /target:library /out:PkgThumb.build.dll /r:System.Drawing.dll PkgThumb.cs
if errorlevel 1 (
  echo Compile failed - copy the error above and send it to the author.
  pause
  exit /b 1
)

REM Remove any previous install first so the old DLL is not locked
if exist "%DEST%\PkgThumb.dll" "%FW%\regasm.exe" /nologo /unregister "%DEST%\PkgThumb.dll" >nul 2>&1
taskkill /f /im explorer.exe >nul 2>&1

echo Installing to "%DEST%" ...
if not exist "%DEST%" mkdir "%DEST%"
copy /y PkgThumb.build.dll "%DEST%\PkgThumb.dll" >nul
if errorlevel 1 (
  echo Could not copy the DLL - it may be in use. Restart your PC and run install.bat again.
  del /f /q PkgThumb.build.dll >nul 2>&1
  call :restart_explorer
  pause
  exit /b 1
)
del /f /q PkgThumb.build.dll >nul 2>&1

echo Registering...
"%FW%\regasm.exe" /nologo /codebase "%DEST%\PkgThumb.dll"
if errorlevel 1 (
  echo Registration failed.
  call :restart_explorer
  pause
  exit /b 1
)

echo Clearing thumbnail cache and restarting Explorer...
del /f /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db" >nul 2>&1
call :restart_explorer

echo.
echo Done. Set your folder view to Medium icons or larger.
echo You can now delete this extracted folder - the tool lives in:
echo   %DEST%
pause
exit /b 0

:restart_explorer
start explorer.exe
timeout /t 4 /nobreak >nul
tasklist /fi "imagename eq explorer.exe" 2>nul | find /i "explorer.exe" >nul
if not errorlevel 1 exit /b 0
echo Explorer did not start yet, trying again...
start explorer.exe
timeout /t 4 /nobreak >nul
tasklist /fi "imagename eq explorer.exe" 2>nul | find /i "explorer.exe" >nul
if not errorlevel 1 exit /b 0
echo.
echo ============================================================
echo  Explorer has not come back on its own. To get it back:
echo    1. Press Ctrl + Shift + Esc to open Task Manager
echo    2. Click "Run new task" (or File, then Run new task)
echo    3. Type  explorer.exe  and press Enter
echo  If Task Manager will not open, press Ctrl + Alt + Delete,
echo  choose Task Manager, or restart your PC.
echo ============================================================
exit /b 1
