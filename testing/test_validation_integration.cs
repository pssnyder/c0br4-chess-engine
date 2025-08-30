using System;
using ChessEngine.Core;
using ChessEngine.Testing;

namespace ChessEngine.Testing
{
    /// <summary>
    /// Test script to verify MoveValidator and IllegalMoveDebugger integration
    /// Tests basic functionality and error handling
    /// </summary>
    class TestValidationIntegration
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== C0BR4 Validation Integration Test ===\n");
            
            // Clear any existing log file
            IllegalMoveDebugger.ClearLogFile();
            
            try
            {
                // Test 1: Standard starting position
                Console.WriteLine("Test 1: Starting position validation");
                var board = new Board();
                
                // Log initial board state
                IllegalMoveDebugger.LogBoardStateAnalysis(board, "Starting Position");
                
                // Test a valid move
                var legalMoves = board.GetLegalMoves();
                if (legalMoves.Length > 0)
                {
                    var firstMove = legalMoves[0];
                    Console.WriteLine($"Testing valid move: {firstMove}");
                    
                    var validation = MoveValidator.ValidateMove(board, firstMove);
                    Console.WriteLine($"Validation result: {validation.IsValid} - {validation.ErrorMessage}");
                    
                    if (validation.IsValid)
                    {
                        board.MakeMove(firstMove);
                        IllegalMoveDebugger.LogBoardStateAnalysis(board, "After first move");
                    }
                }
                
                // Test 2: Try to create an invalid move scenario
                Console.WriteLine("\nTest 2: Invalid move detection");
                
                // Create a move that should be invalid (king moving to occupied square)
                var invalidMove = new Move(new Square(4), new Square(12), PieceType.King); // e1 to e2 
                var invalidValidation = MoveValidator.ValidateMove(board, invalidMove);
                Console.WriteLine($"Invalid move test: {invalidMove}");
                Console.WriteLine($"Validation result: {invalidValidation.IsValid} - {invalidValidation.ErrorMessage}");
                
                if (!invalidValidation.IsValid)
                {
                    IllegalMoveDebugger.LogIllegalMoveAttempt(board, invalidMove, invalidValidation.ErrorMessage);
                }
                
                // Test 3: Test FEN generation
                Console.WriteLine("\nTest 3: FEN generation");
                var fen = board.GetFEN();
                Console.WriteLine($"Current FEN: {fen}");
                
                Console.WriteLine("\n=== Test completed successfully ===");
                Console.WriteLine("Check 'illegal_moves.log' for detailed logging output");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed with exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
