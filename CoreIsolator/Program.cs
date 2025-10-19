using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CPU Core Isolator ===");
        Console.WriteLine("Available CPU cores: 0 to " + (Environment.ProcessorCount - 1));

        if (args.Length != 2)
        {
            Console.WriteLine("Usage: CoreIsolator.exe <process_name> <cpu_core>");
            Console.WriteLine("Example: CoreIsolator.exe TestApp 3");
            return;
        }

        string processName = args[0];
        int cpuCore;

        if (!int.TryParse(args[1], out cpuCore)  || cpuCore < 0 || cpuCore >= Environment.ProcessorCount)
        {
            Console.WriteLine($"Error: CPU core must be between 0 and {Environment.ProcessorCount - 1}");
            return;
        }

        IsolateProcess(processName, cpuCore);
    }

    static void IsolateProcess(string processName, int cpuCore)
    {
        try
        {
            Console.WriteLine($"Looking for process: {processName}");

            // Ждем пока процесс появится
            Process targetProcess = null;
            for (int i = 0; i < 10; i++)
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    targetProcess = processes[0];
                    break;
                }
                Thread.Sleep(1000);
                Console.WriteLine("Waiting for process to start...");
            }

            if (targetProcess == null)
            {
                Console.WriteLine($"Process {processName} not found!");
                return;
            }

            Console.WriteLine($"Found process: {targetProcess.ProcessName} (PID: {targetProcess.Id})");

            // Устанавливаем affinity (маска для одного ядра)
            long affinityMask = 1L << cpuCore;
            targetProcess.ProcessorAffinity = (IntPtr)affinityMask;

            // Повышаем приоритет
            try
            {
                targetProcess.PriorityClass = ProcessPriorityClass.High;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Note: Could not set High priority: {ex.Message}");
            }

            Console.WriteLine($"✅ SUCCESS: Process isolated to CPU {cpuCore}");
            Console.WriteLine($"✅ Affinity mask: {affinityMask} (binary: {Convert.ToString(affinityMask, 2)})");
            Console.WriteLine($"✅ Priority: {targetProcess.PriorityClass}");

            // Мониторим новые потоки
            MonitorProcessThreads(targetProcess.Id, cpuCore);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            Console.WriteLine("Run as Administrator for better results!");
        }
    }

    static void MonitorProcessThreads(int processId, int cpuCore)
    {
        Console.WriteLine("Starting thread monitor... Press 'q' to stop monitoring.");

        long affinityMask = 1L << cpuCore;
        var timer = new System.Timers.Timer(2000); // Проверять каждые 2 секунды

        timer.Elapsed += (s, e) =>
        {
            try
            {
                var process = Process.GetProcessById(processId);
                var threadCount = process.Threads.Count;
                Console.WriteLine($"Active threads: {threadCount}, All pinned to CPU {cpuCore}");
            }
            catch
            {
                Console.WriteLine("Process ended.");
                timer.Stop();
            }
        };

        timer.Start();

        // Ждем нажатия 'q'
        while (Console.ReadKey().Key != ConsoleKey.Q)
        {
            Thread.Sleep(100);
        }

        timer.Stop();
        Console.WriteLine("Monitoring stopped.");
    }
}