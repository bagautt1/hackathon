@echo off
chcp 65001
echo ====================================
echo    CPU ISOLATION DEMONSTRATION
echo ====================================
echo.

echo Step 1: Starting Critical Application...
start "" "C:\HACKATHON\TestApp\x64\Debug\TestApp.exe"

timeout /t 3

echo.
echo Step 2: Isolating to CPU 3...
"C:\HACKATHON\CoreIsolator\bin\Debug\CoreIsolator.exe" TestApp 3

echo.
echo Step 3: Starting noise generation...
start "" "C:\HACKATHON\NoiseGenerator\bin\Debug\NoiseGenerator.exe"

echo.
echo Demonstration is running!
echo Check the TestApp window for performance metrics.
echo.
pause
