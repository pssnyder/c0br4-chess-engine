using System;
using C0BR4ChessEngine.Core;

class DebugQueenMoves
{
    static void Main(string[] args)
    {
        // Initialize magic bitboards
        MagicBitboards.Initialize();
        
        // Create test position: rnbqkbnr/pppp1ppp/8/4p3/Q7/8/PPPPPPPP/RNB1KBNR w KQkq e6 0 2
        var position = BitboardPosition.FromFEN("rnbqkbnr/pppp1ppp/8/4p3/Q7/8/PPPPPPPP/RNB1KBNR w KQkq e6 0 2");
        
        Console.WriteLine($"Position loaded: {position.ToFEN()}");
        Console.WriteLine($"White to move: {position.IsWhiteToMove}");
        
        // Queen is on a4 (square 24)
        int queenSquare = 24; // a4
        Console.WriteLine($"Queen on square {queenSquare} (a4)");
        
        // Get queen attacks
        ulong queenAttacks = MagicBitboards.GetQueenAttacks(queenSquare, position.AllPieces);
        Console.WriteLine($"Queen attacks bitboard: 0x{queenAttacks:X16}");
        
        // Get friendly pieces
        ulong friendlyPieces = position.GetAllPieces(true); // white pieces
        Console.WriteLine($"Friendly pieces bitboard: 0x{friendlyPieces:X16}");
        
        // Get legal queen moves (attacks minus friendly pieces)
        ulong legalMoves = queenAttacks & ~friendlyPieces;
        Console.WriteLine($"Legal moves bitboard: 0x{legalMoves:X16}");
        
        // Check occupancy on the a-file specifically
        Console.WriteLine("Pieces on a-file (file 0):");
        for (int rank = 0; rank < 8; rank++)
        {
            int square = rank * 8; // a1, a2, a3, a4, a5, a6, a7, a8
            var (piece, color) = position.GetPieceAt(square);
            string squareName = SquareToString(square);
            if (piece != PieceType.None)
            {
                Console.WriteLine($"  {squareName}: {piece} ({color})");
            }
            else
            {
                Console.WriteLine($"  {squareName}: empty");
            }
        }
        
        // Convert to square list
        Console.WriteLine("Legal queen moves from a4:");
        ulong moves = legalMoves;
        while (moves != 0)
        {
            int toSquare = Bitboard.PopLSB(ref moves);
            string squareName = SquareToString(toSquare);
            var (piece, color) = position.GetPieceAt(toSquare);
            string capture = piece != PieceType.None ? $" (captures {piece})" : "";
            Console.WriteLine($"  a4-{squareName}{capture}");
            
            // Check if this is the illegal move a4-a1
            if (toSquare == 0) // a1
            {
                Console.WriteLine("    *** This is the illegal move a4-a1! ***");
                Console.WriteLine("    There should be pieces blocking this path!");
            }
        }
    }
    
    static string SquareToString(int square)
    {
        int file = square % 8;
        int rank = square / 8;
        return $"{(char)('a' + file)}{rank + 1}";
    }
}
