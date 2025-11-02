using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Search
{
    /// <summary>
    /// History heuristic for move ordering
    /// Tracks how often moves cause beta cutoffs to improve move ordering
    /// </summary>
    public class HistoryTable
    {
        private const int MaxSquares = 64;
        
        // [from_square][to_square][piece_type][is_white]
        private readonly int[,,,] history = new int[MaxSquares, MaxSquares, 7, 2];
        
        /// <summary>
        /// Update history score for a move that caused a beta cutoff
        /// </summary>
        public void UpdateHistory(Move move, int depth, Board board)
        {
            var piece = board.GetPiece(move.StartSquare);
            if (piece.IsNull) return;
            
            int fromSq = move.StartSquare.Index;
            int toSq = move.TargetSquare.Index;
            int pieceType = (int)piece.PieceType;
            int colorIndex = piece.IsWhite ? 1 : 0;
            
            if (fromSq < MaxSquares && toSq < MaxSquares && pieceType < 7)
            {
                // Increase history score, with higher bonus for deeper searches
                history[fromSq, toSq, pieceType, colorIndex] += depth * depth;
                
                // Prevent overflow by capping at reasonable maximum
                if (history[fromSq, toSq, pieceType, colorIndex] > 10000)
                {
                    history[fromSq, toSq, pieceType, colorIndex] = 10000;
                }
            }
        }
        
        /// <summary>
        /// Get history score for move ordering
        /// </summary>
        public int GetHistoryScore(Move move, Board board)
        {
            var piece = board.GetPiece(move.StartSquare);
            if (piece.IsNull) return 0;
            
            int fromSq = move.StartSquare.Index;
            int toSq = move.TargetSquare.Index;
            int pieceType = (int)piece.PieceType;
            int colorIndex = piece.IsWhite ? 1 : 0;
            
            if (fromSq < MaxSquares && toSq < MaxSquares && pieceType < 7)
            {
                return history[fromSq, toSq, pieceType, colorIndex];
            }
            
            return 0;
        }
        
        /// <summary>
        /// Clear all history scores (called at start of new game)
        /// </summary>
        public void Clear()
        {
            Array.Clear(history, 0, history.Length);
        }
        
        /// <summary>
        /// Age history scores by dividing by 2 (called periodically to reduce old data)
        /// </summary>
        public void Age()
        {
            for (int from = 0; from < MaxSquares; from++)
            {
                for (int to = 0; to < MaxSquares; to++)
                {
                    for (int piece = 0; piece < 7; piece++)
                    {
                        for (int color = 0; color < 2; color++)
                        {
                            history[from, to, piece, color] /= 2;
                        }
                    }
                }
            }
        }
    }
}