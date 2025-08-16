#!/usr/bin/env python3
"""
Quick test of game phase detection function.
"""

import chess
import chess_ai

def test_game_phases():
    """Test game phase detection with different positions."""
    
    # Starting position - should be opening
    board = chess.Board()
    phase = chess_ai.detect_game_phase(board)
    print(f"Starting position (32 pieces): {phase}")
    assert phase == 'opening'
    
    # Move 12 - should be middlegame 
    board = chess.Board()
    for _ in range(12):
        moves = list(board.legal_moves)
        if moves:
            board.push(moves[0])
    phase = chess_ai.detect_game_phase(board)
    print(f"After 12 moves ({len(board.piece_map())} pieces): {phase}")
    
    # Simple endgame position with few pieces
    board = chess.Board("8/8/8/8/8/3k4/3K4/3Q4 w - - 0 1")  # K+Q vs K
    phase = chess_ai.detect_game_phase(board)
    piece_count = len(board.piece_map()) - 2  # excluding kings
    print(f"K+Q vs K ({piece_count} pieces excluding kings): {phase}")
    assert phase == 'endgame'
    
    # Position with exactly 14 pieces (excluding kings) - should be endgame
    board = chess.Board("r3k2r/8/8/8/8/8/8/R3K2R w - - 0 1")  # 4 rooks + 2 kings
    phase = chess_ai.detect_game_phase(board)
    piece_count = len(board.piece_map()) - 2
    print(f"4 rooks + 2 kings ({piece_count} pieces excluding kings): {phase}")
    assert phase == 'endgame'
    
    # Position with 15 pieces (excluding kings) - should be middlegame
    board = chess.Board("rnbqkbnr/pppppppp/8/8/8/8/8/8 w - - 0 1")  # 16 pieces total, 14 excluding kings
    phase = chess_ai.detect_game_phase(board)
    piece_count = len(board.piece_map()) - 2
    print(f"Black starting position ({piece_count} pieces excluding kings): {phase}")
    assert phase == 'endgame'  # 14 pieces should be endgame
    
    # Position with 16 pieces (excluding kings) - should be middlegame
    board = chess.Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/8 w - - 0 20")  # 18 pieces total, 16 excluding kings, move 20
    phase = chess_ai.detect_game_phase(board) 
    piece_count = len(board.piece_map()) - 2
    print(f"16 pieces excluding kings, move 20: {phase}")
    assert phase == 'middlegame'
    
    print("\nAll game phase detection tests passed!")

if __name__ == "__main__":
    test_game_phases()
