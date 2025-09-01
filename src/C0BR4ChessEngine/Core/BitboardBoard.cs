using System;
using System.Collections.Generic;

namespace C0BR4ChessEngine.Core
{
    /// <summary>
    /// Bitboard-based chess position representation
    /// Much more efficient than piece array for move generation and evaluation
    /// </summary>
    public struct BitboardPosition
    {
        // Piece bitboards by color and type
        public ulong WhitePawns;
        public ulong WhiteKnights;
        public ulong WhiteBishops;
        public ulong WhiteRooks;
        public ulong WhiteQueens;
        public ulong WhiteKing;
        
        public ulong BlackPawns;
        public ulong BlackKnights;
        public ulong BlackBishops;
        public ulong BlackRooks;
        public ulong BlackQueens;
        public ulong BlackKing;
        
        // Composite bitboards for efficient operations
        public ulong AllWhitePieces;
        public ulong AllBlackPieces;
        public ulong AllPieces;

        // Game state
        public bool IsWhiteToMove;
        public int HalfMoveClock;
        public int FullMoveNumber;
        
        // Castling rights
        public bool WhiteCanCastleKingside;
        public bool WhiteCanCastleQueenside;
        public bool BlackCanCastleKingside;
        public bool BlackCanCastleQueenside;
        
        // En passant target square (-1 if none)
        public int EnPassantSquare;

        /// <summary>
        /// Update composite bitboards after any piece movement
        /// Must be called whenever individual piece bitboards are modified
        /// </summary>
        public void UpdateCompositeBitboards()
        {
            AllWhitePieces = WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKing;
            AllBlackPieces = BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKing;
            AllPieces = AllWhitePieces | AllBlackPieces;
        }

        /// <summary>
        /// Get the piece type and color at a specific square
        /// Returns (PieceType.None, true) if square is empty
        /// </summary>
        public (PieceType type, bool isWhite) GetPieceAt(int square)
        {
            ulong squareBit = Bitboard.SquareToBitboard(square);
            
            if ((AllWhitePieces & squareBit) != 0)
            {
                if ((WhitePawns & squareBit) != 0) return (PieceType.Pawn, true);
                if ((WhiteKnights & squareBit) != 0) return (PieceType.Knight, true);
                if ((WhiteBishops & squareBit) != 0) return (PieceType.Bishop, true);
                if ((WhiteRooks & squareBit) != 0) return (PieceType.Rook, true);
                if ((WhiteQueens & squareBit) != 0) return (PieceType.Queen, true);
                if ((WhiteKing & squareBit) != 0) return (PieceType.King, true);
            }
            else if ((AllBlackPieces & squareBit) != 0)
            {
                if ((BlackPawns & squareBit) != 0) return (PieceType.Pawn, false);
                if ((BlackKnights & squareBit) != 0) return (PieceType.Knight, false);
                if ((BlackBishops & squareBit) != 0) return (PieceType.Bishop, false);
                if ((BlackRooks & squareBit) != 0) return (PieceType.Rook, false);
                if ((BlackQueens & squareBit) != 0) return (PieceType.Queen, false);
                if ((BlackKing & squareBit) != 0) return (PieceType.King, false);
            }
            
            return (PieceType.None, true);
        }

        /// <summary>
        /// Place a piece on the board
        /// </summary>
        public void SetPiece(int square, PieceType pieceType, bool isWhite)
        {
            ulong squareBit = Bitboard.SquareToBitboard(square);
            
            // Remove any existing piece at this square first
            RemovePiece(square);
            
            // Add the new piece
            if (isWhite)
            {
                switch (pieceType)
                {
                    case PieceType.Pawn: WhitePawns |= squareBit; break;
                    case PieceType.Knight: WhiteKnights |= squareBit; break;
                    case PieceType.Bishop: WhiteBishops |= squareBit; break;
                    case PieceType.Rook: WhiteRooks |= squareBit; break;
                    case PieceType.Queen: WhiteQueens |= squareBit; break;
                    case PieceType.King: WhiteKing |= squareBit; break;
                }
            }
            else
            {
                switch (pieceType)
                {
                    case PieceType.Pawn: BlackPawns |= squareBit; break;
                    case PieceType.Knight: BlackKnights |= squareBit; break;
                    case PieceType.Bishop: BlackBishops |= squareBit; break;
                    case PieceType.Rook: BlackRooks |= squareBit; break;
                    case PieceType.Queen: BlackQueens |= squareBit; break;
                    case PieceType.King: BlackKing |= squareBit; break;
                }
            }
            
            UpdateCompositeBitboards();
        }

        /// <summary>
        /// Remove a piece from the board
        /// </summary>
        public void RemovePiece(int square)
        {
            ulong squareBit = ~Bitboard.SquareToBitboard(square);
            
            WhitePawns &= squareBit;
            WhiteKnights &= squareBit;
            WhiteBishops &= squareBit;
            WhiteRooks &= squareBit;
            WhiteQueens &= squareBit;
            WhiteKing &= squareBit;
            
            BlackPawns &= squareBit;
            BlackKnights &= squareBit;
            BlackBishops &= squareBit;
            BlackRooks &= squareBit;
            BlackQueens &= squareBit;
            BlackKing &= squareBit;
            
            UpdateCompositeBitboards();
        }

        /// <summary>
        /// Move a piece from one square to another
        /// </summary>
        public void MovePiece(int fromSquare, int toSquare)
        {
            var (pieceType, isWhite) = GetPieceAt(fromSquare);
            if (pieceType == PieceType.None) return;
            
            RemovePiece(fromSquare);
            SetPiece(toSquare, pieceType, isWhite);
        }

        /// <summary>
        /// Get all pieces of a specific type and color
        /// </summary>
        public ulong GetPieces(PieceType pieceType, bool isWhite)
        {
            return (pieceType, isWhite) switch
            {
                (PieceType.Pawn, true) => WhitePawns,
                (PieceType.Knight, true) => WhiteKnights,
                (PieceType.Bishop, true) => WhiteBishops,
                (PieceType.Rook, true) => WhiteRooks,
                (PieceType.Queen, true) => WhiteQueens,
                (PieceType.King, true) => WhiteKing,
                (PieceType.Pawn, false) => BlackPawns,
                (PieceType.Knight, false) => BlackKnights,
                (PieceType.Bishop, false) => BlackBishops,
                (PieceType.Rook, false) => BlackRooks,
                (PieceType.Queen, false) => BlackQueens,
                (PieceType.King, false) => BlackKing,
                _ => 0UL
            };
        }

        /// <summary>
        /// Get all pieces of a specific color
        /// </summary>
        public ulong GetAllPieces(bool isWhite)
        {
            return isWhite ? AllWhitePieces : AllBlackPieces;
        }

        /// <summary>
        /// Find the king square for a specific color
        /// </summary>
        public int GetKingSquare(bool isWhite)
        {
            ulong kingBitboard = isWhite ? WhiteKing : BlackKing;
            return kingBitboard != 0 ? Bitboard.LSB(kingBitboard) : -1;
        }

        /// <summary>
        /// Check if a square is attacked by the specified color
        /// </summary>
        public bool IsSquareAttacked(int square, bool byWhite)
        {
            // Safety check for invalid squares
            if (square < 0 || square >= 64)
            {
                return false;
            }
            
            ulong squareBit = Bitboard.SquareToBitboard(square);
            
            // Check pawn attacks
            ulong enemyPawns = GetPieces(PieceType.Pawn, byWhite);
            ulong pawnAttacks = byWhite ? Bitboard.WhitePawnAttacks(enemyPawns) : Bitboard.BlackPawnAttacks(enemyPawns);
            if ((pawnAttacks & squareBit) != 0) return true;
            
            // Check knight attacks
            ulong enemyKnights = GetPieces(PieceType.Knight, byWhite);
            while (enemyKnights != 0)
            {
                int knightSquare = Bitboard.PopLSB(ref enemyKnights);
                if ((Bitboard.KnightAttacks(knightSquare) & squareBit) != 0) return true;
            }
            
            // Check sliding piece attacks
            ulong enemyBishopsQueens = GetPieces(PieceType.Bishop, byWhite) | GetPieces(PieceType.Queen, byWhite);
            if ((MagicBitboards.GetBishopAttacks(square, AllPieces) & enemyBishopsQueens) != 0) return true;
            
            ulong enemyRooksQueens = GetPieces(PieceType.Rook, byWhite) | GetPieces(PieceType.Queen, byWhite);
            if ((MagicBitboards.GetRookAttacks(square, AllPieces) & enemyRooksQueens) != 0) return true;
            
            // Check king attacks
            int enemyKingSquare = GetKingSquare(byWhite);
            if (enemyKingSquare != -1 && (Bitboard.KingAttacks(enemyKingSquare) & squareBit) != 0) return true;
            
            return false;
        }

        /// <summary>
        /// Check if the current player is in check
        /// </summary>
        public bool IsInCheck()
        {
            int kingSquare = GetKingSquare(IsWhiteToMove);
            if (kingSquare == -1)
            {
                // King not found - this should never happen in a valid position
                // Return false to avoid crashes, but this indicates a serious bug
                return false;
            }
            return IsSquareAttacked(kingSquare, !IsWhiteToMove);
        }

        /// <summary>
        /// Load position from FEN string
        /// </summary>
        public static BitboardPosition FromFEN(string fen)
        {
            var position = new BitboardPosition();
            string[] parts = fen.Split(' ');
            
            if (parts.Length != 6)
                throw new ArgumentException("Invalid FEN string");

            // Clear all bitboards
            position.WhitePawns = position.WhiteKnights = position.WhiteBishops = 0UL;
            position.WhiteRooks = position.WhiteQueens = position.WhiteKing = 0UL;
            position.BlackPawns = position.BlackKnights = position.BlackBishops = 0UL;
            position.BlackRooks = position.BlackQueens = position.BlackKing = 0UL;

            // Parse piece placement
            string[] ranks = parts[0].Split('/');
            for (int rank = 0; rank < 8; rank++)
            {
                int file = 0;
                foreach (char c in ranks[7 - rank]) // FEN starts from rank 8
                {
                    if (char.IsDigit(c))
                    {
                        file += c - '0'; // Skip empty squares
                    }
                    else
                    {
                        bool isWhite = char.IsUpper(c);
                        PieceType pieceType = char.ToLower(c) switch
                        {
                            'p' => PieceType.Pawn,
                            'n' => PieceType.Knight,
                            'b' => PieceType.Bishop,
                            'r' => PieceType.Rook,
                            'q' => PieceType.Queen,
                            'k' => PieceType.King,
                            _ => PieceType.None
                        };
                        
                        if (pieceType != PieceType.None)
                        {
                            int square = Bitboard.GetSquare(file, rank);
                            position.SetPiece(square, pieceType, isWhite);
                        }
                        file++;
                    }
                }
            }

            // Parse active color
            position.IsWhiteToMove = parts[1] == "w";

            // Parse castling availability
            string castling = parts[2];
            position.WhiteCanCastleKingside = castling.Contains('K');
            position.WhiteCanCastleQueenside = castling.Contains('Q');
            position.BlackCanCastleKingside = castling.Contains('k');
            position.BlackCanCastleQueenside = castling.Contains('q');

            // Parse en passant target square
            position.EnPassantSquare = parts[3] == "-" ? -1 : Bitboard.GetSquare(
                parts[3][0] - 'a', parts[3][1] - '1');

            // Parse halfmove clock and fullmove number
            position.HalfMoveClock = int.Parse(parts[4]);
            position.FullMoveNumber = int.Parse(parts[5]);

            position.UpdateCompositeBitboards();
            return position;
        }

        /// <summary>
        /// Get starting position
        /// </summary>
        public static BitboardPosition StartingPosition()
        {
            return FromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        }

        /// <summary>
        /// Convert position to FEN string
        /// </summary>
        public string ToFEN()
        {
            var fen = new System.Text.StringBuilder();
            
            // 1. Piece placement
            for (int rank = 7; rank >= 0; rank--)
            {
                int emptySquares = 0;
                for (int file = 0; file < 8; file++)
                {
                    int square = Bitboard.GetSquare(file, rank);
                    var (pieceType, isWhite) = GetPieceAt(square);
                    
                    if (pieceType == PieceType.None)
                    {
                        emptySquares++;
                    }
                    else
                    {
                        if (emptySquares > 0)
                        {
                            fen.Append(emptySquares);
                            emptySquares = 0;
                        }
                        
                        char pieceChar = pieceType switch
                        {
                            PieceType.Pawn => 'p',
                            PieceType.Rook => 'r',
                            PieceType.Knight => 'n',
                            PieceType.Bishop => 'b',
                            PieceType.Queen => 'q',
                            PieceType.King => 'k',
                            _ => '?'
                        };
                        
                        if (isWhite)
                        {
                            pieceChar = char.ToUpper(pieceChar);
                        }
                        
                        fen.Append(pieceChar);
                    }
                }
                
                if (emptySquares > 0)
                {
                    fen.Append(emptySquares);
                }
                
                if (rank > 0)
                {
                    fen.Append('/');
                }
            }
            
            // 2. Active color
            fen.Append(' ');
            fen.Append(IsWhiteToMove ? 'w' : 'b');
            
            // 3. Castling availability
            fen.Append(' ');
            var castling = "";
            if (WhiteCanCastleKingside) castling += "K";
            if (WhiteCanCastleQueenside) castling += "Q";
            if (BlackCanCastleKingside) castling += "k";
            if (BlackCanCastleQueenside) castling += "q";
            fen.Append(castling == "" ? "-" : castling);
            
            // 4. En passant target square
            fen.Append(' ');
            if (EnPassantSquare == -1)
            {
                fen.Append('-');
            }
            else
            {
                char file = (char)('a' + Bitboard.GetFile(EnPassantSquare));
                int rank = Bitboard.GetRank(EnPassantSquare) + 1;
                fen.Append($"{file}{rank}");
            }
            
            // 5. Halfmove clock and fullmove number
            fen.Append($" {HalfMoveClock} {FullMoveNumber}");
            
            return fen.ToString();
        }
    }
}
