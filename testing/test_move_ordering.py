#!/usr/bin/env python3
"""
Quick test of enhanced move ordering function.
"""

import chess
import chess_ai

def test_move_ordering():
    """Test enhanced move ordering with different move types."""
    
    # Test position with various move types
    board = chess.Board("r1bqkb1r/pppp1ppp/2n2n2/4p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 0 4")
    
    moves = list(board.legal_moves)
    killer_table = {}
    
    # Order moves
    ordered_moves = chess_ai._order_moves(board, moves, killer_table)
    
    print("Move ordering test:")
    print("First 10 moves in order:")
    for i, move in enumerate(ordered_moves[:10]):
        # Check move properties
        is_capture = board.is_capture(move)
        board.push(move)
        is_check = board.is_check()
        board.pop()
        
        promotion = move.promotion is not None
        
        print(f"{i+1:2d}. {move.uci():5s} - capture: {is_capture}, check: {is_check}, promotion: {promotion}")
    
    # Test pawn attack detection
    board2 = chess.Board("8/8/8/3p4/2P5/8/8/8 w - - 0 1")
    
    # Test if d4 is attacked by black pawn on d5
    attacked = chess_ai._is_square_attacked_by_pawn(board2, chess.D4, chess.BLACK)
    print(f"\nPawn attack test: d4 attacked by black pawn? {attacked} (should be False)")
    
    # Test if c4 is attacked by black pawn on d5
    attacked = chess_ai._is_square_attacked_by_pawn(board2, chess.C4, chess.BLACK)
    print(f"c4 attacked by black pawn on d5? {attacked} (should be True)")
    
    # Test promotion ordering
    board3 = chess.Board("8/P7/8/8/8/8/8/8 w - - 0 1")
    moves3 = list(board3.legal_moves)
    ordered_moves3 = chess_ai._order_moves(board3, moves3, {})
    
    print(f"\nPromotion test - first move: {ordered_moves3[0].uci()}")
    print(f"Promotion piece: {ordered_moves3[0].promotion}")
    
    print("\nAll move ordering tests completed!")

if __name__ == "__main__":
    test_move_ordering()
