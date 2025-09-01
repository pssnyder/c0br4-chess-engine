using System;
using System.Runtime.CompilerServices;

namespace C0BR4ChessEngine.Core
{
    /// <summary>
    /// Magic bitboard implementation for efficient sliding piece attack generation
    /// Uses pre-computed magic numbers and lookup tables for bishops and rooks
    /// </summary>
    public static class MagicBitboards
    {
        // Magic numbers for rooks (64 squares)
        private static readonly ulong[] RookMagics = {
            0xa8002c000108020UL, 0x4440200140003000UL, 0x8080200101146000UL, 0x200082000801000UL,
            0x200020010080800UL, 0x1000020008040200UL, 0x40000C4000200042UL, 0x4400020004484100UL,
            0x208000400080UL, 0x10200040008000UL, 0x100020004000UL, 0x2480104000800080UL,
            0x800040020008UL, 0x1000100020004UL, 0x4000100020002UL, 0x4100008000004284UL,
            0x1000802000484000UL, 0x204000800080UL, 0x20100080080080UL, 0x1000100080040080UL,
            0x800080040020UL, 0x400020010004UL, 0x200010008004UL, 0x81000208004284UL,
            0x2008020080004000UL, 0x20004000UL, 0x80002000204000UL, 0x1001000804008UL,
            0x804008004UL, 0x200208010004UL, 0x80010002001UL, 0x8004200041UL,
            0x20008080UL, 0x404000UL, 0x802000UL, 0x1008800UL,
            0x800400UL, 0x200400UL, 0x81000200UL, 0x4000200082UL,
            0x800082UL, 0x1000020040UL, 0x200080200040UL, 0x80080080020UL,
            0x20040080080UL, 0x10020040UL, 0x4008020UL, 0x8004021UL,
            0x80004020UL, 0x20004010UL, 0x80002008UL, 0x1000008004UL,
            0x800004002UL, 0x20002001UL, 0x200001UL, 0x420001UL,
            0x2008020080004000UL, 0x204000800080UL, 0x20100080080080UL, 0x1000100080040080UL,
            0x800080040020UL, 0x400020010004UL, 0x200010008004UL, 0x81000208004284UL
        };

        // Magic numbers for bishops (64 squares)
        private static readonly ulong[] BishopMagics = {
            0x89a1121896040240UL, 0x2004844802002010UL, 0x2068080051921000UL, 0x62880a0220200808UL,
            0x4042004402000UL, 0x100822020200011UL, 0xc00444222012000aUL, 0x28808801216001UL,
            0x400492088408100UL, 0x201c401040c0084UL, 0x840800910a0010UL, 0x82080240060UL,
            0x2000840504006000UL, 0x30010c4108405004UL, 0x1008005410080802UL, 0x8144042209100900UL,
            0x208081020014400UL, 0x4800201208ca00UL, 0xf18140408012008UL, 0x1004002802102001UL,
            0x841000820080811UL, 0x40200200a42008UL, 0x800054042000UL, 0x88010400410c9000UL,
            0x520040470104290UL, 0x1004040051500081UL, 0x2002081833080021UL, 0x400c00c010142UL,
            0x941408200c002000UL, 0x658810000806011UL, 0x188071040440a00UL, 0x4800404002011c00UL,
            0x104442040404200UL, 0x511080202091021UL, 0x4022401120400UL, 0x80c0040400080120UL,
            0x8040010040820802UL, 0x480810700020090UL, 0x102008e00040242UL, 0x809005202050100UL,
            0x8002024220104080UL, 0x431008804142000UL, 0x19001802081400UL, 0x200014208040080UL,
            0x3308082008200100UL, 0x41010500040c020UL, 0x4012020c04210308UL, 0x208220a202004080UL,
            0x111040120082000UL, 0x6803040141280a00UL, 0x2101004202410000UL, 0x8200000041108022UL,
            0x21082088000UL, 0x2410204010040UL, 0x40100400809000UL, 0x822088220820214UL,
            0x40808090012004UL, 0x910224040218c9UL, 0x402814422015008UL, 0x90014004842410UL,
            0x1000042304105UL, 0x10008830412a00UL, 0x2520081090008908UL, 0x40102000a0a60140UL
        };

        // Number of bits in the mask for each square (rook)
        private static readonly int[] RookBits = {
            12, 11, 11, 11, 11, 11, 11, 12,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            12, 11, 11, 11, 11, 11, 11, 12
        };

        // Number of bits in the mask for each square (bishop)
        private static readonly int[] BishopBits = {
            6, 5, 5, 5, 5, 5, 5, 6,
            5, 5, 5, 5, 5, 5, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 5, 5, 5, 5, 5, 5,
            6, 5, 5, 5, 5, 5, 5, 6
        };

        // Lookup tables for attacks
        private static readonly ulong[][] RookAttacks = new ulong[64][];
        private static readonly ulong[][] BishopAttacks = new ulong[64][];

        // Mask bitboards (relevant occupancy squares)
        private static readonly ulong[] RookMasks = new ulong[64];
        private static readonly ulong[] BishopMasks = new ulong[64];

        /// <summary>
        /// Initialize magic bitboard lookup tables
        /// Must be called before using GetRookAttacks or GetBishopAttacks
        /// </summary>
        public static void Initialize()
        {
            InitializeRookMasks();
            InitializeBishopMasks();
            InitializeRookAttacks();
            InitializeBishopAttacks();
        }

        /// <summary>
        /// Get rook attacks for a square given board occupancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetRookAttacks(int square, ulong occupancy)
        {
            if (square < 0 || square >= 64)
            {
                throw new ArgumentOutOfRangeException(nameof(square), $"Square {square} is out of bounds (0-63)");
            }
            
            occupancy &= RookMasks[square];
            occupancy *= RookMagics[square];
            int index = (int)(occupancy >> (64 - RookBits[square]));
            
            if (index >= RookAttacks[square].Length)
            {
                throw new IndexOutOfRangeException($"Rook attack index {index} out of bounds for square {square}, array length {RookAttacks[square].Length}");
            }
            
            return RookAttacks[square][index];
        }

        /// <summary>
        /// Get bishop attacks for a square given board occupancy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetBishopAttacks(int square, ulong occupancy)
        {
            if (square < 0 || square >= 64)
            {
                throw new ArgumentOutOfRangeException(nameof(square), $"Square {square} is out of bounds (0-63)");
            }
            
            occupancy &= BishopMasks[square];
            occupancy *= BishopMagics[square];
            int index = (int)(occupancy >> (64 - BishopBits[square]));
            
            if (index >= BishopAttacks[square].Length)
            {
                throw new IndexOutOfRangeException($"Bishop attack index {index} out of bounds for square {square}, array length {BishopAttacks[square].Length}");
            }
            
            return BishopAttacks[square][index];
        }

        /// <summary>
        /// Get queen attacks (combination of rook and bishop attacks)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetQueenAttacks(int square, ulong occupancy)
        {
            return GetRookAttacks(square, occupancy) | GetBishopAttacks(square, occupancy);
        }

        private static void InitializeRookMasks()
        {
            for (int square = 0; square < 64; square++)
            {
                RookMasks[square] = GenerateRookMask(square);
            }
        }

        private static void InitializeBishopMasks()
        {
            for (int square = 0; square < 64; square++)
            {
                BishopMasks[square] = GenerateBishopMask(square);
            }
        }

        private static void InitializeRookAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                ulong mask = RookMasks[square];
                int bitCount = RookBits[square];
                int permutations = 1 << bitCount;
                
                RookAttacks[square] = new ulong[permutations];
                
                for (int i = 0; i < permutations; i++)
                {
                    ulong occupancy = GenerateOccupancy(i, mask);
                    ulong magic = RookMagics[square];
                    int index = (int)((occupancy * magic) >> (64 - bitCount));
                    RookAttacks[square][index] = GenerateRookAttacks(square, occupancy);
                }
            }
        }

        private static void InitializeBishopAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                ulong mask = BishopMasks[square];
                int bitCount = BishopBits[square];
                int permutations = 1 << bitCount;
                
                BishopAttacks[square] = new ulong[permutations];
                
                for (int i = 0; i < permutations; i++)
                {
                    ulong occupancy = GenerateOccupancy(i, mask);
                    ulong magic = BishopMagics[square];
                    int index = (int)((occupancy * magic) >> (64 - bitCount));
                    BishopAttacks[square][index] = GenerateBishopAttacks(square, occupancy);
                }
            }
        }

        private static ulong GenerateRookMask(int square)
        {
            ulong mask = 0UL;
            int file = Bitboard.GetFile(square);
            int rank = Bitboard.GetRank(square);

            // Horizontal (exclude edges)
            for (int f = file + 1; f <= 6; f++)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(f, rank));
            for (int f = file - 1; f >= 1; f--)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(f, rank));

            // Vertical (exclude edges)
            for (int r = rank + 1; r <= 6; r++)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(file, r));
            for (int r = rank - 1; r >= 1; r--)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(file, r));

            return mask;
        }

        private static ulong GenerateBishopMask(int square)
        {
            ulong mask = 0UL;
            int file = Bitboard.GetFile(square);
            int rank = Bitboard.GetRank(square);

            // Diagonal directions (exclude edges)
            for (int f = file + 1, r = rank + 1; f <= 6 && r <= 6; f++, r++)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(f, r));
            for (int f = file + 1, r = rank - 1; f <= 6 && r >= 1; f++, r--)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(f, r));
            for (int f = file - 1, r = rank + 1; f >= 1 && r <= 6; f--, r++)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(f, r));
            for (int f = file - 1, r = rank - 1; f >= 1 && r >= 1; f--, r--)
                mask |= Bitboard.SquareToBitboard(Bitboard.GetSquare(f, r));

            return mask;
        }

        private static ulong GenerateOccupancy(int index, ulong mask)
        {
            ulong occupancy = 0UL;
            int bitCount = Bitboard.PopCount(mask);
            
            for (int i = 0; i < bitCount; i++)
            {
                int square = Bitboard.PopLSB(ref mask);
                if ((index & (1 << i)) != 0)
                {
                    occupancy |= Bitboard.SquareToBitboard(square);
                }
            }
            
            return occupancy;
        }

        private static ulong GenerateRookAttacks(int square, ulong occupancy)
        {
            ulong attacks = 0UL;
            int file = Bitboard.GetFile(square);
            int rank = Bitboard.GetRank(square);

            // Horizontal attacks
            for (int f = file + 1; f <= 7; f++)
            {
                int sq = Bitboard.GetSquare(f, rank);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }
            for (int f = file - 1; f >= 0; f--)
            {
                int sq = Bitboard.GetSquare(f, rank);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }

            // Vertical attacks
            for (int r = rank + 1; r <= 7; r++)
            {
                int sq = Bitboard.GetSquare(file, r);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }
            for (int r = rank - 1; r >= 0; r--)
            {
                int sq = Bitboard.GetSquare(file, r);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }

            return attacks;
        }

        private static ulong GenerateBishopAttacks(int square, ulong occupancy)
        {
            ulong attacks = 0UL;
            int file = Bitboard.GetFile(square);
            int rank = Bitboard.GetRank(square);

            // Diagonal attacks
            for (int f = file + 1, r = rank + 1; f <= 7 && r <= 7; f++, r++)
            {
                int sq = Bitboard.GetSquare(f, r);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }
            for (int f = file + 1, r = rank - 1; f <= 7 && r >= 0; f++, r--)
            {
                int sq = Bitboard.GetSquare(f, r);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }
            for (int f = file - 1, r = rank + 1; f >= 0 && r <= 7; f--, r++)
            {
                int sq = Bitboard.GetSquare(f, r);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }
            for (int f = file - 1, r = rank - 1; f >= 0 && r >= 0; f--, r--)
            {
                int sq = Bitboard.GetSquare(f, r);
                attacks |= Bitboard.SquareToBitboard(sq);
                if ((occupancy & Bitboard.SquareToBitboard(sq)) != 0) break;
            }

            return attacks;
        }
    }
}
