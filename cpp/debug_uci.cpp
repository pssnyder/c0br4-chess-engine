#include "src/uci.h"
#include <iostream>
#include <vector>
#include <string>
#include <thread>
#include <chrono>

using namespace ChessAI;

int main() {
    std::cout << "=== UCI Interface Debug ===" << std::endl;
    
    UCIEngine engine;
    
    // Simulate UCI commands manually
    std::cout << "Testing UCI initialization..." << std::endl;
    engine.handle_uci();
    
    std::cout << "\nTesting ready check..." << std::endl;
    engine.handle_isready();
    
    std::cout << "\nTesting position setup..." << std::endl;
    std::vector<std::string> pos_tokens = {"position", "startpos"};
    engine.handle_position(pos_tokens);
    
    std::cout << "\nTesting search..." << std::endl;
    std::vector<std::string> go_tokens = {"go", "depth", "3"};
    engine.handle_go(go_tokens);
    
    // Give time for search to complete
    std::this_thread::sleep_for(std::chrono::seconds(2));
    
    std::cout << "\nTest complete!" << std::endl;
    return 0;
}
