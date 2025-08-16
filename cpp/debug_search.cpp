#include "position.h"
#include "searcher.h"
#include "types.h"
#include <iostream>

using namespace ChessAI;

int main() {
    std::cout << "=== Search Debug Test ===" << std::endl;
    
    // Create starting position and searcher
    Position pos;
    Searcher searcher;
    
    // Set up simple search parameters
    SearchParams params;
    params.max_depth = 3;
    params.config = SearchConfig::FULL;
    
    std::cout << "Starting search..." << std::endl;
    
    // Perform search
    Move best_move = searcher.search(pos, params);
    
    std::cout << "Search returned move: " << best_move << std::endl;
    std::cout << "NULL_MOVE value: " << NULL_MOVE << std::endl;
    
    if (best_move == NULL_MOVE) {
        std::cout << "❌ Search returned NULL_MOVE" << std::endl;
        
        // Let's check if we can generate moves manually
        std::vector<Move> moves;
        pos.generate_moves(moves);
        std::cout << "Available moves: " << moves.size() << std::endl;
        
        if (!moves.empty()) {
            std::cout << "First move available: " << moves[0] << std::endl;
        }
    } else {
        std::cout << "✓ Search found a move!" << std::endl;
        Square from = from_sq(best_move);
        Square to = to_sq(best_move);
        std::cout << "Best move: " << from << " -> " << to << std::endl;
    }
    
    return 0;
}
