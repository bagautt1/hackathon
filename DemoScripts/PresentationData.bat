batch
@echo off
echo Generating presentation data...

:: Создаем сводный отчет
powershell -Command "
# Анализ данных и создание отчета
`$data = Import-Csv 'C:\HACKATHON\Results\cpu_metrics.csv' -ErrorAction SilentlyContinue
if (`$data) {
    `$baseline = `$data | Where-Object { `$_.TestPhase -eq 'Baseline' -and `$_.Instance -eq '3' }
    `$isolated = `$data | Where-Object { `$_.TestPhase -eq 'Isolated' -and `$_.Instance -eq '3' }
    
    `$baseAvg = (`$baseline | Measure-Object -Property UsagePercent -Average).Average
    `$isolAvg = (`$isolated | Measure-Object -Property UsagePercent -Average).Average
    `$improvement = (`$isolAvg - `$baseAvg) / `$baseAvg * 100
    
    `$report = @'
HACKATHON RESULTS REPORT
========================
Test: CPU Core Isolation
Date: $(Get-Date)

RESULTS:
- Baseline performance: {0}%
- Isolated performance: {1}%
- Improvement: {2}%

CONCLUSION:
CPU isolation provides {3} performance stability
for critical applications under load.
'@ -f [math]::Round(`$baseAvg,2), [math]::Round(`$isolAvg,2), 
       [math]::Round(`$improvement,2), 
       if (`$improvement -gt 0) { 'significant' } else { 'minimal' }
    
    `$report | Out-File 'C:\HACKATHON\Results\final_report.txt' -Encoding UTF8
    echo Report generated: C:\HACKATHON\Results\final_report.txt
} else {
    echo No data available for analysis
    echo Run the demo first to collect metrics
}
"

:: Открываем все результаты
start "" "C:\HACKATHON\Results\"
echo.
echo Presentation data ready!
echo Files available in: C:\HACKATHON\Results\
pause