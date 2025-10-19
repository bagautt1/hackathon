
@echo off
echo Creating Excel charts from collected data...

powershell -Command "
# Читаем данные
`$data = Import-Csv 'C:\HACKATHON\Results\cpu_metrics.csv'

# Создаем Excel COM объект
`$excel = New-Object -ComObject Excel.Application
`$excel.Visible = `$true
`$workbook = `$excel.Workbooks.Add()
`$worksheet = `$workbook.Worksheets.Item(1)

`$worksheet.Name = 'CPU Isolation Results'

# Заголовки
`$worksheet.Cells(1,1) = 'Timestamp'
`$worksheet.Cells(1,2) = 'CPU Core'
`$worksheet.Cells(1,3) = 'Usage %'
`$worksheet.Cells(1,4) = 'Phase'

# Данные
`$row = 2
foreach (`$item in `$data) {
    `$worksheet.Cells(`$row, 1) = `$item.Timestamp
    `$worksheet.Cells(`$row, 2) = `$item.InstanceName
    `$worksheet.Cells(`$row, 3) = `$item.UsagePercent
    `$worksheet.Cells(`$row, 4) = `$item.TestPhase
    `$row++
}

# Создаем график
`$chart = `$worksheet.Shapes.AddChart().Chart
`$chart.ChartType = 74  # Line chart
`$chart.SetSourceData(`$worksheet.Range('A1:D' + (`$row-1)))
`$chart.HasTitle = `$true
`$chart.ChartTitle.Text = 'CPU Isolation Performance Comparison'

echo Excel chart created successfully!
echo.
"
pause