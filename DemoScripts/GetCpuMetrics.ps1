# CPU Isolation Performance Metrics Script

Write-Host "=== CPU Isolation Performance Monitor ===" -ForegroundColor Green

# Функция для сбора метрик
function Get-CpuMetrics {
    param([int]$Duration = 10)
    
    $metrics = @()
    
    for ($i = 1; $i -le $Duration; $i++) {
        Write-Progress -Activity "Collecting CPU Metrics" -Status "Second $i of $Duration" -PercentComplete (($i/$Duration)*100)
        
        $cpuData = Get-Counter "\Processor(*)\% Processor Time" -SampleInterval 1 -MaxSamples 1
        $timestamp = Get-Date -Format "HH:mm:ss"
        
        foreach ($counter in $cpuData.CounterSamples) {
            $instance = $counter.InstanceName
            $value = [math]::Round($counter.CookedValue, 2)
            
            if ($instance -eq "_Total" -or $instance -like "*CPU*") {
                $metrics += [PSCustomObject]@{
                    Timestamp = $timestamp
                    Instance = $instance
                    Usage = $value
                    IsIsolated = ($instance -eq "3")  # Предполагаем что CPU 3 изолирован
                }
            }
        }
        
        Start-Sleep 1
    }
    
    return $metrics
}

# Собираем метрики до изоляции
Write-Host "`n1. Collecting BASELINE metrics (10 seconds)..." -ForegroundColor Yellow
$baseline = Get-CpuMetrics -Duration 10

Write-Host "`n2. Please start your CPU isolation now..." -ForegroundColor Yellow
Read-Host "Press Enter when isolation is active"

# Собираем метрики после изоляции
Write-Host "`n3. Collecting ISOLATED metrics (10 seconds)..." -ForegroundColor Yellow
$isolated = Get-CpuMetrics -Duration 10

# Анализ результатов
Write-Host "`n=== RESULTS ANALYSIS ===" -ForegroundColor Green

$baselineAvg = ($baseline | Where-Object { $_.Instance -eq "3" } | Measure-Object -Property Usage -Average).Average
$isolatedAvg = ($isolated | Where-Object { $_.Instance -eq "3" } | Measure-Object -Property Usage -Average).Average

Write-Host "CPU 3 Average Usage:" -ForegroundColor Cyan
Write-Host "  Baseline:  $([math]::Round($baselineAvg, 2))%" -ForegroundColor White
Write-Host "  Isolated:  $([math]::Round($isolatedAvg, 2))%" -ForegroundColor White

$stabilityImprovement = "N/A"
if ($baselineAvg -gt 0) {
    $variation = (($isolatedAvg - $baselineAvg) / $baselineAvg) * 100
    $stabilityImprovement = "$([math]::Round($variation, 2))%"
}

Write-Host "`nStability Improvement: $stabilityImprovement" -ForegroundColor Cyan

# Экспорт в CSV для графиков
$allMetrics = $baseline + $isolated
$allMetrics | Export-Csv -Path "C:\HACKATHON\cpu_metrics.csv" -NoTypeInformation

Write-Host "`nData exported to: C:\HACKATHON\cpu_metrics.csv" -ForegroundColor Green
Write-Host "You can import this file to Excel for chart creation." -ForegroundColor Yellow



ЭТИ ФАЙЛЫ СОЗДАЮТСЯ В ПАПКЕ DemoScripts