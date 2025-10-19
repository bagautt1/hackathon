@echo off
chcp 65001
title HACKATHON - CPU Isolation Full Demo
color 0F

echo ==================================================
echo        HACKATHON - CPU ISOLATION DEMO
echo ==================================================
echo.

echo This demo will show:
echo 1. Performance monitoring setup
echo 2. GUI isolation tool
echo 3. Real-time metrics collection
echo 4. Comparison results
echo.

echo Step 1: Starting Performance Monitor...
start "" "%~dp0StartMonitoring.bat"

echo Step 2: Starting GUI Isolation Tool...
timeout /t 3 >nul
start "" "C:\HACKATHON\CoreIsolatorGUI\bin\Debug\CoreIsolatorGUI.exe"

echo Step 3: Starting test applications...
timeout /t 2 >nul
start "" "C:\HACKATHON\TestApp\x64\Debug\TestApp.exe"

echo.
echo ==================================================
echo            DEMO INSTRUCTIONS
echo ==================================================
echo.
echo NOW DO THE FOLLOWING STEPS:
echo.
echo 1. IN PERFORMANCE MONITOR WINDOW:
echo    - Wait for it to load completely
echo    - Expand "Data Collector Sets"
echo    - Create or select monitoring set
echo.
echo 2. IN GUI ISOLATION TOOL:
echo    - Find "TestApp" in process list
echo    - Select CPU 3 as target
echo    - Click "ISOLATE PROCESS"
echo.
echo 3. IN POWERSHELL METRICS WINDOW:
echo    - Follow the on-screen instructions
echo    - It will collect baseline and isolated metrics
echo.
echo 4. OBSERVE RESULTS in TestApp window
echo    - Compare performance before/after isolation
echo.
echo ==================================================
echo.
echo Press any key to open Performance Monitor guide...
pause >nul

:: Открываем инструкцию по Performance Monitor
start "" "https://learn.microsoft.com/en-us/windows-server/administration/performance-monitor/performance-monitor"

echo.
echo Demo is running! Complete the steps above.
echo Press any key to show results folder...
pause >nul

:: Показываем папку с результатами
if exist "C:\HACKATHON\Results\" (
    explorer "C:\HACKATHON\Results\"
)

echo.
echo Demonstration complete!
pause