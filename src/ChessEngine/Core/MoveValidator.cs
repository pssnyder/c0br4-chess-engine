using System;
using System.Collections.Generic;
using System.Linq;
using ChessEngine.Core;

namespace ChessEngine.Core
{
    /// <summary>
    /// Comprehensive move validator to prevent illegal moves
    /// Provides multiple layers of validation and detailed error reporting
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Validates a move with comprehensive checks
        /// </summary>
        public static MoveValidationResult ValidateMove(Board board, Move move)
        {
            try
            {
                // Phase 1: Basic move structure validation
                var basicResult = ValidateBasicMoveStructure(move);
                if (!basicResult.IsValid)
                    return basicResult;

                // Phase 2: Board state validation
                var boardResult = ValidateBoardState(board, move);
                if (!boardResult.IsValid)
                    return boardResult;

                // Phase 3: Piece-specific move validation
                var pieceResult = ValidatePieceSpecificMove(board, move);
                if (!pieceResult.IsValid)
                    return pieceResult;

                // Phase 4: Check/legality validation
                var legalityResult = ValidateMoveLegality(board, move);
                if (!legalityResult.IsValid)
                    return legalityResult;

                return MoveValidationResult.Valid();
            }
            catch (Exception ex)
            {
                return MoveValidationResult.Invalid($"Validation exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates that a UCI move string corresponds to a legal move
        /// </summary>
        public static MoveValidationResult ValidateUCIMove(Board board, string moveString)
        {
            try
            {
                // Parse move string
                if (string.IsNullOrEmpty(moveString) || moveString.Length < 4)
                {
                    return MoveValidationResult.Invalid($"Invalid move string format: '{moveString}'");
                }

                // Get all legal moves
                var legalMoves = board.GetLegalMoves();
                
                // Find matching move
                Move? matchingMove = null;
                foreach (var legalMove in legalMoves)
                {
                    if (legalMove.ToString() == moveString)
                    {
                        matchingMove = legalMove;
                        break;
                    }
                }

                if (matchingMove == null)
                {
                    return MoveValidationResult.Invalid(
                        $"Move '{moveString}' not found in legal moves. " +
                        $"Available moves: {string.Join(", ", legalMoves.Take(10).Select(m => m.ToString()))}");
                }

                // Double-check the found move
                return ValidateMove(board, matchingMove.Value);
            }
            catch (Exception ex)
            {
                return MoveValidationResult.Invalid($"UCI validation exception: {ex.Message}");
            }
        }

        private static MoveValidationResult ValidateBasicMoveStructure(Move move)
        {
            // Check for null move
            if (move.IsNull)
                return MoveValidationResult.Invalid("Null move");

            // Check square bounds
            if (!IsValidSquareIndex(move.StartSquare.Index))
                return MoveValidationResult.Invalid($"Invalid start square: {move.StartSquare.Index}");

            if (!IsValidSquareIndex(move.TargetSquare.Index))
                return MoveValidationResult.Invalid($"Invalid target square: {move.TargetSquare.Index}");

            // Check that start and target are different (except for null move)
            if (move.StartSquare.Index == move.TargetSquare.Index)
                return MoveValidationResult.Invalid("Start and target squares are the same");

            // Check piece type validity
            if (move.MovePieceType == PieceType.None)
                return MoveValidationResult.Invalid("Move piece type is None");

            return MoveValidationResult.Valid();
        }

        private static MoveValidationResult ValidateBoardState(Board board, Move move)
        {
            // Check that there's actually a piece on the start square
            var startPiece = board.GetPiece(move.StartSquare);
            if (startPiece.IsNull)
                return MoveValidationResult.Invalid($"No piece on start square {move.StartSquare.Name}");

            // Check that the piece type matches
            if (startPiece.PieceType != move.MovePieceType)
                return MoveValidationResult.Invalid(
                    $"Piece type mismatch: move says {move.MovePieceType}, board has {startPiece.PieceType}");

            // Check that it's the right player's turn
            if (startPiece.IsWhite != board.IsWhiteToMove)
                return MoveValidationResult.Invalid(
                    $"Wrong player: {(startPiece.IsWhite ? "White" : "Black")} piece, but {(board.IsWhiteToMove ? "White" : "Black")} to move");

            // Check capture piece consistency
            var targetPiece = board.GetPiece(move.TargetSquare);
            if (move.IsCapture)
            {
                if (targetPiece.IsNull)
                {
                    // Could be en passant
                    if (move.Flag != MoveFlag.EnPassant)
                        return MoveValidationResult.Invalid("Move marked as capture but target square is empty");
                }
                else
                {
                    if (targetPiece.PieceType != move.CapturePieceType)
                        return MoveValidationResult.Invalid(
                            $"Capture piece mismatch: move says {move.CapturePieceType}, board has {targetPiece.PieceType}");

                    if (targetPiece.IsWhite == startPiece.IsWhite)
                        return MoveValidationResult.Invalid("Cannot capture own piece");
                }
            }
            else
            {
                if (!targetPiece.IsNull)
                    return MoveValidationResult.Invalid("Target square occupied but move not marked as capture");
            }

            return MoveValidationResult.Valid();
        }

        private static MoveValidationResult ValidatePieceSpecificMove(Board board, Move move)
        {
            var startPiece = board.GetPiece(move.StartSquare);

            switch (startPiece.PieceType)
            {
                case PieceType.Pawn:
                    return ValidatePawnMove(board, move);
                case PieceType.Knight:
                    return ValidateKnightMove(move);
                case PieceType.Bishop:
                    return ValidateBishopMove(board, move);
                case PieceType.Rook:
                    return ValidateRookMove(board, move);
                case PieceType.Queen:
                    return ValidateQueenMove(board, move);
                case PieceType.King:
                    return ValidateKingMove(board, move);
                default:
                    return MoveValidationResult.Invalid($"Unknown piece type: {startPiece.PieceType}");
            }
        }

        private static MoveValidationResult ValidateMoveLegality(Board board, Move move)
        {
            try
            {
                // Save current state
                var originalState = CaptureFullBoardState(board);

                // Make move temporarily
                board.MakeMove(move);

                // Check if king is in check (illegal)
                bool isInCheck = board.IsInCheck();

                // Restore state
                board.UnmakeMove();

                // Verify state was restored correctly
                if (!VerifyBoardStateRestored(board, originalState))
                {
                    return MoveValidationResult.Invalid("Board state corruption detected during move validation");
                }

                if (isInCheck)
                {
                    return MoveValidationResult.Invalid("Move leaves king in check");
                }

                return MoveValidationResult.Valid();
            }
            catch (Exception ex)
            {
                return MoveValidationResult.Invalid($"Legality check failed: {ex.Message}");
            }
        }

        #region Piece-Specific Validation

        private static MoveValidationResult ValidatePawnMove(Board board, Move move)
        {
            var startPiece = board.GetPiece(move.StartSquare);
            int startFile = move.StartSquare.Index % 8;
            int startRank = move.StartSquare.Index / 8;
            int targetFile = move.TargetSquare.Index % 8;
            int targetRank = move.TargetSquare.Index / 8;

            bool isWhite = startPiece.IsWhite;
            int direction = isWhite ? 1 : -1;
            int rankDiff = targetRank - startRank;
            int fileDiff = Math.Abs(targetFile - startFile);

            // Check direction
            if (rankDiff * direction <= 0)
                return MoveValidationResult.Invalid("Pawn moving in wrong direction");

            if (fileDiff == 0)
            {
                // Forward move
                if (Math.Abs(rankDiff) == 1)
                {
                    // Single forward move
                    if (move.IsCapture)
                        return MoveValidationResult.Invalid("Pawn forward move cannot be capture");
                }
                else if (Math.Abs(rankDiff) == 2)
                {
                    // Double forward move
                    int startingRank = isWhite ? 1 : 6;
                    if (startRank != startingRank)
                        return MoveValidationResult.Invalid("Pawn double move only from starting rank");
                    if (move.IsCapture)
                        return MoveValidationResult.Invalid("Pawn double move cannot be capture");
                }
                else
                {
                    return MoveValidationResult.Invalid("Invalid pawn forward move distance");
                }
            }
            else if (fileDiff == 1 && Math.Abs(rankDiff) == 1)
            {
                // Diagonal move (capture)
                if (!move.IsCapture && move.Flag != MoveFlag.EnPassant)
                    return MoveValidationResult.Invalid("Pawn diagonal move must be capture");
            }
            else
            {
                return MoveValidationResult.Invalid("Invalid pawn move pattern");
            }

            // Check promotion
            int promotionRank = isWhite ? 7 : 0;
            if (targetRank == promotionRank)
            {
                if (!move.IsPromotion)
                    return MoveValidationResult.Invalid("Pawn reaching promotion rank must promote");
            }
            else
            {
                if (move.IsPromotion)
                    return MoveValidationResult.Invalid("Pawn promotion only on promotion rank");
            }

            return MoveValidationResult.Valid();
        }

        private static MoveValidationResult ValidateKnightMove(Move move)
        {
            int startFile = move.StartSquare.Index % 8;
            int startRank = move.StartSquare.Index / 8;
            int targetFile = move.TargetSquare.Index % 8;
            int targetRank = move.TargetSquare.Index / 8;

            int fileDiff = Math.Abs(targetFile - startFile);
            int rankDiff = Math.Abs(targetRank - startRank);

            if (!((fileDiff == 2 && rankDiff == 1) || (fileDiff == 1 && rankDiff == 2)))
                return MoveValidationResult.Invalid("Invalid knight move pattern");

            return MoveValidationResult.Valid();
        }

        private static MoveValidationResult ValidateBishopMove(Board board, Move move)
        {
            return ValidateSlidingMove(board, move, isDiagonal: true, isOrthogonal: false);
        }

        private static MoveValidationResult ValidateRookMove(Board board, Move move)
        {
            return ValidateSlidingMove(board, move, isDiagonal: false, isOrthogonal: true);
        }

        private static MoveValidationResult ValidateQueenMove(Board board, Move move)
        {
            return ValidateSlidingMove(board, move, isDiagonal: true, isOrthogonal: true);
        }

        private static MoveValidationResult ValidateSlidingMove(Board board, Move move, bool isDiagonal, bool isOrthogonal)
        {
            int startFile = move.StartSquare.Index % 8;
            int startRank = move.StartSquare.Index / 8;
            int targetFile = move.TargetSquare.Index % 8;
            int targetRank = move.TargetSquare.Index / 8;

            int fileDiff = targetFile - startFile;
            int rankDiff = targetRank - startRank;

            bool isDiagonalMove = Math.Abs(fileDiff) == Math.Abs(rankDiff);
            bool isOrthogonalMove = fileDiff == 0 || rankDiff == 0;

            if (isDiagonalMove && !isDiagonal)
                return MoveValidationResult.Invalid("Piece cannot move diagonally");

            if (isOrthogonalMove && !isOrthogonal)
                return MoveValidationResult.Invalid("Piece cannot move orthogonally");

            if (!isDiagonalMove && !isOrthogonalMove)
                return MoveValidationResult.Invalid("Invalid sliding move pattern");

            // Check path is clear
            int stepFile = fileDiff == 0 ? 0 : (fileDiff > 0 ? 1 : -1);
            int stepRank = rankDiff == 0 ? 0 : (rankDiff > 0 ? 1 : -1);

            int currentFile = startFile + stepFile;
            int currentRank = startRank + stepRank;

            while (currentFile != targetFile || currentRank != targetRank)
            {
                int squareIndex = currentRank * 8 + currentFile;
                var piece = board.GetPiece(new Square(squareIndex));
                if (!piece.IsNull)
                    return MoveValidationResult.Invalid("Path blocked by piece");

                currentFile += stepFile;
                currentRank += stepRank;
            }

            return MoveValidationResult.Valid();
        }

        private static MoveValidationResult ValidateKingMove(Board board, Move move)
        {
            int startFile = move.StartSquare.Index % 8;
            int startRank = move.StartSquare.Index / 8;
            int targetFile = move.TargetSquare.Index % 8;
            int targetRank = move.TargetSquare.Index / 8;

            int fileDiff = Math.Abs(targetFile - startFile);
            int rankDiff = Math.Abs(targetRank - startRank);

            if (move.Flag == MoveFlag.Castling)
            {
                // Castling validation would go here
                // For now, accept castling moves
                return MoveValidationResult.Valid();
            }

            if (fileDiff > 1 || rankDiff > 1)
                return MoveValidationResult.Invalid("King can only move one square");

            return MoveValidationResult.Valid();
        }

        #endregion

        #region Helper Methods

        private static bool IsValidSquareIndex(int index)
        {
            return index >= 0 && index < 64;
        }

        private static BoardState CaptureFullBoardState(Board board)
        {
            // Create a snapshot of the current board state
            // This is a simplified version - in practice, you'd want to capture
            // all relevant state information
            return new BoardState
            {
                IsWhiteToMove = board.IsWhiteToMove,
                FullMoveNumber = board.FullMoveNumber,
                HalfMoveClock = board.HalfMoveClock
                // Add other state as needed
            };
        }

        private static bool VerifyBoardStateRestored(Board board, BoardState originalState)
        {
            return board.IsWhiteToMove == originalState.IsWhiteToMove &&
                   board.FullMoveNumber == originalState.FullMoveNumber &&
                   board.HalfMoveClock == originalState.HalfMoveClock;
            // Add other state verification as needed
        }

        #endregion
    }

    /// <summary>
    /// Result of move validation
    /// </summary>
    public struct MoveValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private MoveValidationResult(bool isValid, string errorMessage = "")
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static MoveValidationResult Valid() => new(true);
        public static MoveValidationResult Invalid(string errorMessage) => new(false, errorMessage);

        public override string ToString()
        {
            return IsValid ? "Valid" : $"Invalid: {ErrorMessage}";
        }
    }
}
