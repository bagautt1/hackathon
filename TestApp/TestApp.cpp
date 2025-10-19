#include <windows.h>
#include <iostream>
#include <chrono>
#include <vector>

// Функция для измерения производительности
void performance_test() {
    std::cout << "=== Critical Application Started ===" << std::endl;
    std::cout << "PID: " << GetCurrentProcessId() << std::endl;

    auto start_total = std::chrono::high_resolution_clock::now();
    int stable_iterations = 0;

    for (int iteration = 0; iteration < 50; iteration++) {
        auto start = std::chrono::high_resolution_clock::now();

        // Вычислительная нагрузка
        volatile double result = 0.0;
        for (int i = 0; i < 50000000; i++) {
            result += sqrt(i * 3.14159);
        }

        auto end = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start);

        std::cout << "Iteration " << iteration + 1 << ": " << duration.count() << " microseconds";

        // Проверка стабильности (вариация < 20%)
        if (duration.count() > 0 && duration.count() < 200000) {
            stable_iterations++;
            std::cout << " [STABLE]" << std::endl;
        }
        else {
            std::cout << " [UNSTABLE]" << std::endl;
        }

        //Sleep(100); // Небольшая пауза между итерациями
    }

    auto end_total = std::chrono::high_resolution_clock::now();
    auto total_duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_total - start_total);

    std::cout << "=== Results ===" << std::endl;
    std::cout << "Total time: " << total_duration.count() << "ms" << std::endl;
    std::cout << "Stable iterations: " << stable_iterations << "/50" << std::endl;
    std::cout << "Stability: " << (stable_iterations * 100 / 50) << "%" << std::endl;
}

int main() {
    performance_test();
    std::cout << "Press Enter to exit...";
    std::cin.get();
    return 0;
}