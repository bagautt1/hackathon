using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CoreIsolatorGUI
{
    public partial class MainWindow : Window
    {
        private List<ProcessInfo> _allProcesses = new List<ProcessInfo>();
        private DispatcherTimer _monitorTimer;
        private bool _isMonitoring = false;

        public class ProcessInfo
        {
            public string ProcessName { get; set; }
            public int Id { get; set; }
            public string PriorityClass { get; set; }
            public string ProcessorAffinity { get; set; }
            public Process ProcessObject { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            // Заполняем список CPU ядер
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                CpuCoreComboBox.Items.Add($"CPU {i}");
            }
            CpuCoreComboBox.SelectedIndex = 3; // По умолчанию CPU 3

            // Показываем информацию о системе
            CpuCountText.Text = $"CPU Cores: {Environment.ProcessorCount}";
            StatusText.Text = "Application initialized";

            // Загружаем процессы
            RefreshProcesses();

            // Настраиваем таймер для авто-обновления
            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromSeconds(3);
            _monitorTimer.Tick += (s, e) => RefreshProcesses();
        }

        private void RefreshProcesses()
        {
            try
            {
                _allProcesses.Clear();

                // Получаем все процессы
                var processes = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                    .OrderBy(p => p.ProcessName)
                    .ToList();

                foreach (var process in processes)
                {
                    try
                    {
                        string affinity = "All";
                        if (process.ProcessorAffinity != (IntPtr)((1L << Environment.ProcessorCount) - 1))
                        {
                            // Определяем к каким CPU привязан процесс
                            long mask = (long)process.ProcessorAffinity;
                            var cpus = new List<int>();
                            for (int i = 0; i < Environment.ProcessorCount; i++)
                            {
                                if ((mask & (1L << i)) != 0)
                                    cpus.Add(i);
                            }
                            affinity = string.Join(",", cpus);
                        }

                        _allProcesses.Add(new ProcessInfo
                        {
                            ProcessName = process.ProcessName,
                            Id = process.Id,
                            PriorityClass = process.PriorityClass.ToString(),
                            ProcessorAffinity = affinity,
                            ProcessObject = process
                        });
                    }
                    catch
                    {
                        // Игнорируем процессы к которым нет доступа
                    }
                }

                ApplySearchFilter();
                Log($"Refreshed processes list. Total: {_allProcesses.Count}");
            }
            catch (Exception ex)
            {
                Log($"Error refreshing processes: {ex.Message}");
            }
        }

        private void ApplySearchFilter()
        {
            var filtered = _allProcesses;

            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                filtered = _allProcesses
                    .Where(p => p.ProcessName.IndexOf(SearchBox.Text,StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            ProcessesListBox.ItemsSource = filtered;
            ProcessesListBox.Items.Refresh();
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                LogTextBox.ScrollToEnd();
            });
        }

        // ===== ОБРАБОТЧИКИ СОБЫТИЙ =====

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcesses();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void ProcessesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ProcessesListBox.SelectedItem as ProcessInfo;
            if (selected != null)
            {
                SelectedProcessText.Text = $"{selected.ProcessName} (PID: {selected.Id})";
                StatusText.Text = $"Selected: {selected.ProcessName}";
            }
        }

        private void KillProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = ProcessesListBox.SelectedItem as ProcessInfo;
            if (selected != null && selected.ProcessObject != null)
            {
                try
                {
                    selected.ProcessObject.Kill();
                    Log($"Killed process: {selected.ProcessName} (PID: {selected.Id})");
                    RefreshProcesses();
                }
                catch (Exception ex)
                {
                    Log($"Error killing process: {ex.Message}");
                }
            }
        }

        private async void IsolateBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = ProcessesListBox.SelectedItem as ProcessInfo;
            if (selected == null)
            {
                MessageBox.Show("Please select a process first!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CpuCoreComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a CPU core!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int targetCpu = CpuCoreComboBox.SelectedIndex;
            string priority = (PriorityComboBox.SelectedItem as ComboBoxItem)?.Tag as string;

            await Task.Run(() => ApplyIsolation(selected.ProcessObject, targetCpu, priority));
        }

        private void ApplyIsolation(Process process, int cpuCore, string priority)
        {
            try
            {
                // Устанавливаем affinity
                long affinityMask = 1L << cpuCore;
                process.ProcessorAffinity = (IntPtr)affinityMask;

                // Устанавливаем приоритет
                if (Enum.TryParse<ProcessPriorityClass>(priority, out var priorityClass))
                {
                    process.PriorityClass = priorityClass;
                }

                Dispatcher.Invoke(() =>
                {
                    Log($"✅ SUCCESS: Isolated {process.ProcessName} to CPU {cpuCore} with {priority} priority");
                    StatusText.Text = $"Isolated {process.ProcessName} to CPU {cpuCore}";

                    // Подсвечиваем кнопку
                    IsolateBtn.Background = System.Windows.Media.Brushes.LightGreen;

                    RefreshProcesses();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    Log($"❌ ERROR isolating process: {ex.Message}");
                    StatusText.Text = "Isolation failed - run as Administrator";
                });
            }
        }

        private void MonitorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_isMonitoring)
            {
                _monitorTimer.Start();
                MonitorBtn.Content = "STOP MONITORING";
                MonitorBtn.Background = System.Windows.Media.Brushes.LightCoral;
                MonitoringProgress.Visibility = Visibility.Visible;
                _isMonitoring = true;
                Log("Started automatic monitoring (3s interval)");
            }
            else
            {
                _monitorTimer.Stop();
                MonitorBtn.Content = "START MONITORING";
                MonitorBtn.Background = System.Windows.Media.Brushes.LightBlue;
                MonitoringProgress.Visibility = Visibility.Collapsed;
                _isMonitoring = false;
                Log("Stopped automatic monitoring");
            }
        }

        private void CpuCoreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CpuCoreComboBox.SelectedIndex >= 0)
            {
                StatusText.Text = $"Target CPU: {CpuCoreComboBox.SelectedIndex}";
            }
        }
    }
}