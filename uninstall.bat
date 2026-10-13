@echo off
setlocal
net session >nul 2>&1
if errorlevel 1 (
  echo Please right-click uninstall.bat and choose "Run as administrator".
  pause
  exit /b 1
)

set FW=%windir%\Microsoft.NET\Framework64\v4.0.30319
set DEST=%ProgramW6432%\PkgThumb
if "%ProgramW6432%"=="" set DEST=%ProgramFiles%\PkgThumb

echo ============================================================
echo  PkgThumb uninstaller
echo ============================================================
echo.
echo  HEADS UP: Windows Explorer restarts during this. Your
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

if exist "%DEST%\PkgThumb.dll" "%FW%\regasm.exe" /nologo /unregister "%DEST%\PkgThumb.dll"
taskkill /f /im explorer.exe >nul 2>&1
del /f /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db" >nul 2>&1
if exist "%DEST%" rmdir /s /q "%DEST%"
if exist "%DEST%" echo Note: some files could not be deleted yet. Restart your PC, then delete "%DEST%".
call :restart_explorer
echo Removed.
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
