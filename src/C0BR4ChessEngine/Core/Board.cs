using System;
using System.Collections.Generic;

namespace C0BR4ChessEngine.Core
{
    /// <summary>
    /// Board state for undo operations
    /// </summary>
    public struct BoardState
    {
        public bool IsWhiteToMove;
        public int HalfMoveClock;
        public int FullMoveNumber;
        public bool WhiteCanCastleKingside;
        public bool WhiteCanCastleQueenside;
        public bool BlackCanCastleKingside;
        public bool BlackCanCastleQueenside;
        public int EnPassantSquare;
        public Piece CapturedPiece;
        public Move LastMove;
        public Piece[] PreviousBoardState; // Copy of board state
    }

    /// <summary>
    /// Simplified board representation for our chess engine
    /// This will be expanded as we add more functionality
    /// </summary>
    public class Board
    {
        private Piece[] squares = new Piece[64];
        private bool isWhiteToMove = true;
        private int fullMoveNumber = 1;
        private int halfMoveClock = 0;
        
        // Castling rights
        private bool whiteCanCastleKingside = true;
        private bool whiteCanCastleQueenside = true;
        private bool blackCanCastleKingside = true;
        private bool blackCanCastleQueenside = true;
        
        // En passant
        private int enPassantSquare = -1;

        // Move history for undo operations
        private Stack<BoardState> stateHistory = new();

        public bool IsWhiteToMove => isWhiteToMove;
        public int FullMoveNumber => fullMoveNumber;
        public int HalfMoveClock => halfMoveClock;

        public Board()
        {
            LoadStartPosition();
        }

        public Board(string fen)
        {
            LoadPosition(fen);
        }

        /// <summary>
        /// Load the standard starting position
        /// </summary>
        public void LoadStartPosition()
        {
            LoadPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        }

        /// <summary>
        /// Load a position from FEN notation
        /// </summary>
        public void LoadPosition(string fen)
        {
            string[] parts = fen.Split(' ');
            if (parts.Length != 6) 
                throw new ArgumentException("Invalid FEN string");

            // Clear board
            Array.Fill(squares, new Piece(PieceType.None, true, new Square(0)));

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
                        
                        Square square = new Square(file, rank);
                        squares[square.Index] = new Piece(pieceType, isWhite, square);
                        file++;
                    }
                }
            }

            // Parse active color
            isWhiteToMove = parts[1] == "w";

            // Parse castling availability
            string castling = parts[2];
            whiteCanCastleKingside = castling.Contains('K');
            whiteCanCastleQueenside = castling.Contains('Q');
            blackCanCastleKingside = castling.Contains('k');
            blackCanCastleQueenside = castling.Contains('q');

            // Parse en passant target square
            enPassantSquare = parts[3] == "-" ? -1 : BoardHelper.SquareIndexFromName(parts[3]);

            // Parse halfmove clock and fullmove number
            halfMoveClock = int.Parse(parts[4]);
            fullMoveNumber = int.Parse(parts[5]);
        }

        /// <summary>
        /// Get the piece on a square
        /// </summary>
        public Piece GetPiece(Square square)
        {
            return squares[square.Index];
        }

        /// <summary>
        /// Generate all legal moves for the current position
        /// </summary>
        public Move[] GetLegalMoves()
        {
            var moveGenerator = new MoveGenerator(this);
            return moveGenerator.GenerateLegalMoves();
        }

        /// <summary>
        /// Generate all pseudo-legal moves (may include moves that leave king in check)
        /// </summary>
        public Move[] GetPseudoLegalMoves()
        {
            var moveGenerator = new MoveGenerator(this);
            return moveGenerator.GeneratePseudoLegalMoves();
        }

        /// <summary>
        /// Make a move on the board
        /// </summary>
        public void MakeMove(Move move)
        {
            // Save current state for undo
            var state = new BoardState
            {
                IsWhiteToMove = isWhiteToMove,
                HalfMoveClock = halfMoveClock,
                FullMoveNumber = fullMoveNumber,
                WhiteCanCastleKingside = whiteCanCastleKingside,
                WhiteCanCastleQueenside = whiteCanCastleQueenside,
                BlackCanCastleKingside = blackCanCastleKingside,
                BlackCanCastleQueenside = blackCanCastleQueenside,
                EnPassantSquare = enPassantSquare,
                CapturedPiece = GetPiece(move.TargetSquare),
                LastMove = move,
                PreviousBoardState = (Piece[])squares.Clone() // Save board state
            };
            stateHistory.Push(state);

            // Clear en passant square (will be set again if this move creates one)
            enPassantSquare = -1;

            // Update halfmove clock (reset on pawn move or capture)
            if (move.MovePieceType == PieceType.Pawn || move.IsCapture)
            {
                halfMoveClock = 0;
            }
            else
            {
                halfMoveClock++;
            }

            // Handle special moves
            switch (move.Flag)
            {
                case MoveFlag.PawnTwoForward:
                    // Set en passant square
                    enPassantSquare = (move.StartSquare.Index + move.TargetSquare.Index) / 2;
                    break;

                case MoveFlag.EnPassant:
                    // Remove the captured pawn
                    int capturedPawnSquare = isWhiteToMove ? move.TargetSquare.Index - 8 : move.TargetSquare.Index + 8;
                    squares[capturedPawnSquare] = new Piece(PieceType.None, true, new Square(capturedPawnSquare));
                    break;

                case MoveFlag.Castling:
                    // Move the rook for castling
                    HandleCastlingRookMove(move);
                    break;
            }

            // Update castling rights
            UpdateCastlingRights(move);

            // Move the piece
            Piece movingPiece = GetPiece(move.StartSquare);
            squares[move.StartSquare.Index] = new Piece(PieceType.None, true, move.StartSquare);

            // Handle promotion
            if (move.IsPromotion)
            {
                squares[move.TargetSquare.Index] = new Piece(move.PromotionPieceType, movingPiece.IsWhite, move.TargetSquare);
            }
            else
            {
                squares[move.TargetSquare.Index] = new Piece(movingPiece.PieceType, movingPiece.IsWhite, move.TargetSquare);
            }

            // Switch turns
            isWhiteToMove = !isWhiteToMove;
            if (isWhiteToMove) // If it's white's turn again, increment full move number
            {
                fullMoveNumber++;
            }
        }

        /// <summary>
        /// Undo the last move
        /// </summary>
        public void UnmakeMove()
        {
            if (stateHistory.Count == 0)
                throw new InvalidOperationException("No moves to undo");

            var state = stateHistory.Pop();

            // Restore complete board state
            isWhiteToMove = state.IsWhiteToMove;
            halfMoveClock = state.HalfMoveClock;
            fullMoveNumber = state.FullMoveNumber;
            whiteCanCastleKingside = state.WhiteCanCastleKingside;
            whiteCanCastleQueenside = state.WhiteCanCastleQueenside;
            blackCanCastleKingside = state.BlackCanCastleKingside;
            blackCanCastleQueenside = state.BlackCanCastleQueenside;
            enPassantSquare = state.EnPassantSquare;
            squares = state.PreviousBoardState; // Restore board state
        }

        private void HandleCastlingRookMove(Move move)
        {
            // TODO: Implement castling rook movement
            // This requires determining which side is castling and moving the rook accordingly
        }

        private void UpdateCastlingRights(Move move)
        {
            // If king moves, lose both castling rights
            if (move.MovePieceType == PieceType.King)
            {
                if (isWhiteToMove)
                {
                    whiteCanCastleKingside = false;
                    whiteCanCastleQueenside = false;
                }
                else
                {
                    blackCanCastleKingside = false;
                    blackCanCastleQueenside = false;
                }
            }

            // If rook moves from starting square, lose that side's castling rights
            if (move.MovePieceType == PieceType.Rook)
            {
                if (isWhiteToMove)
                {
                    if (move.StartSquare.Index == 0) // a1
                        whiteCanCastleQueenside = false;
                    else if (move.StartSquare.Index == 7) // h1
                        whiteCanCastleKingside = false;
                }
                else
                {
                    if (move.StartSquare.Index == 56) // a8
                        blackCanCastleQueenside = false;
                    else if (move.StartSquare.Index == 63) // h8
                        blackCanCastleKingside = false;
                }
            }

            // If rook is captured, lose that side's castling rights
            if (move.CapturePieceType == PieceType.Rook)
            {
                if (move.TargetSquare.Index == 0) // a1
                    whiteCanCastleQueenside = false;
                else if (move.TargetSquare.Index == 7) // h1
                    whiteCanCastleKingside = false;
                else if (move.TargetSquare.Index == 56) // a8
                    blackCanCastleQueenside = false;
                else if (move.TargetSquare.Index == 63) // h8
                    blackCanCastleKingside = false;
            }
        }

        /// <summary>
        /// Check if the current player is in check
        /// </summary>
        public bool IsInCheck()
        {
            var moveGenerator = new MoveGenerator(this);
            return moveGenerator.IsCurrentPlayerInCheck();
        }

        /// <summary>
        /// Check if the position is checkmate
        /// </summary>
        public bool IsCheckmate()
        {
            return IsInCheck() && GetLegalMoves().Length == 0;
        }

        /// <summary>
        /// Check if the position is stalemate
        /// </summary>
        public bool IsStalemate()
        {
            return !IsInCheck() && GetLegalMoves().Length == 0;
        }

        /// <summary>
        /// Check if the game is drawn
        /// </summary>
        public bool IsDraw()
        {
            return IsStalemate() || halfMoveClock >= 100; // 50-move rule
        }

        /// <summary>
        /// Get the current position as FEN
        /// </summary>
        public string GetFEN()
        {
            var fen = new System.Text.StringBuilder();
            
            // 1. Piece placement
            for (int rank = 7; rank >= 0; rank--)
            {
                int emptySquares = 0;
                for (int file = 0; file < 8; file++)
                {
                    int squareIndex = rank * 8 + file;
                    var piece = GetPiece(new Square(squareIndex));
                    
                    if (piece.IsNull)
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
                        
                        char pieceChar = piece.PieceType switch
                        {
                            PieceType.Pawn => 'p',
                            PieceType.Rook => 'r',
                            PieceType.Knight => 'n',
                            PieceType.Bishop => 'b',
                            PieceType.Queen => 'q',
                            PieceType.King => 'k',
                            _ => '?'
                        };
                        
                        if (piece.IsWhite)
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
            
            // 3. Castling availability (simplified - just use stored flags)
            fen.Append(' ');
            var castling = "";
            if (whiteCanCastleKingside) castling += "K";
            if (whiteCanCastleQueenside) castling += "Q";
            if (blackCanCastleKingside) castling += "k";
            if (blackCanCastleQueenside) castling += "q";
            fen.Append(castling == "" ? "-" : castling);
            
            // 4. En passant target square
            fen.Append(' ');
            if (enPassantSquare == -1)
            {
                fen.Append('-');
            }
            else
            {
                char file = (char)('a' + (enPassantSquare % 8));
                int rank = (enPassantSquare / 8) + 1;
                fen.Append($"{file}{rank}");
            }
            
            // 5. Halfmove clock and fullmove number (simplified)
            fen.Append(" 0 1");
            
            return fen.ToString();
        }
    }
}
