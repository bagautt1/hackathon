@echo off
chcp 65001
echo ====================================
echo    TEST WITHOUT ISOLATION
echo ====================================
echo.

echo Starting Critical Application without isolation...
start "" "C:\HACKATHON\TestApp\x64\Debug\TestApp.exe"

timeout /t 2

echo Starting noise generation...
start "" "C:\HACKATHON\NoiseGenerator\bin\Debug\NoiseGenerator.exe"

echo Test running! Compare results with isolated version.
pause