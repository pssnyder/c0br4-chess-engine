using System;
using System.Collections.Generic;
using System.Linq;
using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Search
{
    /// <summary>
    /// Move ordering utility to improve alpha-beta pruning efficiency
    /// Orders moves by expected strength: captures, promotions, checks, then others
    /// v3.0: Enhanced with killer moves and history heuristic
    /// </summary>
    public static class MoveOrdering
    {
        /// <summary>
        /// Order moves for better alpha-beta pruning with killer moves and history
        /// Most promising moves first to maximize cutoffs
        /// </summary>
        public static Move[] OrderMoves(Board board, Move[] moves, int depth = 0, 
            KillerMoves? killerMoves = null, HistoryTable? historyTable = null)
        {
            if (moves.Length <= 1) return moves;

            // Create array of moves with their scores for sorting
            var scoredMoves = new (Move move, int score)[moves.Length];
            
            for (int i = 0; i < moves.Length; i++)
            {
                scoredMoves[i] = (moves[i], ScoreMove(board, moves[i], depth, killerMoves, historyTable));
            }

            // Sort by score (highest first)
            Array.Sort(scoredMoves, (a, b) => b.score.CompareTo(a.score));

            // Extract the ordered moves
            var orderedMoves = new Move[moves.Length];
            for (int i = 0; i < moves.Length; i++)
            {
                orderedMoves[i] = scoredMoves[i].move;
            }

            return orderedMoves;
        }

        /// <summary>
        /// v3.0: Enhanced move scoring with killer moves and history heuristic
        /// Higher scores = more promising moves that should be searched first
        /// </summary>
        private static int ScoreMove(Board board, Move move, int depth = 0, 
            KillerMoves? killerMoves = null, HistoryTable? historyTable = null)
        {
            int score = 0;

            // Get piece types
            var movingPiece = board.GetPiece(move.StartSquare);
            var targetPiece = board.GetPiece(move.TargetSquare);

            // 1. Captures - prioritize by value difference (MVV-LVA: Most Valuable Victim - Least Valuable Attacker)
            if (targetPiece.PieceType != PieceType.None)
            {
                int captureValue = GetPieceValue(targetPiece.PieceType) - GetPieceValue(movingPiece.PieceType);
                score += 10000 + captureValue; // Base capture score + value difference

                // v3.0: Bonus for capturing undefended pieces
                if (!IsSquareDefended(board, move.TargetSquare, !movingPiece.IsWhite))
                {
                    score += 200; // Extra bonus for capturing undefended pieces
                }
            }
            else
            {
                // Non-capture moves: use killer moves and history heuristic
                
                // 2. Killer moves - non-captures that caused beta cutoffs
                if (killerMoves != null)
                {
                    score += killerMoves.GetKillerBonus(move, depth);
                }
                
                // 3. History heuristic - moves that historically caused cutoffs
                if (historyTable != null)
                {
                    score += historyTable.GetHistoryScore(move, board);
                }
            }

            // 4. Promotions - very valuable
            if (move.PromotionPieceType != PieceType.None)
            {
                score += 9000 + GetPieceValue(move.PromotionPieceType);
            }

            // 5. Checks - often strong moves, especially discovered checks
            board.MakeMove(move);
            bool givesCheck = board.IsInCheck();
            board.UnmakeMove();
            
            if (givesCheck)
            {
                score += 500;
                
                // v3.0: Extra bonus for discovered checks
                if (IsDiscoveredCheck(board, move))
                {
                    score += 200;
                }
            }

            // v3.0: 4. Threats - moves that threaten valuable pieces
            int threatValue = EvaluateThreats(board, move);
            score += threatValue;

            // v3.0: 5. Tactical patterns
            int tacticalValue = EvaluateTacticalPatterns(board, move);
            score += tacticalValue;

            // 6. Center control (minor bonus)
            if (IsCenter(move.TargetSquare))
            {
                score += 10;
            }

            // 7. Piece development (minor bonus for knights and bishops)
            if ((movingPiece.PieceType == PieceType.Knight || movingPiece.PieceType == PieceType.Bishop))
            {
                if (IsBackRank(move.StartSquare, movingPiece.IsWhite))
                {
                    score += 5; // Development from back rank
                }
            }

            // v3.0: 8. Penalty for moving into attacked squares (unless capturing)
            if (targetPiece.PieceType == PieceType.None && IsSquareAttacked(board, move.TargetSquare, !movingPiece.IsWhite))
            {
                score -= GetPieceValue(movingPiece.PieceType) / 4; // Penalty for hanging pieces
            }

            return score;
        }

        /// <summary>
        /// Get the relative value of a piece type for capture ordering
        /// </summary>
        private static int GetPieceValue(PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Pawn => 100,
                PieceType.Knight => 300,
                PieceType.Bishop => 300,
                PieceType.Rook => 500,
                PieceType.Queen => 900,
                PieceType.King => 10000, // Should never be captured, but just in case
                _ => 0
            };
        }

        /// <summary>
        /// Check if a square is in the center (e4, e5, d4, d5)
        /// </summary>
        private static bool IsCenter(Square square)
        {
            int file = square.File;
            int rank = square.Rank;
            return (file == 3 || file == 4) && (rank == 3 || rank == 4); // d4, d5, e4, e5
        }

        /// <summary>
        /// Check if a square is on the back rank for the given color
        /// </summary>
        private static bool IsBackRank(Square square, bool isWhite)
        {
            return isWhite ? square.Rank == 0 : square.Rank == 7;
        }

        /// <summary>
        /// v3.0: Check if a square is defended by the specified color
        /// </summary>
        private static bool IsSquareDefended(Board board, Square square, bool byColor)
        {
            return IsSquareAttacked(board, square, byColor);
        }

        /// <summary>
        /// v3.0: Check if a square is attacked by the specified color
        /// </summary>
        private static bool IsSquareAttacked(Board board, Square square, bool byColor)
        {
            // Get all legal moves for the attacking color and see if any attack the square
            // This is a simplified implementation - a full version would be more efficient
            var originalTurn = board.IsWhiteToMove;
            
            // Temporarily set the turn to the attacking color for move generation
            // Note: This is a simplified approach - proper implementation would use attack detection without move generation
            if (originalTurn != byColor)
            {
                // We need the attacking side to move to generate their moves
                // For now, we'll use a simplified approach
                return false; // Simplified - in full implementation, we'd have proper attack detection
            }
            
            var moves = board.GetLegalMoves();
            foreach (var move in moves)
            {
                if (move.TargetSquare.Index == square.Index)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// v3.0: Check if a move creates a discovered check
        /// </summary>
        private static bool IsDiscoveredCheck(Board board, Move move)
        {
            // Simplified check for discovered attacks
            var movingPiece = board.GetPiece(move.StartSquare);
            
            // Look for pieces on the same line as the moving piece that could create discovered attacks
            // This is a simplified implementation
            return false; // Placeholder - full implementation would check for discovered attack patterns
        }

        /// <summary>
        /// v3.0: Evaluate threats created by a move
        /// </summary>
        private static int EvaluateThreats(Board board, Move move)
        {
            int threatValue = 0;
            
            // Make the move and see what pieces we threaten
            board.MakeMove(move);
            
            var ourMoves = board.GetLegalMoves();
            foreach (var threatMove in ourMoves)
            {
                var threatenedPiece = board.GetPiece(threatMove.TargetSquare);
                if (threatenedPiece.PieceType != PieceType.None && 
                    threatenedPiece.IsWhite != board.IsWhiteToMove)
                {
                    // We threaten an enemy piece
                    threatValue += GetPieceValue(threatenedPiece.PieceType) / 10; // 10% of piece value
                }
            }
            
            board.UnmakeMove();
            return Math.Min(threatValue, 300); // Cap threat value
        }

        /// <summary>
        /// v3.0: Evaluate tactical patterns (pins, forks, skewers, etc.)
        /// </summary>
        private static int EvaluateTacticalPatterns(Board board, Move move)
        {
            int tacticalValue = 0;
            
            var movingPiece = board.GetPiece(move.StartSquare);
            
            // Check for fork patterns (knight moves that attack multiple pieces)
            if (movingPiece.PieceType == PieceType.Knight)
            {
                tacticalValue += EvaluateForkPattern(board, move);
            }
            
            // Check for pin/skewer patterns (sliding pieces)
            if (movingPiece.PieceType == PieceType.Bishop || 
                movingPiece.PieceType == PieceType.Rook || 
                movingPiece.PieceType == PieceType.Queen)
            {
                tacticalValue += EvaluatePinSkewPattern(board, move);
            }
            
            return tacticalValue;
        }

        /// <summary>
        /// v3.0: Evaluate potential fork patterns for knight moves
        /// </summary>
        private static int EvaluateForkPattern(Board board, Move move)
        {
            // Simplified fork detection - check if knight move attacks multiple valuable pieces
            board.MakeMove(move);
            
            var knightMoves = board.GetLegalMoves().Where(m => 
                board.GetPiece(m.StartSquare).PieceType == PieceType.Knight &&
                m.StartSquare.Index == move.TargetSquare.Index);
            
            int attackedValue = 0;
            foreach (var knightMove in knightMoves)
            {
                var target = board.GetPiece(knightMove.TargetSquare);
                if (target.PieceType != PieceType.None && target.IsWhite != board.IsWhiteToMove)
                {
                    attackedValue += GetPieceValue(target.PieceType);
                }
            }
            
            board.UnmakeMove();
            
            // If we attack pieces worth more than a knight, it might be a fork
            return attackedValue > 300 ? 100 : 0;
        }

        /// <summary>
        /// v3.0: Evaluate potential pin/skewer patterns for sliding pieces
        /// </summary>
        private static int EvaluatePinSkewPattern(Board board, Move move)
        {
            // Simplified pin/skewer detection
            // Look for patterns where we attack a piece with a more valuable piece behind it
            return 0; // Placeholder - full implementation would check for pin/skewer patterns along lines
        }
    }
}
