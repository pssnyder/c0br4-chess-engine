using C0BR4ChessEngine.Core;
using System.Text;

namespace C0BR4ChessEngine.Opening
{
    /// <summary>
    /// Minimal opening book with embedded move sequences for key openings
    /// Covers London System, Vienna Gambit, Caro-Kann, and Dutch Defense
    /// </summary>
    public static class OpeningBook
    {
        // Opening move sequences - each array represents a line of moves
        // Format: [move1, move2, move3, ...] in algebraic notation
        
        #region London System (White with d4)
        private static readonly string[][] LondonSystemLines = new string[][]
        {
            // Main London System lines - extended with castling and development
            new[] { "d4", "Nf3", "Bf4", "e3", "Bd3", "Nbd2", "c3", "O-O" },
            new[] { "d4", "Nf3", "Bf4", "e3", "c3", "Bd3", "Nbd2", "O-O" },
            new[] { "d4", "Bf4", "Nf3", "e3", "Bd3", "Nbd2", "O-O", "c3" },
            new[] { "d4", "Bf4", "e3", "Nf3", "Bd3", "c3", "Nbd2", "O-O" },
            
            // London vs d5 setup - solid development
            new[] { "d4", "Nf3", "Bf4", "e3", "Bd3", "Nbd2", "O-O", "c3", "Re1" },
            new[] { "d4", "Nf3", "Bf4", "e3", "Bd3", "h3", "Nbd2", "O-O", "c3" },
            
            // London vs Nf6 setup - space advantage
            new[] { "d4", "Nf3", "Bf4", "e3", "Bd3", "c3", "Nbd2", "O-O", "h3" },
            new[] { "d4", "Bf4", "Nf3", "e3", "Bd3", "h3", "Nbd2", "O-O", "c4" },
        };
        #endregion

        #region Vienna Gambit (White with e4)
        private static readonly string[][] ViennaGambitLines = new string[][]
        {
            // Main Vienna Gambit lines - aggressive but sound development
            new[] { "e4", "Nc3", "f4", "Nf3", "Bb5+", "Bd2", "Bxd2+", "Qxd2" },
            new[] { "e4", "Nc3", "Bc4", "f4", "Nf3", "d3", "O-O", "Qe2" },
            new[] { "e4", "Nc3", "f4", "Bc4", "Nf3", "d3", "O-O", "Bb3" },
            
            // Vienna Game proper (less aggressive) - positional control
            new[] { "e4", "Nc3", "Nf3", "Bc4", "d3", "O-O", "Bg5", "h3" },
            new[] { "e4", "Nc3", "Bc4", "Nf3", "d3", "O-O", "a3", "Ba2" },
            
            // Vienna vs King's Indian setup
            new[] { "e4", "Nc3", "Nf3", "g3", "Bg2", "O-O", "d3", "Rb1" },
            new[] { "e4", "Nc3", "g3", "Bg2", "Nge2", "O-O", "d3", "h3" },
        };
        #endregion

        #region Caro-Kann Defense (Black vs e4)
        private static readonly string[][] CaroKannLines = new string[][]
        {
            // Main line Caro-Kann - solid development for Black
            new[] { "c6", "d5", "Nc6", "Bg4", "Be2", "Bxe2", "Qxe2", "e6", "Nf3" },
            new[] { "c6", "d5", "Nc6", "e6", "Nf3", "Bd6", "Bd3", "Ne7", "O-O" },
            new[] { "c6", "d5", "Nc6", "dxe4", "Nxe4", "Nd7", "Nf3", "Ngf6", "Ng3" },
            
            // Advance variation response - counterplay
            new[] { "c6", "d5", "c5", "e6", "Nc6", "Nge7", "cxd4", "cxd4", "Nf5" },
            new[] { "c6", "d5", "c5", "Nc6", "cxd4", "cxd4", "Nf6", "Bd3", "e6" },
            
            // Exchange variation - active piece play
            new[] { "c6", "d5", "cxd5", "Nc6", "Nf3", "Bg4", "Be2", "e6", "O-O" },
            new[] { "c6", "d5", "cxd5", "Nf6", "Nc3", "cxd5", "Bg5", "e6", "Bxf6" },
            
            // Panov-Botvinnik Attack response
            new[] { "c6", "d5", "cxd5", "e6", "Nc3", "exd5", "Nf3", "Nc6", "Bg5" },
        };
        #endregion

        #region Dutch Defense (Black vs d4)
        private static readonly string[][] DutchDefenseLines = new string[][]
        {
            // Classical Dutch - solid kingside development
            new[] { "f5", "Nf6", "e6", "Be7", "O-O", "d6", "Qe8", "Qh5", "Nbd7" },
            new[] { "f5", "Nf6", "e6", "d6", "Be7", "O-O", "Nbd7", "Qe8", "c6" },
            
            // Leningrad Dutch - kingside fianchetto system
            new[] { "f5", "Nf6", "g6", "Bg7", "O-O", "d6", "Nc6", "Qe8", "e5" },
            new[] { "f5", "g6", "Bg7", "Nf6", "O-O", "d6", "c6", "Qe8", "Na6" },
            
            // Stonewall Dutch - central control
            new[] { "f5", "e6", "d5", "Bd6", "Nf6", "c6", "Nbd7", "O-O", "Qe7" },
            new[] { "f5", "e6", "Nf6", "d5", "Bd6", "c6", "O-O", "Nbd7", "Ne4" },
            
            // Classical Dutch with early ...c6
            new[] { "f5", "Nf6", "e6", "c6", "d6", "Be7", "O-O", "Nbd7", "Qe8" },
            new[] { "f5", "e6", "Nf6", "c6", "Be7", "d6", "O-O", "Nbd7", "Qe7" },
        };
        #endregion

        #region Position-based opening knowledge
        // Key position hashes mapped to preferred moves
        private static readonly Dictionary<string, string[]> PositionMoves = new()
        {
            // Starting position - choose e4 or d4 randomly
            ["rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"] = new[] { "e4", "d4" },
            
            // After 1.e4 - encourage main defenses
            ["rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"] = new[] { "e5", "c6", "c5", "e6" },
            
            // After 1.d4 - encourage solid responses
            ["rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR b KQkq d3 0 1"] = new[] { "d5", "Nf6", "f5", "e6" },
            
            // After 1.e4 e5 - go into Vienna
            ["rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2"] = new[] { "Nc3", "Nf3" },
            
            // After 1.e4 c6 - Caro-Kann continuation
            ["rnbqkbnr/pp1ppppp/2p5/8/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 2"] = new[] { "d4", "Nc3" },
            
            // After 1.d4 d5 - go into London System
            ["rnbqkbnr/ppp1pppp/8/3p4/3P4/8/PPP1PPPP/RNBQKBNR w KQkq d6 0 2"] = new[] { "Nf3", "Bf4" },
            
            // After 1.d4 f5 - Dutch Defense responses
            ["rnbqkbnr/ppppp1pp/8/5p2/3P4/8/PPP1PPPP/RNBQKBNR w KQkq f6 0 2"] = new[] { "Nf3", "c4", "g3" },
            
            // After 1.d4 Nf6 - solid development
            ["rnbqkb1r/pppppppp/5n2/8/3P4/8/PPP1PPPP/RNBQKBNR w KQkq - 1 2"] = new[] { "Nf3", "c4", "Bf4" },
            
            // Vienna Game continuations
            ["rnbqkb1r/pppp1ppp/5n2/4p3/4P3/2N5/PPPP1PPP/R1BQKBNR w KQkq - 2 3"] = new[] { "f4", "Bc4", "Nf3" },
            
            // London System development
            ["rnbqkb1r/ppp1pppp/5n2/3p4/3P1B2/5N2/PPP1PPPP/RN1QKBNR w KQkq - 2 3"] = new[] { "e3", "c3" },
        };
        #endregion

        private static readonly Random random = new Random();

        /// <summary>
        /// Try to get an opening move for the current position
        /// </summary>
        /// <param name="board">Current board position</param>
        /// <returns>Opening move in algebraic notation, or null if not in book</returns>
        public static string? GetOpeningMove(Board board)
        {
            // Extended opening book coverage to 12 moves (24 half-moves)
            int halfMoves = (board.FullMoveNumber - 1) * 2 + (board.IsWhiteToMove ? 0 : 1);
            if (halfMoves > 24) // 12 moves per side
                return null;

            // For now, use simple move-based logic since FEN generation isn't implemented
            return GetMoveByCount(board, halfMoves);
        }

        /// <summary>
        /// Get opening move based on move count (simplified approach)
        /// </summary>
        private static string? GetMoveByCount(Board board, int halfMoves)
        {
            return halfMoves switch
            {
                0 => board.IsWhiteToMove ? (random.Next(2) == 0 ? "e4" : "d4") : null, // First move for white
                1 => !board.IsWhiteToMove ? GetBlackResponse() : null, // First move for black
                2 => board.IsWhiteToMove ? GetWhiteSecondMove() : null, // Second move for white
                3 => !board.IsWhiteToMove ? GetBlackSecondMove() : null, // Second move for black
                4 => board.IsWhiteToMove ? GetWhiteThirdMove() : null, // Third move for white
                5 => !board.IsWhiteToMove ? GetBlackThirdMove() : null, // Third move for black
                6 => board.IsWhiteToMove ? GetWhiteFourthMove() : null, // Fourth move for white
                7 => !board.IsWhiteToMove ? GetBlackFourthMove() : null, // Fourth move for black
                8 => board.IsWhiteToMove ? GetWhiteFifthMove() : null, // Fifth move for white
                9 => !board.IsWhiteToMove ? GetBlackFifthMove() : null, // Fifth move for black
                10 => board.IsWhiteToMove ? GetWhiteSixthMove() : null, // Sixth move for white
                11 => !board.IsWhiteToMove ? GetBlackSixthMove() : null, // Sixth move for black
                _ => null // Fall back to engine after move 6
            };
        }

        /// <summary>
        /// Get Black's response to White's first move
        /// </summary>
        private static string GetBlackResponse()
        {
            // Simplified: respond to e4 with e5 or c6, to d4 with d5 or f5
            string[] responses = { "e5", "c6", "d5", "f5" };
            return responses[random.Next(responses.Length)];
        }

        /// <summary>
        /// Get White's second move
        /// </summary>
        private static string GetWhiteSecondMove()
        {
            // After e4: develop knight or bishop, after d4: develop pieces
            string[] moves = { "Nf3", "Nc3", "Bf4", "Bc4" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get Black's second move
        /// </summary>
        private static string GetBlackSecondMove()
        {
            // Develop pieces
            string[] moves = { "Nf6", "Nc6", "Be7", "Bg4" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get White's third move
        /// </summary>
        private static string GetWhiteThirdMove()
        {
            // Continue opening development
            string[] moves = { "Bd3", "Bb5+", "e3", "d3", "O-O", "Be2" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get Black's third move
        /// </summary>
        private static string GetBlackThirdMove()
        {
            // Continue development and castle preparation
            string[] moves = { "Be7", "Bd6", "O-O", "e6", "d6", "Bg4" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get White's fourth move
        /// </summary>
        private static string GetWhiteFourthMove()
        {
            // Castle and complete development
            string[] moves = { "O-O", "Nbd2", "c3", "h3", "Qe2", "Re1" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get Black's fourth move
        /// </summary>
        private static string GetBlackFourthMove()
        {
            // Castle and coordinate pieces
            string[] moves = { "O-O", "Nbd7", "c6", "Qe8", "Re8", "h6" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get White's fifth move
        /// </summary>
        private static string GetWhiteFifthMove()
        {
            // Central control and piece coordination
            string[] moves = { "Re1", "c4", "h3", "Qe2", "Bb3", "a3" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get Black's fifth move
        /// </summary>
        private static string GetBlackFifthMove()
        {
            // Active piece play
            string[] moves = { "Re8", "c5", "Qe7", "h6", "a6", "Rb8" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get White's sixth move
        /// </summary>
        private static string GetWhiteSixthMove()
        {
            // Transition to middlegame
            string[] moves = { "Qe2", "h3", "a3", "c4", "Rb1", "Bd2" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Get Black's sixth move
        /// </summary>
        private static string GetBlackSixthMove()
        {
            // Middlegame preparation
            string[] moves = { "Qe7", "a6", "c5", "h6", "Bd7", "Rb8" };
            return moves[random.Next(moves.Length)];
        }

        /// <summary>
        /// Extract the game history from the current position
        /// </summary>
        private static List<string> GetGameHistory(Board board)
        {
            // This is a simplified implementation
            // In a full implementation, we'd track the actual move history
            var history = new List<string>();
            
            // For now, we'll work with the limited position information we have
            // This is a placeholder - in practice, you'd maintain move history
            
            return history;
        }

        /// <summary>
        /// Find a book move based on the current game history
        /// </summary>
        private static string? FindBookMove(List<string> gameHistory)
        {
            // If no history, return starting moves
            if (gameHistory.Count == 0)
            {
                return random.Next(2) == 0 ? "e4" : "d4";
            }

            // Try to match against our opening lines
            var allLines = GetAllOpeningLines();
            
            foreach (var line in allLines)
            {
                if (MatchesHistory(line, gameHistory))
                {
                    // Return the next move in this line
                    if (gameHistory.Count < line.Length)
                    {
                        return line[gameHistory.Count];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get all opening lines combined
        /// </summary>
        private static string[][] GetAllOpeningLines()
        {
            var allLines = new List<string[]>();
            allLines.AddRange(LondonSystemLines);
            allLines.AddRange(ViennaGambitLines);
            allLines.AddRange(CaroKannLines);
            allLines.AddRange(DutchDefenseLines);
            return allLines.ToArray();
        }

        /// <summary>
        /// Check if a line matches the current game history
        /// </summary>
        private static bool MatchesHistory(string[] line, List<string> history)
        {
            if (history.Count > line.Length)
                return false;

            for (int i = 0; i < history.Count; i++)
            {
                if (!string.Equals(line[i], history[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if current position is likely still in opening phase
        /// </summary>
        public static bool IsInOpeningPhase(Board board)
        {
            // Extended opening phase to match deeper book coverage
            int halfMoves = (board.FullMoveNumber - 1) * 2 + (board.IsWhiteToMove ? 0 : 1);
            if (halfMoves > 24) // 12 moves per side (extended from 10)
                return false;

            // Count developed pieces (not on starting squares)
            int developedPieces = CountDevelopedPieces(board);
            
            // If too many pieces developed, we're past opening (increased threshold)
            return developedPieces < 10;
        }

        /// <summary>
        /// Count how many pieces have been developed from starting positions
        /// </summary>
        private static int CountDevelopedPieces(Board board)
        {
            int developed = 0;
            
            // Starting positions for pieces we care about
            int[] startingSquares = { 1, 2, 5, 6, 57, 58, 61, 62 }; // Knights and bishops
            
            foreach (int square in startingSquares)
            {
                var piece = board.GetPiece(new Square(square));
                
                // If square is empty or has different piece type, it's been developed
                if (piece.IsNull || !IsOnStartingSquare(piece, square))
                {
                    developed++;
                }
            }
            
            return developed;
        }

        /// <summary>
        /// Check if a piece is on its expected starting square
        /// </summary>
        private static bool IsOnStartingSquare(Piece piece, int square)
        {
            // Map expected piece types to starting squares
            return square switch
            {
                1 or 57 => piece.PieceType == PieceType.Knight,  // b1, b8
                2 or 58 => piece.PieceType == PieceType.Bishop,  // c1, c8  
                5 or 61 => piece.PieceType == PieceType.Bishop,  // f1, f8
                6 or 62 => piece.PieceType == PieceType.Knight,  // g1, g8
                _ => false
            };
        }

        /// <summary>
        /// Get statistics about the opening book
        /// </summary>
        public static string GetBookStats()
        {
            var stats = new StringBuilder();
            stats.AppendLine($"Opening Book Statistics:");
            stats.AppendLine($"London System lines: {LondonSystemLines.Length}");
            stats.AppendLine($"Vienna Gambit lines: {ViennaGambitLines.Length}");
            stats.AppendLine($"Caro-Kann lines: {CaroKannLines.Length}");
            stats.AppendLine($"Dutch Defense lines: {DutchDefenseLines.Length}");
            stats.AppendLine($"Position-specific moves: {PositionMoves.Count}");
            
            int totalMoves = LondonSystemLines.Sum(line => line.Length) +
                           ViennaGambitLines.Sum(line => line.Length) +
                           CaroKannLines.Sum(line => line.Length) +
                           DutchDefenseLines.Sum(line => line.Length);
            
            stats.AppendLine($"Total book moves: {totalMoves}");
            
            return stats.ToString();
        }
    }
}
