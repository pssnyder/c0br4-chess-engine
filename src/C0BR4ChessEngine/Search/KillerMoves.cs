using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Search
{
    /// <summary>
    /// Killer move heuristic for move ordering
    /// Stores non-capture moves that caused beta cutoffs at each depth
    /// </summary>
    public class KillerMoves
    {
        private const int MaxDepth = 32;
        private const int KillersPerDepth = 2;
        
        private readonly Move[,] killers = new Move[MaxDepth, KillersPerDepth];
        
        /// <summary>
        /// Store a killer move at the given depth
        /// </summary>
        public void StoreKiller(Move move, int depth)
        {
            if (depth >= MaxDepth) return;
            
            // Shift existing killers down and insert new one at index 0
            if (!killers[depth, 0].Equals(move))
            {
                killers[depth, 1] = killers[depth, 0];
                killers[depth, 0] = move;
            }
        }
        
        /// <summary>
        /// Check if a move is a killer at the given depth
        /// </summary>
        public bool IsKiller(Move move, int depth)
        {
            if (depth >= MaxDepth) return false;
            
            return killers[depth, 0].Equals(move) || killers[depth, 1].Equals(move);
        }
        
        /// <summary>
        /// Get killer move bonus for move ordering
        /// </summary>
        public int GetKillerBonus(Move move, int depth)
        {
            if (depth >= MaxDepth) return 0;
            
            if (killers[depth, 0].Equals(move)) return 900; // Primary killer
            if (killers[depth, 1].Equals(move)) return 800; // Secondary killer
            
            return 0;
        }
        
        /// <summary>
        /// Clear all killer moves (called at start of search)
        /// </summary>
        public void Clear()
        {
            Array.Clear(killers, 0, killers.Length);
        }
    }
}