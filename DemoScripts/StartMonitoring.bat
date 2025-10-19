Создайте StartMonitoring.bat:


@echo off
echo Starting Performance Monitor for CPU Isolation Demo...

:: Запускаем Performance Monitor с нашим набором
start perfmon.exe /sys

echo.
echo INSTRUCTIONS:
echo 1. In Performance Monitor, go to "Data Collector Sets -> User Defined"
echo 2. Right-click "CPU Isolation Demo" -> Start
echo 3. Run your isolation test
echo 4. Right-click -> Stop when done
echo 5. View results in "Reports -> User Defined -> CPU Isolation Demo"
echo.
pause


