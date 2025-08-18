using System;
using ChessEngine.Core;

namespace ChessEngine.Testing
{
    public static class IllegalMoveDebugger
    {
        public static void AnalyzePosition(Board board)
        {
            Console.WriteLine("=== ILLEGAL MOVE ANALYSIS ===");
            
            // Show current position info
            Console.WriteLine($"White to move: {board.IsWhiteToMove}");
            
            // Check what piece is on c1
            var c1Piece = board.GetPiece(new Square(2)); // c1 = index 2
            Console.WriteLine($"Piece on c1: {c1Piece}");
            
            // Check what piece is on a8
            var a8Piece = board.GetPiece(new Square(56)); // a8 = index 56
            Console.WriteLine($"Piece on a8: {a8Piece}");
            
            // Check diagonal path from c1 to a8
            Console.WriteLine("Diagonal path from c1 to a8:");
            int[] diagonalSquares = { 2, 11, 20, 29, 38, 47, 56 }; // c1, d2, e3, f4, g5, h6, a8
            
            foreach (int square in diagonalSquares)
            {
                var piece = board.GetPiece(new Square(square));
                char file = (char)('a' + (square % 8));
                int rank = (square / 8) + 1;
                Console.WriteLine($"  {file}{rank} (index {square}): {piece}");
            }
            
            // Generate legal moves for bishop on c1 if it exists
            if (!c1Piece.IsNull && c1Piece.PieceType == PieceType.Bishop)
            {
                Console.WriteLine("\nGenerating moves for bishop on c1:");
                var moveGen = new MoveGenerator(board);
                var allMoves = board.GetLegalMoves();
                
                foreach (var move in allMoves)
                {
                    if (move.StartSquare.Index == 2) // c1
                    {
                        char startFile = (char)('a' + (move.StartSquare.Index % 8));
                        int startRank = (move.StartSquare.Index / 8) + 1;
                        char endFile = (char)('a' + (move.TargetSquare.Index % 8));
                        int endRank = (move.TargetSquare.Index / 8) + 1;
                        Console.WriteLine($"  {startFile}{startRank}{endFile}{endRank} (from {move.StartSquare.Index} to {move.TargetSquare.Index})");
                    }
                }
            }
            
            Console.WriteLine("=== END ANALYSIS ===");
        }
    }
}
