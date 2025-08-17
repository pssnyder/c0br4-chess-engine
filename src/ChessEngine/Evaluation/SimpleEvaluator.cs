namespace ChessEngine.Evaluation
{
    /// <summary>
    /// Position evaluation with material counting and piece-square tables
    /// Implements game phase detection and piece-specific positional bonuses
    /// </summary>
    public class SimpleEvaluator
    {
        // Material values in centipawns
        private static readonly int[] PieceValues = { 0, 100, 320, 330, 500, 900, 0 }; // None, Pawn, Knight, Bishop, Rook, Queen, King

        // Game phase thresholds (total material on board)
        private const int EndgameThreshold = 1400; // Roughly 14 pieces or equivalent material

        // Piece-Square Tables (from white's perspective, flip for black)
        // Values in centipawns, positive = good for the piece on that square

        #region Pawn PST
        // Middlegame: Safe advance on non-castled side focused
        private static readonly int[] PawnMiddlegame = {
             0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
             5,  5, 10, 25, 25, 10,  5,  5,
             0,  0,  0, 20, 20,  0,  0,  0,
             5, -5,-10,  0,  0,-10, -5,  5,
             5, 10, 10,-20,-20, 10, 10,  5,
             0,  0,  0,  0,  0,  0,  0,  0
        };

        // Endgame: Second rank and promotion focused
        private static readonly int[] PawnEndgame = {
             0,  0,  0,  0,  0,  0,  0,  0,
            80, 80, 80, 80, 80, 80, 80, 80,
            50, 50, 50, 50, 50, 50, 50, 50,
            30, 30, 30, 30, 30, 30, 30, 30,
            20, 20, 20, 20, 20, 20, 20, 20,
            10, 10, 10, 10, 10, 10, 10, 10,
            10, 10, 10, 10, 10, 10, 10, 10,
             0,  0,  0,  0,  0,  0,  0,  0
        };
        #endregion

        #region Knight PST
        // Middlegame: Center focused
        private static readonly int[] KnightMiddlegame = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        // Endgame: Check focused (near enemy king)
        private static readonly int[] KnightEndgame = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-20,-30,-30,-20,-40,-50
        };
        #endregion

        #region Bishop PST
        // Middlegame: Long diagonal focused
        private static readonly int[] BishopMiddlegame = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        // Endgame: Check focused
        private static readonly int[] BishopEndgame = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };
        #endregion

        #region Queen PST
        // Middlegame: Safe piece attack focused
        private static readonly int[] QueenMiddlegame = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        // Endgame: Safe check focused
        private static readonly int[] QueenEndgame = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };
        #endregion

        #region Rook PST
        // Middlegame: Rank and file alignment focused
        private static readonly int[] RookMiddlegame = {
              0,  0,  0,  0,  0,  0,  0,  0,
              5, 10, 10, 10, 10, 10, 10,  5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              0,  0,  0,  5,  5,  0,  0,  0
        };

        // Endgame: Second rank and check focused
        private static readonly int[] RookEndgame = {
              0,  0,  0,  0,  0,  0,  0,  0,
             35, 35, 35, 35, 35, 35, 35, 35,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              0,  0,  0,  5,  5,  0,  0,  0
        };
        #endregion

        #region King PST
        // Middlegame: Hiding focused, minimizing attack lanes
        private static readonly int[] KingMiddlegame = {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20
        };

        // Endgame: Stay close to opponent king
        private static readonly int[] KingEndgame = {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        };
        #endregion

        /// <summary>
        /// Evaluate the position from the perspective of the side to move
        /// Positive = good for side to move, Negative = bad for side to move
        /// </summary>
        public int Evaluate(Core.Board board)
        {
            int evaluation = 0;
            
            // Calculate total material to determine game phase
            int totalMaterial = CalculateTotalMaterial(board);
            bool isEndgame = totalMaterial <= EndgameThreshold;
            
            // Evaluate both sides
            int whiteEval = EvaluateSide(board, true, isEndgame);
            int blackEval = EvaluateSide(board, false, isEndgame);
            
            evaluation = whiteEval - blackEval;
            
            // Return from perspective of side to move
            return board.IsWhiteToMove ? evaluation : -evaluation;
        }

        private int CalculateTotalMaterial(Core.Board board)
        {
            int total = 0;
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Core.Square(square));
                if (!piece.IsNull)
                {
                    total += PieceValues[(int)piece.PieceType];
                }
            }
            return total;
        }

        private int EvaluateSide(Core.Board board, bool isWhite, bool isEndgame)
        {
            int evaluation = 0;
            
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Core.Square(square));
                if (piece.IsNull || piece.IsWhite != isWhite)
                    continue;
                
                // Material value
                evaluation += PieceValues[(int)piece.PieceType];
                
                // Piece-square table value
                evaluation += GetPSTValue(piece.PieceType, square, isWhite, isEndgame);
            }
            
            return evaluation;
        }

        private int GetPSTValue(Core.PieceType pieceType, int square, bool isWhite, bool isEndgame)
        {
            // Flip square for black pieces (they see the board from opposite perspective)
            int pstSquare = isWhite ? square : 63 - square;
            
            return pieceType switch
            {
                Core.PieceType.Pawn => isEndgame ? PawnEndgame[pstSquare] : PawnMiddlegame[pstSquare],
                Core.PieceType.Knight => isEndgame ? KnightEndgame[pstSquare] : KnightMiddlegame[pstSquare],
                Core.PieceType.Bishop => isEndgame ? BishopEndgame[pstSquare] : BishopMiddlegame[pstSquare],
                Core.PieceType.Rook => isEndgame ? RookEndgame[pstSquare] : RookMiddlegame[pstSquare],
                Core.PieceType.Queen => isEndgame ? QueenEndgame[pstSquare] : QueenMiddlegame[pstSquare],
                Core.PieceType.King => isEndgame ? KingEndgame[pstSquare] : KingMiddlegame[pstSquare],
                _ => 0
            };
        }
    }
}
