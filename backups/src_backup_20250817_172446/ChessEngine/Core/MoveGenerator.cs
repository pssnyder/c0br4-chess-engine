using System;
using System.Collections.Generic;

namespace ChessEngine.Core
{
    /// <summary>
    /// Handles move generation for all piece types
    /// Starting with simple, readable implementation
    /// </summary>
    public class MoveGenerator
    {
        private readonly Board board;
        private readonly List<Move> moves = new();

        // Direction vectors for piece movement
        private static readonly int[] KnightMoves = { -17, -15, -10, -6, 6, 10, 15, 17 };
        private static readonly int[] KingMoves = { -9, -8, -7, -1, 1, 7, 8, 9 };
        private static readonly int[] RookDirections = { -8, -1, 1, 8 }; // N, W, E, S
        private static readonly int[] BishopDirections = { -9, -7, 7, 9 }; // NW, NE, SW, SE
        private static readonly int[] QueenDirections = { -9, -8, -7, -1, 1, 7, 8, 9 }; // All 8 directions

        public MoveGenerator(Board board)
        {
            this.board = board;
        }

        /// <summary>
        /// Generate all pseudo-legal moves for the current player
        /// (May include moves that leave king in check - will filter later)
        /// </summary>
        public Move[] GeneratePseudoLegalMoves()
        {
            moves.Clear();

            for (int square = 0; square < 64; square++)
            {
                Piece piece = board.GetPiece(new Square(square));
                
                if (piece.IsNull || piece.IsWhite != board.IsWhiteToMove)
                    continue;

                GenerateMovesForPiece(piece);
            }

            return moves.ToArray();
        }

        /// <summary>
        /// Generate all legal moves (pseudo-legal moves that don't leave king in check)
        /// </summary>
        public Move[] GenerateLegalMoves()
        {
            Move[] pseudoLegalMoves = GeneratePseudoLegalMoves();
            List<Move> legalMoves = new();

            foreach (Move move in pseudoLegalMoves)
            {
                if (IsLegalMove(move))
                {
                    legalMoves.Add(move);
                }
            }

            return legalMoves.ToArray();
        }

        private void GenerateMovesForPiece(Piece piece)
        {
            switch (piece.PieceType)
            {
                case PieceType.Pawn:
                    GeneratePawnMoves(piece);
                    break;
                case PieceType.Knight:
                    GenerateKnightMoves(piece);
                    break;
                case PieceType.Bishop:
                    GenerateBishopMoves(piece);
                    break;
                case PieceType.Rook:
                    GenerateRookMoves(piece);
                    break;
                case PieceType.Queen:
                    GenerateQueenMoves(piece);
                    break;
                case PieceType.King:
                    GenerateKingMoves(piece);
                    break;
            }
        }

        private void GeneratePawnMoves(Piece pawn)
        {
            int square = pawn.Square.Index;
            int file = square % 8;
            int rank = square / 8;
            bool isWhite = pawn.IsWhite;
            int direction = isWhite ? 8 : -8; // White moves up, black moves down
            int startRank = isWhite ? 1 : 6;
            int promotionRank = isWhite ? 7 : 0;

            // Forward moves
            int oneSquareForward = square + direction;
            if (IsValidSquare(oneSquareForward) && board.GetPiece(new Square(oneSquareForward)).IsNull)
            {
                if (rank + (isWhite ? 1 : -1) == promotionRank)
                {
                    // Promotion moves
                    AddPromotionMoves(pawn.Square, new Square(oneSquareForward));
                }
                else
                {
                    AddMove(pawn.Square, new Square(oneSquareForward), PieceType.Pawn);
                }

                // Two squares forward from starting position
                if (rank == startRank)
                {
                    int twoSquaresForward = square + 2 * direction;
                    if (IsValidSquare(twoSquaresForward) && board.GetPiece(new Square(twoSquaresForward)).IsNull)
                    {
                        AddMove(pawn.Square, new Square(twoSquaresForward), PieceType.Pawn, flag: MoveFlag.PawnTwoForward);
                    }
                }
            }

            // Captures
            int[] captureOffsets = isWhite ? new[] { 7, 9 } : new[] { -9, -7 };
            foreach (int offset in captureOffsets)
            {
                int captureSquare = square + offset;
                if (IsValidSquare(captureSquare))
                {
                    // Check if we're not wrapping around the board
                    int captureFile = captureSquare % 8;
                    if (Math.Abs(captureFile - file) == 1)
                    {
                        Piece targetPiece = board.GetPiece(new Square(captureSquare));
                        if (!targetPiece.IsNull && targetPiece.IsWhite != isWhite)
                        {
                            if (rank + (isWhite ? 1 : -1) == promotionRank)
                            {
                                // Promotion captures
                                AddPromotionMoves(pawn.Square, new Square(captureSquare), targetPiece.PieceType);
                            }
                            else
                            {
                                AddMove(pawn.Square, new Square(captureSquare), PieceType.Pawn, targetPiece.PieceType);
                            }
                        }
                    }
                }
            }

            // TODO: En passant captures
        }

        private void GenerateKnightMoves(Piece knight)
        {
            int square = knight.Square.Index;
            int file = square % 8;
            int rank = square / 8;

            foreach (int offset in KnightMoves)
            {
                int targetSquare = square + offset;
                if (IsValidSquare(targetSquare))
                {
                    int targetFile = targetSquare % 8;
                    int targetRank = targetSquare / 8;
                    
                    // Check if knight move is valid (not wrapping around board)
                    int fileDiff = Math.Abs(targetFile - file);
                    int rankDiff = Math.Abs(targetRank - rank);
                    
                    if ((fileDiff == 2 && rankDiff == 1) || (fileDiff == 1 && rankDiff == 2))
                    {
                        Piece targetPiece = board.GetPiece(new Square(targetSquare));
                        if (targetPiece.IsNull || targetPiece.IsWhite != knight.IsWhite)
                        {
                            AddMove(knight.Square, new Square(targetSquare), PieceType.Knight, 
                                   targetPiece.IsNull ? PieceType.None : targetPiece.PieceType);
                        }
                    }
                }
            }
        }

        private void GenerateBishopMoves(Piece bishop)
        {
            GenerateSlidingMoves(bishop, BishopDirections);
        }

        private void GenerateRookMoves(Piece rook)
        {
            GenerateSlidingMoves(rook, RookDirections);
        }

        private void GenerateQueenMoves(Piece queen)
        {
            GenerateSlidingMoves(queen, QueenDirections);
        }

        private void GenerateSlidingMoves(Piece piece, int[] directions)
        {
            int square = piece.Square.Index;

            foreach (int direction in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    int targetSquare = square + direction * i;
                    
                    if (!IsValidSquare(targetSquare))
                        break;

                    // Check for board edge wrapping
                    int startFile = square % 8;
                    int targetFile = targetSquare % 8;
                    if (Math.Abs(direction) != 8 && Math.Abs(targetFile - startFile) > i)
                        break;

                    Piece targetPiece = board.GetPiece(new Square(targetSquare));
                    
                    if (targetPiece.IsNull)
                    {
                        AddMove(piece.Square, new Square(targetSquare), piece.PieceType);
                    }
                    else
                    {
                        if (targetPiece.IsWhite != piece.IsWhite)
                        {
                            AddMove(piece.Square, new Square(targetSquare), piece.PieceType, targetPiece.PieceType);
                        }
                        break; // Can't move past any piece
                    }
                }
            }
        }

        private void GenerateKingMoves(Piece king)
        {
            int square = king.Square.Index;
            int file = square % 8;

            foreach (int offset in KingMoves)
            {
                int targetSquare = square + offset;
                if (IsValidSquare(targetSquare))
                {
                    int targetFile = targetSquare % 8;
                    
                    // Check for board edge wrapping
                    if (Math.Abs(targetFile - file) <= 1)
                    {
                        Piece targetPiece = board.GetPiece(new Square(targetSquare));
                        if (targetPiece.IsNull || targetPiece.IsWhite != king.IsWhite)
                        {
                            AddMove(king.Square, new Square(targetSquare), PieceType.King,
                                   targetPiece.IsNull ? PieceType.None : targetPiece.PieceType);
                        }
                    }
                }
            }

            // TODO: Castling moves
        }

        private void AddMove(Square from, Square to, PieceType movePiece, PieceType capturePiece = PieceType.None, MoveFlag flag = MoveFlag.None)
        {
            moves.Add(new Move(from, to, movePiece, capturePiece, PieceType.None, flag));
        }

        private void AddPromotionMoves(Square from, Square to, PieceType capturePiece = PieceType.None)
        {
            PieceType[] promotionPieces = { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };
            foreach (PieceType promotionPiece in promotionPieces)
            {
                moves.Add(new Move(from, to, PieceType.Pawn, capturePiece, promotionPiece));
            }
        }

        private bool IsValidSquare(int square)
        {
            return square >= 0 && square < 64;
        }

        private bool IsLegalMove(Move move)
        {
            // Make the move temporarily
            board.MakeMove(move);
            
            // Check if the king is in check (which means the move was illegal)
            bool isInCheck = IsKingInCheck(!board.IsWhiteToMove); // Check the king of the player who just moved
            
            // Undo the move
            board.UnmakeMove();
            
            return !isInCheck;
        }

        /// <summary>
        /// Check if the current player is in check
        /// </summary>
        public bool IsCurrentPlayerInCheck()
        {
            return IsKingInCheck(board.IsWhiteToMove);
        }

        /// <summary>
        /// Check if the specified player's king is in check
        /// </summary>
        private bool IsKingInCheck(bool isWhiteKing)
        {
            // Find the king
            Square kingSquare = FindKing(isWhiteKing);
            if (kingSquare.Index == -1) return false; // No king found (shouldn't happen)

            // Check if any enemy piece can attack the king
            return IsSquareAttackedBy(kingSquare, !isWhiteKing);
        }

        /// <summary>
        /// Find the king of the specified color
        /// </summary>
        private Square FindKing(bool isWhite)
        {
            for (int square = 0; square < 64; square++)
            {
                Piece piece = board.GetPiece(new Square(square));
                if (piece.PieceType == PieceType.King && piece.IsWhite == isWhite)
                {
                    return new Square(square);
                }
            }
            return new Square(-1); // King not found (shouldn't happen)
        }

        /// <summary>
        /// Check if a square is attacked by the specified color
        /// </summary>
        private bool IsSquareAttackedBy(Square square, bool byWhite)
        {
            // Check pawn attacks
            if (IsPawnAttackingSquare(square, byWhite)) return true;
            
            // Check knight attacks
            if (IsKnightAttackingSquare(square, byWhite)) return true;
            
            // Check sliding piece attacks (bishop, rook, queen)
            if (IsSlidingPieceAttackingSquare(square, byWhite)) return true;
            
            // Check king attacks
            if (IsKingAttackingSquare(square, byWhite)) return true;
            
            return false;
        }

        private bool IsPawnAttackingSquare(Square square, bool byWhite)
        {
            int[] pawnAttackOffsets = byWhite ? new[] { -7, -9 } : new[] { 7, 9 };
            int file = square.Index % 8;
            
            foreach (int offset in pawnAttackOffsets)
            {
                int attackerSquare = square.Index + offset;
                if (IsValidSquare(attackerSquare))
                {
                    int attackerFile = attackerSquare % 8;
                    if (Math.Abs(attackerFile - file) == 1) // Valid pawn attack direction
                    {
                        Piece piece = board.GetPiece(new Square(attackerSquare));
                        if (piece.PieceType == PieceType.Pawn && piece.IsWhite == byWhite)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsKnightAttackingSquare(Square square, bool byWhite)
        {
            int squareIndex = square.Index;
            int file = squareIndex % 8;
            int rank = squareIndex / 8;

            foreach (int offset in KnightMoves)
            {
                int attackerSquare = squareIndex + offset;
                if (IsValidSquare(attackerSquare))
                {
                    int attackerFile = attackerSquare % 8;
                    int attackerRank = attackerSquare / 8;
                    
                    int fileDiff = Math.Abs(attackerFile - file);
                    int rankDiff = Math.Abs(attackerRank - rank);
                    
                    if ((fileDiff == 2 && rankDiff == 1) || (fileDiff == 1 && rankDiff == 2))
                    {
                        Piece piece = board.GetPiece(new Square(attackerSquare));
                        if (piece.PieceType == PieceType.Knight && piece.IsWhite == byWhite)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsSlidingPieceAttackingSquare(Square square, bool byWhite)
        {
            // Check all 8 directions
            foreach (int direction in QueenDirections)
            {
                for (int i = 1; i < 8; i++)
                {
                    int attackerSquare = square.Index + direction * i;
                    
                    if (!IsValidSquare(attackerSquare)) break;

                    // Check for board edge wrapping
                    int startFile = square.Index % 8;
                    int attackerFile = attackerSquare % 8;
                    if (Math.Abs(direction) != 8 && Math.Abs(attackerFile - startFile) > i)
                        break;

                    Piece piece = board.GetPiece(new Square(attackerSquare));
                    
                    if (!piece.IsNull)
                    {
                        if (piece.IsWhite == byWhite)
                        {
                            // Check if this piece can attack in this direction
                            bool isDiagonal = Math.Abs(direction) == 7 || Math.Abs(direction) == 9;
                            bool isOrthogonal = Math.Abs(direction) == 1 || Math.Abs(direction) == 8;
                            
                            if ((piece.PieceType == PieceType.Queen) ||
                                (piece.PieceType == PieceType.Bishop && isDiagonal) ||
                                (piece.PieceType == PieceType.Rook && isOrthogonal))
                            {
                                return true;
                            }
                        }
                        break; // Any piece blocks further attacks in this direction
                    }
                }
            }
            return false;
        }

        private bool IsKingAttackingSquare(Square square, bool byWhite)
        {
            int squareIndex = square.Index;
            int file = squareIndex % 8;

            foreach (int offset in KingMoves)
            {
                int attackerSquare = squareIndex + offset;
                if (IsValidSquare(attackerSquare))
                {
                    int attackerFile = attackerSquare % 8;
                    
                    if (Math.Abs(attackerFile - file) <= 1) // Valid king move
                    {
                        Piece piece = board.GetPiece(new Square(attackerSquare));
                        if (piece.PieceType == PieceType.King && piece.IsWhite == byWhite)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
