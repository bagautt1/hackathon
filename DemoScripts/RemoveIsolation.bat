@echo off
echo Removing CPU isolation from TestApp...

powershell -Command "
\$process = Get-Process -Name 'TestApp' -ErrorAction SilentlyContinue
if (\$process) {
    # Устанавливаем affinity на ВСЕ ядра (все биты = 1)
    \$allCoresMask = (1 -shl [Environment]::ProcessorCount) - 1
    \$process.ProcessorAffinity = [IntPtr]\$allCoresMask
    \$process.PriorityClass = 'Normal'
    echo 'Isolation removed! TestApp now uses ALL CPUs'
} else {
    echo 'TestApp not found!'
}
"

pause
