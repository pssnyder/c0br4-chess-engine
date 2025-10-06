using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Evaluation
{
    /// <summary>
    /// Advanced endgame evaluation with tablebaseless heuristics
    /// Implements sophisticated endgame patterns: pawn races, king opposition, piece coordination
    /// </summary>
    public static class AdvancedEndgame
    {
        // Evaluation constants in centipawns
        private const int PawnRaceBonus = 150;              // Safe pawn race outside "the box"
        private const int PawnRacePenalty = -75;            // Unsafe pawn race inside "the box"
        private const int RookPawnTacticsBonus = 80;        // Rook + pawn promotion tactics
        private const int Rook7thRankBonus = 60;            // Rook on 7th rank in medium endgame
        private const int KingEdgeForceBonus = 40;          // Forcing enemy king to edge
        private const int BoxClosingBonus = 25;             // Reducing enemy king mobility
        private const int PieceCentralizationBonus = 15;    // Pieces attacking enemy king
        private const int PawnPromotionUrgency = 120;       // Push pawns in very late endgame

        /// <summary>
        /// Evaluate advanced endgame patterns based on game phase
        /// </summary>
        /// <param name="board">Current board position</param>
        /// <param name="gamePhase">Game phase (0.0 = pure endgame, 1.0 = opening)</param>
        /// <returns>Evaluation from white's perspective</returns>
        public static int Evaluate(Board board, double gamePhase)
        {
            // Only apply in endgame phases
            double endgameWeight = 1.0 - gamePhase;
            if (endgameWeight < 0.3) return 0; // Not enough of an endgame

            int evaluation = 0;

            // Different heuristics based on endgame intensity
            bool mediumEndgame = endgameWeight >= 0.3 && endgameWeight < 0.7; // 30-70% endgame
            bool highEndgame = endgameWeight >= 0.7 && endgameWeight < 0.9;   // 70-90% endgame
            bool veryHighEndgame = endgameWeight >= 0.9;                      // 90%+ endgame

            // Pawn race evaluations (all endgame phases)
            evaluation += EvaluatePawnRaces(board) * (int)(endgameWeight * 100) / 100;

            // Rook + pawn tactics (medium to high endgame)
            if (endgameWeight >= 0.4)
                evaluation += EvaluateRookPawnTactics(board) * (int)(endgameWeight * 100) / 100;

            // Rook on 7th rank (medium endgame)
            if (mediumEndgame || highEndgame)
                evaluation += EvaluateRook7thRank(board);

            // King edge forcing (high endgame)
            if (highEndgame || veryHighEndgame)
                evaluation += EvaluateKingEdgeForcing(board);

            // Box closing and piece coordination (high to very high endgame)
            if (endgameWeight >= 0.7)
                evaluation += EvaluateBoxClosing(board) * (int)(endgameWeight * 100) / 100;

            // Piece centralization toward enemy king (very high endgame)
            if (veryHighEndgame)
                evaluation += EvaluatePieceCentralization(board);

            // Pawn promotion urgency (very high endgame)
            if (veryHighEndgame)
                evaluation += EvaluatePawnPromotionUrgency(board);

            return evaluation;
        }

        /// <summary>
        /// Evaluate pawn races using "the box" principle
        /// If pawn is outside the enemy king's box, it's safe to push
        /// If inside the box, the race will be lost and pawn should defend
        /// </summary>
        private static int EvaluatePawnRaces(Board board)
        {
            int evaluation = 0;

            var whitePawns = FindPawns(board, true);
            var blackPawns = FindPawns(board, false);
            var whiteKing = FindKing(board, true);
            var blackKing = FindKing(board, false);

            if (whiteKing.IsNull || blackKing.IsNull) return 0;

            // Evaluate white pawn races
            foreach (var pawn in whitePawns)
            {
                if (IsPawnPassed(board, pawn))
                {
                    evaluation += EvaluatePawnRace(pawn, blackKing, true);
                }
            }

            // Evaluate black pawn races
            foreach (var pawn in blackPawns)
            {
                if (IsPawnPassed(board, pawn))
                {
                    evaluation -= EvaluatePawnRace(pawn, whiteKing, false);
                }
            }

            return evaluation;
        }

        /// <summary>
        /// Evaluate a single pawn race using the box principle
        /// </summary>
        private static int EvaluatePawnRace(Piece pawn, Piece enemyKing, bool isWhitePawn)
        {
            int pawnFile = pawn.Square.Index % 8;
            int pawnRank = pawn.Square.Index / 8;
            int kingFile = enemyKing.Square.Index % 8;
            int kingRank = enemyKing.Square.Index / 8;

            // Calculate distance to promotion
            int promotionRank = isWhitePawn ? 7 : 0;
            int distanceToPromotion = Math.Abs(promotionRank - pawnRank);

            // Calculate "the box" - area the king can reach to stop the pawn
            int boxRadius = distanceToPromotion + 1; // King's reach to intercept

            // Check if king is outside the box
            int kingDistanceToPromotionSquare = Math.Max(
                Math.Abs(kingFile - pawnFile), 
                Math.Abs(kingRank - promotionRank)
            );

            if (kingDistanceToPromotionSquare > boxRadius)
            {
                // King is outside the box - safe to push!
                return PawnRaceBonus + (8 - distanceToPromotion) * 20; // Bonus increases as pawn advances
            }
            else
            {
                // King is inside the box - race will be lost, should defend
                return PawnRacePenalty;
            }
        }

        /// <summary>
        /// Evaluate rook + pawn promotion tactics
        /// Look for patterns where rook supports pawn promotion by controlling key squares
        /// </summary>
        private static int EvaluateRookPawnTactics(Board board)
        {
            int evaluation = 0;
            var whiteRooks = FindPieces(board, PieceType.Rook, true);
            var blackRooks = FindPieces(board, PieceType.Rook, false);
            var whitePawns = FindPawns(board, true);
            var blackPawns = FindPawns(board, false);

            // Evaluate white rook + pawn tactics
            foreach (var rook in whiteRooks)
            {
                foreach (var pawn in whitePawns)
                {
                    if (IsAdvancedPawn(pawn) && IsRookSupportingPawn(rook, pawn))
                    {
                        evaluation += RookPawnTacticsBonus;
                    }
                }
            }

            // Evaluate black rook + pawn tactics
            foreach (var rook in blackRooks)
            {
                foreach (var pawn in blackPawns)
                {
                    if (IsAdvancedPawn(pawn) && IsRookSupportingPawn(rook, pawn))
                    {
                        evaluation -= RookPawnTacticsBonus;
                    }
                }
            }

            return evaluation;
        }

        /// <summary>
        /// Check if rook is supporting pawn promotion (on same file or controlling promotion square)
        /// </summary>
        private static bool IsRookSupportingPawn(Piece rook, Piece pawn)
        {
            int rookFile = rook.Square.Index % 8;
            int pawnFile = pawn.Square.Index % 8;
            int pawnRank = pawn.Square.Index / 8;
            
            // Rook on same file as pawn
            if (rookFile == pawnFile) return true;

            // Rook controlling promotion square
            int promotionSquare = pawn.IsWhite ? 7 * 8 + pawnFile : pawnFile;
            int rookRank = rook.Square.Index / 8;
            
            // Rook on same rank as promotion square
            if ((pawn.IsWhite && rookRank == 7) || (!pawn.IsWhite && rookRank == 0))
                return true;

            return false;
        }

        /// <summary>
        /// Evaluate rooks on 7th rank during medium endgame
        /// </summary>
        private static int EvaluateRook7thRank(Board board)
        {
            int evaluation = 0;
            var whiteRooks = FindPieces(board, PieceType.Rook, true);
            var blackRooks = FindPieces(board, PieceType.Rook, false);

            // White rooks on 7th rank
            foreach (var rook in whiteRooks)
            {
                int rank = rook.Square.Index / 8;
                if (rank == 6) // 7th rank (0-indexed)
                {
                    evaluation += Rook7thRankBonus;
                }
            }

            // Black rooks on 2nd rank
            foreach (var rook in blackRooks)
            {
                int rank = rook.Square.Index / 8;
                if (rank == 1) // 2nd rank (0-indexed)
                {
                    evaluation -= Rook7thRankBonus;
                }
            }

            return evaluation;
        }

        /// <summary>
        /// Evaluate forcing enemy king to edges of the board
        /// </summary>
        private static int EvaluateKingEdgeForcing(Board board)
        {
            int evaluation = 0;
            var whiteKing = FindKing(board, true);
            var blackKing = FindKing(board, false);

            if (whiteKing.IsNull || blackKing.IsNull) return 0;

            // Evaluate black king's distance from edges (white wants to force black to edge)
            evaluation += EvaluateKingEdgeDistance(blackKing) * KingEdgeForceBonus / 20;

            // Evaluate white king's distance from edges (black wants to force white to edge)
            evaluation -= EvaluateKingEdgeDistance(whiteKing) * KingEdgeForceBonus / 20;

            return evaluation;
        }

        /// <summary>
        /// Calculate how close a king is to the edge of the board
        /// Lower values mean closer to edge (worse for the king)
        /// </summary>
        private static int EvaluateKingEdgeDistance(Piece king)
        {
            int file = king.Square.Index % 8;
            int rank = king.Square.Index / 8;

            // Distance from nearest edge
            int edgeDistance = Math.Min(Math.Min(file, 7 - file), Math.Min(rank, 7 - rank));
            
            // Convert to penalty (closer to edge = higher penalty)
            return 3 - edgeDistance; // 0 = center, 3 = edge
        }

        /// <summary>
        /// Evaluate "box closing" - reducing the number of squares available to enemy king
        /// </summary>
        private static int EvaluateBoxClosing(Board board)
        {
            int evaluation = 0;
            var whiteKing = FindKing(board, true);
            var blackKing = FindKing(board, false);

            if (whiteKing.IsNull || blackKing.IsNull) return 0;

            // Count squares available to each king
            int blackKingMobility = CountKingMobility(board, blackKing);
            int whiteKingMobility = CountKingMobility(board, whiteKing);

            // Lower mobility for enemy king is good
            evaluation += (8 - blackKingMobility) * BoxClosingBonus / 4;
            evaluation -= (8 - whiteKingMobility) * BoxClosingBonus / 4;

            return evaluation;
        }

        /// <summary>
        /// Count the number of squares a king can legally move to
        /// </summary>
        private static int CountKingMobility(Board board, Piece king)
        {
            int mobility = 0;
            int kingSquare = king.Square.Index;
            int file = kingSquare % 8;
            int rank = kingSquare / 8;

            // Check all 8 king directions
            int[] fileOffsets = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] rankOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int newFile = file + fileOffsets[i];
                int newRank = rank + rankOffsets[i];

                if (newFile >= 0 && newFile <= 7 && newRank >= 0 && newRank <= 7)
                {
                    int newSquare = newRank * 8 + newFile;
                    var pieceOnSquare = board.GetPiece(new Square(newSquare));
                    
                    // Square is available if empty or contains enemy piece
                    if (pieceOnSquare.IsNull || pieceOnSquare.IsWhite != king.IsWhite)
                    {
                        mobility++;
                    }
                }
            }

            return mobility;
        }

        /// <summary>
        /// Evaluate piece centralization toward enemy king in very late endgame
        /// </summary>
        private static int EvaluatePieceCentralization(Board board)
        {
            int evaluation = 0;
            var whiteKing = FindKing(board, true);
            var blackKing = FindKing(board, false);

            if (whiteKing.IsNull || blackKing.IsNull) return 0;

            // Evaluate white pieces attacking toward black king
            var whitePieces = FindAllPieces(board, true);
            foreach (var piece in whitePieces)
            {
                if (piece.PieceType != PieceType.King && piece.PieceType != PieceType.Pawn)
                {
                    int distanceToEnemyKing = CalculateDistance(piece, blackKing);
                    evaluation += (8 - distanceToEnemyKing) * PieceCentralizationBonus / 8;
                }
            }

            // Evaluate black pieces attacking toward white king
            var blackPieces = FindAllPieces(board, false);
            foreach (var piece in blackPieces)
            {
                if (piece.PieceType != PieceType.King && piece.PieceType != PieceType.Pawn)
                {
                    int distanceToEnemyKing = CalculateDistance(piece, whiteKing);
                    evaluation -= (8 - distanceToEnemyKing) * PieceCentralizationBonus / 8;
                }
            }

            return evaluation;
        }

        /// <summary>
        /// Evaluate pawn promotion urgency in very late endgame
        /// </summary>
        private static int EvaluatePawnPromotionUrgency(Board board)
        {
            int evaluation = 0;
            var whitePawns = FindPawns(board, true);
            var blackPawns = FindPawns(board, false);

            // White pawn promotion urgency
            foreach (var pawn in whitePawns)
            {
                int rank = pawn.Square.Index / 8;
                int promotionDistance = 7 - rank;
                if (promotionDistance <= 3) // On 5th rank or higher
                {
                    evaluation += PawnPromotionUrgency - (promotionDistance * 30);
                }
            }

            // Black pawn promotion urgency
            foreach (var pawn in blackPawns)
            {
                int rank = pawn.Square.Index / 8;
                int promotionDistance = rank;
                if (promotionDistance <= 3) // On 4th rank or lower
                {
                    evaluation -= PawnPromotionUrgency - (promotionDistance * 30);
                }
            }

            return evaluation;
        }

        // Helper methods

        private static List<Piece> FindPawns(Board board, bool isWhite)
        {
            return FindPieces(board, PieceType.Pawn, isWhite);
        }

        private static List<Piece> FindPieces(Board board, PieceType pieceType, bool isWhite)
        {
            var pieces = new List<Piece>();
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (!piece.IsNull && piece.IsWhite == isWhite && piece.PieceType == pieceType)
                {
                    pieces.Add(piece);
                }
            }
            return pieces;
        }

        private static List<Piece> FindAllPieces(Board board, bool isWhite)
        {
            var pieces = new List<Piece>();
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (!piece.IsNull && piece.IsWhite == isWhite)
                {
                    pieces.Add(piece);
                }
            }
            return pieces;
        }

        private static Piece FindKing(Board board, bool isWhite)
        {
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (!piece.IsNull && piece.IsWhite == isWhite && piece.PieceType == PieceType.King)
                {
                    return piece;
                }
            }
            return new Piece(); // Null piece
        }

        private static bool IsPawnPassed(Board board, Piece pawn)
        {
            int pawnFile = pawn.Square.Index % 8;
            int pawnRank = pawn.Square.Index / 8;
            bool isWhite = pawn.IsWhite;

            // Check files: pawn's file and adjacent files
            for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
            {
                int checkFile = pawnFile + fileOffset;
                if (checkFile < 0 || checkFile > 7) continue;

                // Check squares in front of the pawn
                int startRank = isWhite ? pawnRank + 1 : pawnRank - 1;
                int endRank = isWhite ? 8 : -1;
                int direction = isWhite ? 1 : -1;

                for (int rank = startRank; rank != endRank; rank += direction)
                {
                    if (rank < 0 || rank > 7) break;

                    var piece = board.GetPiece(new Square(rank * 8 + checkFile));
                    if (!piece.IsNull && piece.PieceType == PieceType.Pawn && piece.IsWhite != isWhite)
                    {
                        return false; // Found enemy pawn blocking
                    }
                }
            }

            return true; // No blocking pawns found
        }

        private static bool IsAdvancedPawn(Piece pawn)
        {
            int rank = pawn.Square.Index / 8;
            return pawn.IsWhite ? rank >= 4 : rank <= 3; // 5th rank+ for white, 4th rank- for black
        }

        private static int CalculateDistance(Piece piece1, Piece piece2)
        {
            int file1 = piece1.Square.Index % 8;
            int rank1 = piece1.Square.Index / 8;
            int file2 = piece2.Square.Index % 8;
            int rank2 = piece2.Square.Index / 8;

            return Math.Max(Math.Abs(file1 - file2), Math.Abs(rank1 - rank2));
        }
    }
}