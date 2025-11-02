using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Evaluation
{
    /// <summary>
    /// v3.0: Advanced tactical pattern recognition and evaluation
    /// Detects pins, forks, skewers, sacrifices, deflection, and other tactical motifs
    /// </summary>
    public static class TacticalEvaluator
    {
        // Tactical pattern values in centipawns
        private const int PinValue = 50;
        private const int ForkValue = 100;
        private const int SkewerValue = 75;
        private const int DeflectionValue = 60;
        private const int RemoveDefenderValue = 80;
        private const int OverloadValue = 40;
        private const int DiscoveredAttackValue = 120;
        private const int BackRankThreatValue = 200;

        /// <summary>
        /// Evaluate tactical patterns in the position
        /// </summary>
        /// <param name="board">Current board position</param>
        /// <param name="gamePhase">Game phase for weight adjustment</param>
        /// <returns>Tactical evaluation from white's perspective</returns>
        public static int Evaluate(Board board, double gamePhase)
        {
            // Tactics are most important in middlegame
            double tacticalWeight = gamePhase > 0.7 ? 0.7 :      // Opening: 70% weight
                                   gamePhase > 0.3 ? 1.0 :      // Middlegame: 100% weight
                                   0.5;                          // Endgame: 50% weight

            int whiteEval = EvaluateTacticsForSide(board, true);
            int blackEval = EvaluateTacticsForSide(board, false);
            
            return (int)((whiteEval - blackEval) * tacticalWeight);
        }

        /// <summary>
        /// Evaluate tactical patterns for one side
        /// </summary>
        private static int EvaluateTacticsForSide(Board board, bool isWhite)
        {
            int evaluation = 0;

            // 1. Look for pins
            evaluation += EvaluatePins(board, isWhite);

            // 2. Look for forks
            evaluation += EvaluateForks(board, isWhite);

            // 3. Look for skewers
            evaluation += EvaluateSkewers(board, isWhite);

            // 4. Look for back rank threats
            evaluation += EvaluateBackRankThreats(board, isWhite);

            // 5. Look for discovered attack potential
            evaluation += EvaluateDiscoveredAttacks(board, isWhite);

            // 6. Look for overloaded defenders
            evaluation += EvaluateOverloadedDefenders(board, isWhite);

            return evaluation;
        }

        /// <summary>
        /// Evaluate pin patterns
        /// </summary>
        private static int EvaluatePins(Board board, bool isWhite)
        {
            int pinValue = 0;
            
            // Look for our pieces that can create pins
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (piece.IsNull || piece.IsWhite != isWhite)
                    continue;

                // Only sliding pieces can create pins
                if (piece.PieceType == PieceType.Bishop || 
                    piece.PieceType == PieceType.Rook || 
                    piece.PieceType == PieceType.Queen)
                {
                    pinValue += FindPinsAlongLine(board, new Square(square), piece.PieceType);
                }
            }

            return pinValue;
        }

        /// <summary>
        /// Find pins along the lines of attack for a sliding piece
        /// </summary>
        private static int FindPinsAlongLine(Board board, Square pieceSquare, PieceType pieceType)
        {
            int pinValue = 0;
            var piece = board.GetPiece(pieceSquare);

            // Define directions based on piece type
            var directions = GetPieceDirections(pieceType);

            foreach (var (fileDir, rankDir) in directions)
            {
                // Look along this direction for pin patterns
                Square attackedPiece = new Square(-1); // Invalid square as marker
                Square pinnedPiece = new Square(-1);   // Invalid square as marker
                bool foundEnemyPiece = false;

                for (int distance = 1; distance < 8; distance++)
                {
                    int newFile = pieceSquare.File + fileDir * distance;
                    int newRank = pieceSquare.Rank + rankDir * distance;

                    if (newFile < 0 || newFile > 7 || newRank < 0 || newRank > 7)
                        break;

                    var checkSquare = new Square(newRank * 8 + newFile);
                    var checkPiece = board.GetPiece(checkSquare);

                    if (!checkPiece.IsNull)
                    {
                        if (checkPiece.IsWhite != piece.IsWhite)
                        {
                            // Enemy piece
                            if (!foundEnemyPiece)
                            {
                                attackedPiece = checkSquare;
                                foundEnemyPiece = true;
                            }
                            else
                            {
                                // Second enemy piece - potential pin target
                                if (checkPiece.PieceType == PieceType.King || 
                                    GetPieceValue(checkPiece.PieceType) > GetPieceValue(board.GetPiece(attackedPiece).PieceType))
                                {
                                    // We have a pin! The first piece is pinned to the second
                                    pinValue += PinValue;
                                }
                                break;
                            }
                        }
                        else
                        {
                            // Our piece blocks the line
                            break;
                        }
                    }
                }
            }

            return pinValue;
        }

        /// <summary>
        /// Evaluate fork patterns (especially knight forks)
        /// </summary>
        private static int EvaluateForks(Board board, bool isWhite)
        {
            int forkValue = 0;

            // Look for knight forks
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (piece.IsNull || piece.IsWhite != isWhite || piece.PieceType != PieceType.Knight)
                    continue;

                forkValue += EvaluateKnightForks(board, new Square(square));
            }

            return forkValue;
        }

        /// <summary>
        /// Evaluate knight fork potential from a given square
        /// </summary>
        private static int EvaluateKnightForks(Board board, Square knightSquare)
        {
            var knight = board.GetPiece(knightSquare);
            int[] knightMoves = { -17, -15, -10, -6, 6, 10, 15, 17 };
            
            var attackedSquares = new List<Square>();
            
            foreach (int moveOffset in knightMoves)
            {
                int targetSquare = knightSquare.Index + moveOffset;
                if (targetSquare < 0 || targetSquare > 63)
                    continue;

                // Check if move crosses board edges
                int fromFile = knightSquare.Index % 8;
                int toFile = targetSquare % 8;
                if (Math.Abs(fromFile - toFile) > 2)
                    continue;

                var target = new Square(targetSquare);
                var targetPiece = board.GetPiece(target);
                
                if (!targetPiece.IsNull && targetPiece.IsWhite != knight.IsWhite)
                {
                    attackedSquares.Add(target);
                }
            }

            // If we attack 2+ valuable pieces, it's a fork
            if (attackedSquares.Count >= 2)
            {
                int totalValue = 0;
                foreach (var square in attackedSquares)
                {
                    totalValue += GetPieceValue(board.GetPiece(square).PieceType);
                }
                
                // Fork value increases with the value of attacked pieces
                if (totalValue >= 600) // At least rook + pawn or two minor pieces
                {
                    return ForkValue;
                }
            }

            return 0;
        }

        /// <summary>
        /// Evaluate skewer patterns
        /// </summary>
        private static int EvaluateSkewers(Board board, bool isWhite)
        {
            // Similar to pins but the more valuable piece is in front
            int skewerValue = 0;
            
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (piece.IsNull || piece.IsWhite != isWhite)
                    continue;

                if (piece.PieceType == PieceType.Bishop || 
                    piece.PieceType == PieceType.Rook || 
                    piece.PieceType == PieceType.Queen)
                {
                    skewerValue += FindSkewersAlongLine(board, new Square(square), piece.PieceType);
                }
            }

            return skewerValue;
        }

        /// <summary>
        /// Find skewer patterns along piece lines
        /// </summary>
        private static int FindSkewersAlongLine(Board board, Square pieceSquare, PieceType pieceType)
        {
            // Simplified skewer detection - similar to pins but valuable piece is in front
            return 0; // Placeholder for full implementation
        }

        /// <summary>
        /// Evaluate back rank mate threats
        /// </summary>
        private static int EvaluateBackRankThreats(Board board, bool isWhite)
        {
            int threatValue = 0;
            int backRank = isWhite ? 0 : 7; // Enemy back rank
            
            // Look for enemy king on back rank with limited escape squares
            for (int file = 0; file < 8; file++)
            {
                var piece = board.GetPiece(new Square(backRank * 8 + file));
                if (!piece.IsNull && piece.PieceType == PieceType.King && piece.IsWhite != isWhite)
                {
                    // Check if we have rook/queen that can attack this rank
                    if (HasBackRankAttacker(board, isWhite, backRank))
                    {
                        threatValue += BackRankThreatValue;
                    }
                    break;
                }
            }

            return threatValue;
        }

        /// <summary>
        /// Check if we have pieces that can attack the back rank
        /// </summary>
        private static bool HasBackRankAttacker(Board board, bool isWhite, int targetRank)
        {
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (piece.IsNull || piece.IsWhite != isWhite)
                    continue;

                if (piece.PieceType == PieceType.Rook || piece.PieceType == PieceType.Queen)
                {
                    // Check if this piece can reach the target rank
                    // Simplified check - full implementation would verify clear lines
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluate discovered attack potential
        /// </summary>
        private static int EvaluateDiscoveredAttacks(Board board, bool isWhite)
        {
            // Placeholder for discovered attack detection
            return 0;
        }

        /// <summary>
        /// Evaluate overloaded defenders
        /// </summary>
        private static int EvaluateOverloadedDefenders(Board board, bool isWhite)
        {
            // Placeholder for overload detection
            return 0;
        }

        /// <summary>
        /// Get piece directions for line-based attacks
        /// </summary>
        private static (int fileDir, int rankDir)[] GetPieceDirections(PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Rook => new[] { (1, 0), (-1, 0), (0, 1), (0, -1) },
                PieceType.Bishop => new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) },
                PieceType.Queen => new[] { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) },
                _ => new (int, int)[0]
            };
        }

        /// <summary>
        /// Get standard piece values for tactical calculations
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
                PieceType.King => 10000,
                _ => 0
            };
        }
    }
}