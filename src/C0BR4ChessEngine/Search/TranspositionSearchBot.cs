using System;
using System.Collections.Generic;
using C0BR4ChessEngine.Core;
using C0BR4ChessEngine.Evaluation;

namespace C0BR4ChessEngine.Search
{
    /// <summary>
    /// Alpha-beta search with move ordering, quiescence search, and transposition table
    /// Transposition table caches previously computed positions to avoid re-search
    /// </summary>
    public class TranspositionSearchBot : IChessBot
    {
        private readonly SimpleEvaluator evaluator = new();
        private readonly TranspositionTable transpositionTable = new(100000); // 100K entries
        private long nodesSearched = 0;
        private long quiescenceNodes = 0;
        private int searchDepth = 6; // v3.0: Enhanced default search depth
        private int maxDepth = 10; // v3.0: Target maximum depth for iterative deepening
        private List<Move> currentPV = new(); // Principal variation line
        private List<Move> lastOpponentPV = new(); // v3.0: Track opponent's expected PV
        private Move lastOurMove = Move.NullMove; // v3.0: Track our last move for PV followup

        public Move Think(Board board, TimeSpan timeLimit)
        {
            nodesSearched = 0;
            quiescenceNodes = 0;
            
            // v3.0: PV Fast Followup - check if opponent played into our predicted line
            if (ShouldUsePVFollowup(board))
            {
                var pvMove = GetNextPVMove();
                if (pvMove != Move.NullMove && board.IsLegalMove(pvMove))
                {
                    Console.WriteLine($"info string PV followup: using predicted move {pvMove}");
                    lastOurMove = pvMove;
                    return pvMove;
                }
            }
            
            currentPV.Clear();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Safety check - ensure we have legal moves before searching
            var legalMoves = board.GetLegalMoves();
            if (legalMoves.Length == 0)
            {
                Console.WriteLine("info string No legal moves in position");
                return Move.NullMove;
            }
            
            Move bestMove = legalMoves[0];
            int bestScore = -50000;
            
            // Enhanced iterative deepening search - target depth 10
            int maxSearchDepth = Math.Min(maxDepth, searchDepth + 4); // Allow up to 4 extra plies based on time
            for (int depth = 1; depth <= maxSearchDepth; depth++)
            {
                // Smart time allocation: reserve time for deeper searches
                double timeUsedRatio = (double)stopwatch.ElapsedMilliseconds / timeLimit.TotalMilliseconds;
                double progressRatio = (double)depth / maxSearchDepth;
                
                // If we're using time faster than progress, be more conservative
                if (timeUsedRatio > progressRatio * 0.7 && depth > searchDepth)
                    break;
                    
                var (move, score, pv) = SearchWithPV(board, depth);
                
                if (move != Move.NullMove)
                {
                    bestMove = move;
                    bestScore = score;
                    currentPV = pv;
                    
                    // Output UCI info for this depth
                    var pvString = string.Join(" ", currentPV.Select(m => m.ToString()));
                    var elapsed = stopwatch.ElapsedMilliseconds;
                    var nps = elapsed > 0 ? (long)(nodesSearched * 1000 / elapsed) : 0;
                    
                    Console.WriteLine($"info depth {depth} score cp {bestScore} nodes {nodesSearched} nps {nps} time {elapsed} pv {pvString}");
                }
                
                // Break if we found a mate
                if (Math.Abs(bestScore) > 20000)
                    break;
            }
            
            stopwatch.Stop();
            
            // Final validation - ensure we return a legal move
            if (bestMove.IsNull || !IsMoveLegal(board, bestMove, legalMoves))
            {
                Console.WriteLine($"info string Warning: Invalid best move {bestMove}, using fallback");
                bestMove = legalMoves[0]; // Return any legal move as fallback
            }
            
            // Get transposition table statistics
            var (ttHits, ttStores, ttEntries) = transpositionTable.GetStatistics();
            
            // Final search summary
            Console.WriteLine($"info string Search completed: depth {searchDepth} nodes {nodesSearched} qnodes {quiescenceNodes} tthits {ttHits}");
            
            return bestMove;
        }

        private (Move bestMove, int bestScore, List<Move> pv) SearchWithPV(Board board, int depth)
        {
            var moves = board.GetLegalMoves();
            if (moves.Length == 0)
            {
                return (Move.NullMove, -50000, new List<Move>());
            }

            // Order moves for better alpha-beta pruning
            moves = MoveOrdering.OrderMoves(board, moves);

            Move bestMove = moves[0];
            int bestScore = -50000;
            List<Move> bestPV = new();

            // Check transposition table for a previous best move to try first
            if (transpositionTable.TryGetEntry(board, depth, -50000, 50000, out var ttEntry))
            {
                if (ttEntry.BestMove != Move.NullMove && IsMoveLegal(board, ttEntry.BestMove, moves))
                {
                    for (int i = 0; i < moves.Length; i++)
                    {
                        if (moves[i].Equals(ttEntry.BestMove))
                        {
                            (moves[0], moves[i]) = (moves[i], moves[0]);
                            break;
                        }
                    }
                }
            }

            foreach (var move in moves)
            {
                board.MakeMove(move);
                var (score, childPV) = AlphaBetaWithPV(board, depth - 1, -50000, -bestScore);
                score = -score;
                board.UnmakeMove();

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                    bestPV = new List<Move> { move };
                    bestPV.AddRange(childPV);
                }
            }

            // Store the result in the transposition table
            transpositionTable.StoreEntry(board, depth, bestScore, bestMove, -50000, 50000);

            return (bestMove, bestScore, bestPV);
        }

        /// <summary>
        /// Check if a move from transposition table is still legal in current position
        /// </summary>
        private bool IsMoveLegal(Board board, Move move, Move[] legalMoves)
        {
            foreach (var legalMove in legalMoves)
            {
                if (move.Equals(legalMove))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Alpha-beta search with Principal Variation collection
        /// </summary>
        private (int score, List<Move> pv) AlphaBetaWithPV(Board board, int depth, int alpha, int beta)
        {
            nodesSearched++;

            // Check transposition table first
            if (transpositionTable.TryGetEntry(board, depth, alpha, beta, out var ttEntry))
            {
                return (ttEntry.Score, new List<Move>());
            }

            // Base case: enter quiescence search
            if (depth == 0)
            {
                int qScore = Quiescence(board, alpha, beta);
                transpositionTable.StoreEntry(board, 0, qScore, Move.NullMove, alpha, beta);
                return (qScore, new List<Move>());
            }

            var moves = board.GetLegalMoves();
            
            // Check for terminal positions
            if (moves.Length == 0)
            {
                int terminalScore;
                if (board.IsInCheck())
                {
                    terminalScore = -30000 + (searchDepth - depth);
                }
                else
                {
                    terminalScore = 0;
                }
                
                transpositionTable.StoreEntry(board, depth, terminalScore, Move.NullMove, alpha, beta);
                return (terminalScore, new List<Move>());
            }

            // Order moves for better pruning
            moves = MoveOrdering.OrderMoves(board, moves);

            // If we have a cached best move from TT, try it first
            if (ttEntry.BestMove != Move.NullMove && IsMoveLegal(board, ttEntry.BestMove, moves))
            {
                for (int i = 0; i < moves.Length; i++)
                {
                    if (moves[i].Equals(ttEntry.BestMove))
                    {
                        (moves[0], moves[i]) = (moves[i], moves[0]);
                        break;
                    }
                }
            }

            int maxScore = alpha;
            Move bestMove = Move.NullMove;
            List<Move> bestPV = new();

            // Try each move
            foreach (var move in moves)
            {
                board.MakeMove(move);
                var (score, childPV) = AlphaBetaWithPV(board, depth - 1, -beta, -maxScore);
                score = -score;
                board.UnmakeMove();

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                    bestPV = new List<Move> { move };
                    bestPV.AddRange(childPV);
                }

                // Alpha-beta cutoff
                if (maxScore >= beta)
                {
                    transpositionTable.StoreEntry(board, depth, maxScore, bestMove, alpha, beta);
                    return (beta, bestPV);
                }
            }

            // Store the search result
            transpositionTable.StoreEntry(board, depth, maxScore, bestMove, alpha, beta);
            return (maxScore, bestPV);
        }

        /// <summary>
        /// Alpha-beta search with transposition table lookups
        /// </summary>
        private int AlphaBeta(Board board, int depth, int alpha, int beta)
        {
            nodesSearched++;

            // Check transposition table first
            if (transpositionTable.TryGetEntry(board, depth, alpha, beta, out var ttEntry))
            {
                return ttEntry.Score;
            }

            // Base case: enter quiescence search
            if (depth == 0)
            {
                int qScore = Quiescence(board, alpha, beta);
                // Store quiescence result in TT with depth 0
                transpositionTable.StoreEntry(board, 0, qScore, Move.NullMove, alpha, beta);
                return qScore;
            }

            var moves = board.GetLegalMoves();
            
            // Check for terminal positions
            if (moves.Length == 0)
            {
                int terminalScore;
                if (board.IsInCheck())
                {
                    // Checkmate - return very negative score, adjusted for depth to prefer quicker mates
                    terminalScore = -30000 + (searchDepth - depth);
                }
                else
                {
                    // Stalemate
                    terminalScore = 0;
                }
                
                // Store terminal result
                transpositionTable.StoreEntry(board, depth, terminalScore, Move.NullMove, alpha, beta);
                return terminalScore;
            }

            // Order moves for better pruning
            moves = MoveOrdering.OrderMoves(board, moves);

            // If we have a cached best move from TT, try it first
            if (ttEntry.BestMove != Move.NullMove && IsMoveLegal(board, ttEntry.BestMove, moves))
            {
                for (int i = 0; i < moves.Length; i++)
                {
                    if (moves[i].Equals(ttEntry.BestMove))
                    {
                        (moves[0], moves[i]) = (moves[i], moves[0]);
                        break;
                    }
                }
            }

            int maxScore = alpha;
            Move bestMove = Move.NullMove;

            // Try each move
            foreach (var move in moves)
            {
                board.MakeMove(move);
                int score = -AlphaBeta(board, depth - 1, -beta, -maxScore);
                board.UnmakeMove();

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }

                // Alpha-beta cutoff
                if (maxScore >= beta)
                {
                    // Store the cutoff result
                    transpositionTable.StoreEntry(board, depth, maxScore, bestMove, alpha, beta);
                    return beta; // Fail-high (beta cutoff)
                }
            }

            // Store the search result
            transpositionTable.StoreEntry(board, depth, maxScore, bestMove, alpha, beta);
            return maxScore;
        }

        /// <summary>
        /// Quiescence search - search only captures and checks until position is "quiet"
        /// </summary>
        private int Quiescence(Board board, int alpha, int beta)
        {
            quiescenceNodes++;

            // Stand-pat evaluation
            int standPat = evaluator.Evaluate(board);
            
            if (standPat >= beta)
                return beta;
            
            if (standPat > alpha)
                alpha = standPat;

            // Generate and search tactical moves
            var tacticalMoves = GetTacticalMoves(board);
            
            if (tacticalMoves.Length == 0)
                return standPat;

            // Order tactical moves
            tacticalMoves = MoveOrdering.OrderMoves(board, tacticalMoves);

            foreach (var move in tacticalMoves)
            {
                board.MakeMove(move);
                int score = -Quiescence(board, -beta, -alpha);
                board.UnmakeMove();

                if (score >= beta)
                    return beta;

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        /// <summary>
        /// Get only tactical moves for quiescence search
        /// </summary>
        private Move[] GetTacticalMoves(Board board)
        {
            var allMoves = board.GetLegalMoves();
            var tacticalMoves = new List<Move>();

            foreach (var move in allMoves)
            {
                // Include captures
                var targetPiece = board.GetPiece(move.TargetSquare);
                if (targetPiece.PieceType != PieceType.None)
                {
                    tacticalMoves.Add(move);
                    continue;
                }

                // Include promotions
                if (move.PromotionPieceType != PieceType.None)
                {
                    tacticalMoves.Add(move);
                    continue;
                }
            }

            return tacticalMoves.ToArray();
        }

        /// <summary>
        /// Set the search depth for the engine
        /// </summary>
        public void SetDepth(int depth)
        {
            searchDepth = Math.Max(1, Math.Min(depth, 10));
        }

        /// <summary>
        /// Get current search statistics
        /// </summary>
        public long GetNodesSearched() => nodesSearched;
        
        /// <summary>
        /// Get quiescence search statistics
        /// </summary>
        public long GetQuiescenceNodes() => quiescenceNodes;

        /// <summary>
        /// Clear the transposition table (useful between games)
        /// </summary>
        public void ClearTranspositionTable()
        {
            transpositionTable.Clear();
        }

        /// <summary>
        /// v3.0: Check if we should use PV followup instead of full search
        /// </summary>
        private bool ShouldUsePVFollowup(Board board)
        {
            // Only use PV followup if we have a stored PV and it's not our first move
            if (currentPV.Count < 2 || lastOurMove.IsNull)
                return false;

            // Check if the opponent played the move we expected (first move in our PV after our move)
            // This is a simplified check - in a full implementation, we'd track the full game history
            return currentPV.Count > 1; // For now, just check if we have a multi-move PV
        }

        /// <summary>
        /// v3.0: Get the next move from our stored PV
        /// </summary>
        private Move GetNextPVMove()
        {
            // Return the next move in our PV sequence
            // In a full implementation, we'd need to track which move in the PV we should use
            if (currentPV.Count >= 2)
                return currentPV[1]; // Return our next planned move
            
            return Move.NullMove;
        }

        /// <summary>
        /// v3.0: Update PV tracking after making a move
        /// </summary>
        public void UpdatePVTracking(Move ourMove)
        {
            lastOurMove = ourMove;
            // Shift PV to account for moves played
            if (currentPV.Count >= 2)
            {
                currentPV.RemoveRange(0, 2); // Remove our move and opponent's expected response
            }
        }
    }
}
