using System;
using System.Diagnostics;
using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Testing
{
    /// <summary>
    /// Performance benchmarking for chess engine components
    /// </summary>
    public static class PerformanceBenchmark
    {
        /// <summary>
        /// Benchmark move generation performance
        /// </summary>
        public static void BenchmarkMoveGeneration(int iterations = 10000)
        {
            Console.WriteLine("=== Move Generation Benchmark ===");
            
            // Test positions
            var testPositions = new[]
            {
                ("Starting Position", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"),
                ("Middle Game", "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"),
                ("Endgame", "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1")
            };

            foreach (var (name, fen) in testPositions)
            {
                BenchmarkPosition(name, fen, iterations);
                Console.WriteLine();
            }
        }

        private static void BenchmarkPosition(string positionName, string fen, int iterations)
        {
            Console.WriteLine($"Position: {positionName}");
            Console.WriteLine($"FEN: {fen}");
            
            var board = new Board(fen);
            
            // Warmup
            for (int i = 0; i < 100; i++)
            {
                board.GetPseudoLegalMoves();
            }

            // Benchmark pseudo-legal move generation
            var stopwatch = Stopwatch.StartNew();
            int totalMoves = 0;
            
            for (int i = 0; i < iterations; i++)
            {
                var moves = board.GetPseudoLegalMoves();
                totalMoves += moves.Length;
            }
            
            stopwatch.Stop();
            
            double movesPerSecond = (totalMoves / stopwatch.Elapsed.TotalSeconds);
            double avgMoves = (double)totalMoves / iterations;
            
            Console.WriteLine($"Pseudo-legal moves: {avgMoves:F1} average per position");
            Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations");
            Console.WriteLine($"Rate: {movesPerSecond:F0} moves/sec");
            Console.WriteLine($"Performance: {iterations / stopwatch.Elapsed.TotalSeconds:F0} positions/sec");
        }

        /// <summary>
        /// Perft test (performance test) for move generation verification
        /// Counts all legal positions reachable in exactly N plies
        /// </summary>
        public static long Perft(Board board, int depth)
        {
            if (depth == 0) return 1;
            
            var moves = board.GetLegalMoves(); // Use legal moves to ensure accuracy
            long nodes = 0;
            
            foreach (var move in moves)
            {
                // Make the move
                board.MakeMove(move);
                
                // Recursively count nodes at remaining depth
                nodes += Perft(board, depth - 1);
                
                // Unmake the move
                board.UnmakeMove();
            }
            
            return nodes;
        }

        /// <summary>
        /// Detailed perft with move breakdown (perft divide)
        /// Shows node count for each root move - useful for debugging
        /// </summary>
        public static void PerftDivide(Board board, int depth)
        {
            Console.WriteLine($"=== Perft Divide (Depth {depth}) ===");
            Console.WriteLine($"Position: {board.GetFEN()}");
            
            var moves = board.GetLegalMoves();
            long totalNodes = 0;
            var stopwatch = Stopwatch.StartNew();
            
            foreach (var move in moves)
            {
                board.MakeMove(move);
                long nodes = depth > 1 ? Perft(board, depth - 1) : 1;
                board.UnmakeMove();
                
                totalNodes += nodes;
                Console.WriteLine($"{move}: {nodes} nodes");
            }
            
            stopwatch.Stop();
            double nps = stopwatch.ElapsedMilliseconds > 0 ? totalNodes * 1000.0 / stopwatch.ElapsedMilliseconds : 0;
            
            Console.WriteLine($"\nTotal: {totalNodes} nodes in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Performance: {nps:F0} nodes/second");
        }

        /// <summary>
        /// Run comprehensive perft tests for known positions with expected results
        /// </summary>
        public static void RunPerftTests()
        {
            Console.WriteLine("=== Comprehensive Perft Tests ===");
            
            // Test positions with known results
            var testPositions = new[]
            {
                ("Starting Position", 
                 "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                 new long[] { 20, 400, 8902, 197281 }),
                 
                ("Kiwipete Position", 
                 "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
                 new long[] { 6, 264, 9467, 422333 }),
                 
                ("Position 3",
                 "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
                 new long[] { 14, 191, 2812, 43238 }),
                 
                ("Position 4", 
                 "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
                 new long[] { 5, 44, 1486, 62379 }),
                 
                ("Position 5",
                 "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
                 new long[] { 44, 1486, 62379, 2103487 })
            };

            foreach (var (name, fen, expectedResults) in testPositions)
            {
                Console.WriteLine($"\n--- {name} ---");
                Console.WriteLine($"FEN: {fen}");
                
                var board = new Board(fen);
                
                for (int depth = 1; depth <= Math.Min(4, expectedResults.Length); depth++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    long nodes = Perft(board, depth);
                    stopwatch.Stop();
                    
                    bool correct = depth <= expectedResults.Length && nodes == expectedResults[depth - 1];
                    string status = correct ? "✓" : "✗";
                    string expected = depth <= expectedResults.Length ? expectedResults[depth - 1].ToString() : "unknown";
                    
                    double nps = stopwatch.ElapsedMilliseconds > 0 ? nodes * 1000.0 / stopwatch.ElapsedMilliseconds : 0;
                    
                    Console.WriteLine($"Depth {depth}: {nodes} nodes ({expected} expected) {status} - {stopwatch.ElapsedMilliseconds}ms ({nps:F0} nps)");
                    
                    // Stop if we hit an error or take too long
                    if (!correct)
                    {
                        Console.WriteLine($"ERROR: Expected {expected}, got {nodes}");
                        break;
                    }
                    
                    if (stopwatch.ElapsedMilliseconds > 10000) // More than 10 seconds
                    {
                        Console.WriteLine("Stopping due to time limit");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Run a quick perft benchmark for performance measurement
        /// </summary>
        public static void BenchmarkPerft()
        {
            Console.WriteLine("=== Perft Performance Benchmark ===");
            
            var board = new Board(); // Starting position
            int targetDepth = 4;
            
            // Warmup
            Perft(board, 2);
            
            var stopwatch = Stopwatch.StartNew();
            long nodes = Perft(board, targetDepth);
            stopwatch.Stop();
            
            double nps = stopwatch.ElapsedMilliseconds > 0 ? nodes * 1000.0 / stopwatch.ElapsedMilliseconds : 0;
            
            Console.WriteLine($"Perft({targetDepth}): {nodes:N0} nodes in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Performance: {nps:N0} nodes/second");
            Console.WriteLine($"Expected: 197,281 nodes for starting position depth 4");
        }
    }
}
