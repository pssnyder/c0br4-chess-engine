using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Evaluation
{
    /// <summary>
    /// Enhanced position evaluation with material counting, piece-square tables, 
    /// game phase detection, and advanced evaluation features for improved play
    /// </summary>
    public class SimpleEvaluator
    {
        // Material values in centipawns
        private static readonly int[] PieceValues = { 0, 100, 300, 300, 500, 900, 0 }; // None, Pawn, Knight, Bishop, Rook, Queen, King

        /// <summary>
        /// Evaluate the position from the perspective of the side to move
        /// Positive = good for side to move, Negative = bad for side to move
        /// </summary>
        public int Evaluate(Board board)
        {
            int evaluation = 0;
            
            // Calculate game phase for PST interpolation and feature weighting
            double gamePhase = GamePhase.CalculatePhase(board);
            
            // Core evaluation components
            evaluation += EvaluateMaterial(board);
            evaluation += PieceSquareTables.EvaluatePosition(board, gamePhase);
            
            // Advanced evaluation features
            evaluation += RookCoordination.Evaluate(board, gamePhase);
            evaluation += KingSafety.Evaluate(board, gamePhase);
            evaluation += KingEndgame.Evaluate(board, gamePhase);
            evaluation += CastlingIncentive.Evaluate(board, gamePhase);
            evaluation += CastlingRights.Evaluate(board, gamePhase);
            
            // DISABLED: Advanced endgame heuristics (potential performance impact)
            // TODO v3.1: Profile and optimize before re-enabling
            // evaluation += AdvancedEndgame.Evaluate(board, gamePhase);
            
            // DISABLED: Tactical pattern recognition (causing 10x performance regression)
            // TODO v3.1: Optimize TacticalEvaluator before re-enabling
            // evaluation += TacticalEvaluator.Evaluate(board, gamePhase);
            
            // Return from perspective of side to move
            return board.IsWhiteToMove ? evaluation : -evaluation;
        }

        /// <summary>
        /// Calculate material difference from white's perspective
        /// v3.0: Optimized using bitboards instead of 64-square loop
        /// </summary>
        private int EvaluateMaterial(Board board)
        {
            var pos = board.GetBitboardPosition();
            
            // Count pieces using bitboard pop count (much faster than loops)
            int whiteValue = 
                Bitboard.PopCount(pos.WhitePawns) * PieceValues[1] +     // Pawns
                Bitboard.PopCount(pos.WhiteKnights) * PieceValues[2] +   // Knights  
                Bitboard.PopCount(pos.WhiteBishops) * PieceValues[3] +   // Bishops
                Bitboard.PopCount(pos.WhiteRooks) * PieceValues[4] +     // Rooks
                Bitboard.PopCount(pos.WhiteQueens) * PieceValues[5];     // Queens
                // King value not counted (always 1)
            
            int blackValue = 
                Bitboard.PopCount(pos.BlackPawns) * PieceValues[1] +     // Pawns
                Bitboard.PopCount(pos.BlackKnights) * PieceValues[2] +   // Knights
                Bitboard.PopCount(pos.BlackBishops) * PieceValues[3] +   // Bishops
                Bitboard.PopCount(pos.BlackRooks) * PieceValues[4] +     // Rooks
                Bitboard.PopCount(pos.BlackQueens) * PieceValues[5];     // Queens
                // King value not counted (always 1)
            
            return whiteValue - blackValue;
        }
    }
}
