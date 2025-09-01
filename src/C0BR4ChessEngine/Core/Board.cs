using System;
using System.Collections.Generic;

namespace C0BR4ChessEngine.Core
{
    /// <summary>
    /// Board state for undo operations - updated for bitboard compatibility
    /// </summary>
    public struct BoardState
    {
        public BitboardPosition Position;
        public Move LastMove;
        public Piece CapturedPiece;
    }

    /// <summary>
    /// Board representation that internally uses bitboards for efficiency and accuracy
    /// Maintains compatibility with existing UCI interface while using fast bitboard operations
    /// </summary>
    public class Board
    {
        // Internal bitboard representation
        private BitboardPosition position;
        private BitboardMoveGenerator moveGenerator;
        
        // Move history for undo operations
        private Stack<BoardState> stateHistory = new();

        public bool IsWhiteToMove => position.IsWhiteToMove;
        public int FullMoveNumber => position.FullMoveNumber;
        public int HalfMoveClock => position.HalfMoveClock;

        public Board()
        {
            moveGenerator = new BitboardMoveGenerator();
            LoadStartPosition();
        }

        public Board(string fen)
        {
            moveGenerator = new BitboardMoveGenerator();
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
            position = BitboardPosition.FromFEN(fen);
            stateHistory.Clear();
        }

        /// <summary>
        /// Get the piece on a square (for backward compatibility)
        /// </summary>
        public Piece GetPiece(Square square)
        {
            var (pieceType, isWhite) = position.GetPieceAt(square.Index);
            return new Piece(pieceType, isWhite, square);
        }

        /// <summary>
        /// Generate all legal moves for the current position
        /// Now uses the efficient bitboard move generator
        /// </summary>
        public Move[] GetLegalMoves()
        {
            return moveGenerator.GenerateLegalMoves(position);
        }

        /// <summary>
        /// Generate all pseudo-legal moves (may include moves that leave king in check)
        /// </summary>
        public Move[] GetPseudoLegalMoves()
        {
            return moveGenerator.GeneratePseudoLegalMoves(position);
        }

        /// <summary>
        /// Make a move on the board
        /// </summary>
        public void MakeMove(Move move)
        {
            // Save current state for undo
            var state = new BoardState
            {
                Position = position,
                LastMove = move,
                CapturedPiece = GetPiece(move.TargetSquare)
            };
            stateHistory.Push(state);

            // Apply the move using bitboard operations
            ApplyMove(ref position, move);
        }

        /// <summary>
        /// Undo the last move
        /// </summary>
        public void UnmakeMove()
        {
            if (stateHistory.Count == 0)
                throw new InvalidOperationException("No moves to undo");

            var state = stateHistory.Pop();
            position = state.Position;
        }

        /// <summary>
        /// Apply a move to the bitboard position
        /// </summary>
        private void ApplyMove(ref BitboardPosition pos, Move move)
        {
            int fromSquare = move.StartSquare.Index;
            int toSquare = move.TargetSquare.Index;
            
            // Update halfmove clock
            if (move.MovePieceType == PieceType.Pawn || move.IsCapture)
            {
                pos.HalfMoveClock = 0;
            }
            else
            {
                pos.HalfMoveClock++;
            }

            // Update en passant
            pos.EnPassantSquare = -1; // Clear previous en passant

            // Handle special moves
            switch (move.Flag)
            {
                case MoveFlag.PawnTwoForward:
                    // Set en passant square
                    pos.EnPassantSquare = pos.IsWhiteToMove ? toSquare - 8 : toSquare + 8;
                    break;

                case MoveFlag.EnPassant:
                    // Remove the captured pawn
                    int capturedPawnSquare = pos.IsWhiteToMove ? toSquare - 8 : toSquare + 8;
                    pos.RemovePiece(capturedPawnSquare);
                    break;

                case MoveFlag.Castling:
                    // Move the rook for castling
                    HandleCastlingRookMove(ref pos, move);
                    break;
            }

            // Update castling rights
            UpdateCastlingRights(ref pos, move);

            // Move the piece
            pos.MovePiece(fromSquare, toSquare);

            // Handle promotion
            if (move.IsPromotion)
            {
                pos.RemovePiece(toSquare);
                pos.SetPiece(toSquare, move.PromotionPieceType, pos.IsWhiteToMove);
            }

            // Switch turns
            pos.IsWhiteToMove = !pos.IsWhiteToMove;
            
            // Update full move number
            if (pos.IsWhiteToMove) // If it's white's turn again, increment full move number
            {
                pos.FullMoveNumber++;
            }
        }

        private void HandleCastlingRookMove(ref BitboardPosition pos, Move move)
        {
            int toSquare = move.TargetSquare.Index;
            
            // Move the rook based on castling type
            if (toSquare == 6) // White kingside castling (e1-g1)
            {
                pos.MovePiece(7, 5); // h1 to f1
            }
            else if (toSquare == 2) // White queenside castling (e1-c1)
            {
                pos.MovePiece(0, 3); // a1 to d1
            }
            else if (toSquare == 62) // Black kingside castling (e8-g8)
            {
                pos.MovePiece(63, 61); // h8 to f8
            }
            else if (toSquare == 58) // Black queenside castling (e8-c8)
            {
                pos.MovePiece(56, 59); // a8 to d8
            }
        }

        private void UpdateCastlingRights(ref BitboardPosition pos, Move move)
        {
            // If king moves, lose both castling rights
            if (move.MovePieceType == PieceType.King)
            {
                if (pos.IsWhiteToMove)
                {
                    pos.WhiteCanCastleKingside = false;
                    pos.WhiteCanCastleQueenside = false;
                }
                else
                {
                    pos.BlackCanCastleKingside = false;
                    pos.BlackCanCastleQueenside = false;
                }
            }

            // If rook moves from starting square, lose that side's castling rights
            if (move.MovePieceType == PieceType.Rook)
            {
                if (pos.IsWhiteToMove)
                {
                    if (move.StartSquare.Index == 0) // a1
                        pos.WhiteCanCastleQueenside = false;
                    else if (move.StartSquare.Index == 7) // h1
                        pos.WhiteCanCastleKingside = false;
                }
                else
                {
                    if (move.StartSquare.Index == 56) // a8
                        pos.BlackCanCastleQueenside = false;
                    else if (move.StartSquare.Index == 63) // h8
                        pos.BlackCanCastleKingside = false;
                }
            }

            // If rook is captured, lose that side's castling rights
            if (move.CapturePieceType == PieceType.Rook)
            {
                if (move.TargetSquare.Index == 0) // a1
                    pos.WhiteCanCastleQueenside = false;
                else if (move.TargetSquare.Index == 7) // h1
                    pos.WhiteCanCastleKingside = false;
                else if (move.TargetSquare.Index == 56) // a8
                    pos.BlackCanCastleQueenside = false;
                else if (move.TargetSquare.Index == 63) // h8
                    pos.BlackCanCastleKingside = false;
            }
        }

        /// <summary>
        /// Check if the current player is in check
        /// </summary>
        public bool IsInCheck()
        {
            return position.IsInCheck();
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
            return IsStalemate() || position.HalfMoveClock >= 100; // 50-move rule
        }

        /// <summary>
        /// Get the current position as FEN
        /// </summary>
        public string GetFEN()
        {
            return position.ToFEN();
        }

        /// <summary>
        /// Get the internal bitboard position (for advanced operations)
        /// </summary>
        public BitboardPosition GetBitboardPosition()
        {
            return position;
        }

        /// <summary>
        /// Validate that a move is legal before applying it
        /// This prevents the rule infractions that occurred in v2.0
        /// </summary>
        public bool IsLegalMove(Move move)
        {
            return moveGenerator.IsLegalMove(position, move);
        }

        /// <summary>
        /// Find a legal move from a UCI move string (e.g., "e2e4")
        /// Returns null if the move is not legal
        /// </summary>
        public Move? FindLegalMove(string moveString)
        {
            var legalMoves = GetLegalMoves();
            
            foreach (var move in legalMoves)
            {
                if (move.ToString() == moveString)
                {
                    return move;
                }
            }
            
            return null; // Move not found or not legal
        }
    }
}
