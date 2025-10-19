using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CPU Noise Generator ===");
        Console.WriteLine("This will create heavy load on all CPUs except CPU 3");
        Console.WriteLine("Press Enter to start noise generation...");
        Console.ReadLine();

        int noiseCores = Environment.ProcessorCount;
        Console.WriteLine($"Creating load on {noiseCores} cores...");

        // Создаем нагрузку на всех ядрах кроме 3 (если больше 4 ядер)
        var cancellationTokenSource = new CancellationTokenSource();

        for (int i = 0; i < noiseCores; i++)
        {
            if (i == 3 && noiseCores > 4) // Пропускаем ядро 3 если ядер достаточно
                continue;

            int core = i;
            Task.Run(() => GenerateLoad(core, cancellationTokenSource.Token));
        }

        Console.WriteLine("Noise generation running! Press Enter to stop...");
        Console.ReadLine();

        cancellationTokenSource.Cancel();
        Console.WriteLine("Stopping noise generation...");
        Thread.Sleep(2000);
    }

    static void GenerateLoad(int core, CancellationToken token)
    {
        // Привязываем задачу к конкретному ядру
        Thread.BeginThreadAffinity();

        Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1L << core);

        Console.WriteLine($"Started load on CPU {core}");

        long counter = 0;
        var random = new Random();

        while (!token.IsCancellationRequested)
        {
            // Интенсивные вычисления
            double result = 0;
            for (int i = 0; i < 1000000; i++)
            {
                result += Math.Sqrt(random.NextDouble() * 1000);
                counter++;
            }

            // Небольшая пауза чтобы не перегреть
            Thread.Sleep(10);
        }

        Thread.EndThreadAffinity();
        Console.WriteLine($"Stopped load on CPU {core}");
    }
}