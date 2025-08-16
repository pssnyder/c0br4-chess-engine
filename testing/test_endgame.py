#!/usr/bin/env python3
"""
Quick test of endgame evaluation function.
"""

import chess
import chess_ai

def test_endgame_evaluation():
    """Test endgame evaluation with different king positions."""
    
    # Test 1: Black king in corner (good for white)
    board = chess.Board("8/8/8/8/8/8/8/k6K w - - 0 1")
    score = chess_ai.evaluate_endgame(board)
    print(f"Black king in corner a1, white king h1: {score} (should be positive)")
    assert score > 0
    
    # Test 2: Black king in center (bad for white)  
    board = chess.Board("8/8/8/3k4/8/8/8/7K w - - 0 1")
    score = chess_ai.evaluate_endgame(board)
    print(f"Black king in center d5, white king h1: {score} (should be negative)")
    assert score < 0
    
    # Test 3: Kings close together (generally good)
    board = chess.Board("8/8/8/3kK3/8/8/8/8 w - - 0 1")
    score1 = chess_ai.evaluate_endgame(board)
    
    # Test 4: Kings far apart
    board = chess.Board("K7/8/8/8/8/8/8/7k w - - 0 1")
    score2 = chess_ai.evaluate_endgame(board)
    
    print(f"Kings close together: {score1}")
    print(f"Kings far apart: {score2}")
    print(f"Close kings should be better for king activity")
    
    # Test full evaluation in endgame
    board = chess.Board("8/8/8/8/8/3k4/3K4/3Q4 w - - 0 1")  # K+Q vs K
    full_score = chess_ai.evaluate(board)
    print(f"K+Q vs K endgame evaluation: {full_score} (should be strongly positive)")
    assert full_score > 800  # Should be very positive due to material advantage
    
    print("\nAll endgame evaluation tests passed!")

if __name__ == "__main__":
    test_endgame_evaluation()
