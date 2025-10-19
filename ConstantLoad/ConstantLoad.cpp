#include <windows.h>
#include <iostream>

int main() {
    std::cout << "Constant 100% CPU load on isolated core..." << std::endl;
    
    while (true) {
        // Постоянная нагрузка без пауз
        volatile double result = 0.0;
        for (int i = 0; i < 100000000; i++) {
            result += sqrt(i * 2.71828);
        }
        // НИКАКИХ Sleep()!
    }
    
    return 0;
}