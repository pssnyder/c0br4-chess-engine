using System;
using System.IO;
using System.Threading;
using ChessEngine.Core;
using ChessEngine.Search;
using ChessEngine.Testing;

namespace ChessEngine.UCI
{
    /// <summary>
    /// UCI (Universal Chess Interface) implementation
    /// Handles communication with chess GUIs
    /// </summary>
    public class UCIEngine
    {
        private Board board = new();
        private IChessBot bot = new AlphaBetaSearchBot(); // Changed to Alpha-Beta for better performance
        private bool isRunning = true;
        private string engineVersion;

        public UCIEngine()
        {
            // Read version from VERSION file
            try
            {
                string versionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VERSION");
                if (File.Exists(versionPath))
                {
                    engineVersion = File.ReadAllText(versionPath).Trim();
                }
                else
                {
                    engineVersion = "dev"; // Default for development
                }
            }
            catch
            {
                engineVersion = "unknown";
            }
        }

        public void Run()
        {
            Console.WriteLine($"ChessAI {engineVersion}");
            
            while (isRunning)
            {
                string? input = Console.ReadLine();
                if (input == null) break;
                
                ProcessCommand(input.Trim());
            }
        }

        private void ProcessCommand(string command)
        {
            string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            switch (parts[0].ToLower())
            {
                case "uci":
                    HandleUCI();
                    break;
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "ucinewgame":
                    board = new Board();
                    break;
                case "position":
                    HandlePosition(parts);
                    break;
                case "go":
                    HandleGo(parts);
                    break;
                case "stop":
                    // TODO: Implement search stopping
                    break;
                case "quit":
                    isRunning = false;
                    break;
                case "d":
                case "display":
                    DisplayBoard();
                    break;
                case "bench":
                case "benchmark":
                    RunBenchmark();
                    break;
                case "perft":
                    RunPerft(parts);
                    break;
                case "eval":
                    RunEval();
                    break;
                case "depth":
                    SetDepth(parts);
                    break;
                case "engine":
                    SetEngine(parts);
                    break;
                default:
                    // Unknown command - ignore in UCI mode
                    break;
            }
        }

        private void HandleUCI()
        {
            Console.WriteLine($"id name ChessAI {engineVersion}");
            Console.WriteLine("id author Chess AI Developer");
            // TODO: Add UCI options here
            Console.WriteLine("uciok");
        }

        private void HandlePosition(string[] parts)
        {
            if (parts.Length < 2) return;

            if (parts[1] == "startpos")
            {
                board.LoadStartPosition();
                
                // Apply moves if provided
                int movesIndex = Array.IndexOf(parts, "moves");
                if (movesIndex != -1 && movesIndex + 1 < parts.Length)
                {
                    for (int i = movesIndex + 1; i < parts.Length; i++)
                    {
                        if (TryParseAndApplyMove(parts[i]))
                        {
                            Console.WriteLine($"info string Applied move: {parts[i]}");
                        }
                        else
                        {
                            Console.WriteLine($"info string Failed to apply move: {parts[i]}");
                        }
                    }
                }
            }
            else if (parts[1] == "fen" && parts.Length >= 8)
            {
                // Reconstruct FEN string
                string fen = string.Join(" ", parts, 2, 6);
                board.LoadPosition(fen);
                
                // Apply moves if provided
                int movesIndex = Array.IndexOf(parts, "moves");
                if (movesIndex != -1 && movesIndex + 1 < parts.Length)
                {
                    for (int i = movesIndex + 1; i < parts.Length; i++)
                    {
                        if (TryParseAndApplyMove(parts[i]))
                        {
                            Console.WriteLine($"info string Applied move: {parts[i]}");
                        }
                        else
                        {
                            Console.WriteLine($"info string Failed to apply move: {parts[i]}");
                        }
                    }
                }
            }
        }

        private void HandleGo(string[] parts)
        {
            // Parse time control parameters
            TimeSpan timeLimit = TimeSpan.FromSeconds(1); // Default 1 second
            
            for (int i = 1; i < parts.Length - 1; i++)
            {
                switch (parts[i])
                {
                    case "movetime":
                        if (int.TryParse(parts[i + 1], out int movetime))
                            timeLimit = TimeSpan.FromMilliseconds(movetime);
                        break;
                    case "depth":
                        // TODO: Handle depth-limited search
                        break;
                    case "wtime":
                    case "btime":
                        // TODO: Handle time control
                        break;
                }
            }

            // Start search
            try
            {
                Move bestMove = bot.Think(board, timeLimit);
                Console.WriteLine($"bestmove {bestMove}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"info string Error: {ex.Message}");
                Console.WriteLine("bestmove 0000"); // Null move
            }
        }

        private void DisplayBoard()
        {
            // TODO: Implement board display
            Console.WriteLine("Board display not yet implemented");
            Console.WriteLine($"Position: {board.IsWhiteToMove} to move");
            
            // Show move count for now
            var pseudoMoves = board.GetPseudoLegalMoves();
            var legalMoves = board.GetLegalMoves();
            Console.WriteLine($"Pseudo-legal moves: {pseudoMoves.Length}");
            Console.WriteLine($"Legal moves: {legalMoves.Length}");
            
            // Show first few legal moves
            for (int i = 0; i < Math.Min(10, legalMoves.Length); i++)
            {
                Console.WriteLine($"  {legalMoves[i]}");
            }
            if (legalMoves.Length > 10)
            {
                Console.WriteLine($"  ... and {legalMoves.Length - 10} more");
            }
        }

        private void RunBenchmark()
        {
            Console.WriteLine("Running performance benchmark...");
            PerformanceBenchmark.BenchmarkMoveGeneration(1000);
        }

        private void RunPerft(string[] parts)
        {
            if (parts.Length > 1 && int.TryParse(parts[1], out int depth))
            {
                Console.WriteLine($"Running perft to depth {depth}...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                long nodes = PerformanceBenchmark.Perft(board, depth);
                stopwatch.Stop();
                Console.WriteLine($"Perft({depth}): {nodes} nodes in {stopwatch.ElapsedMilliseconds}ms");
            }
            else
            {
                PerformanceBenchmark.RunPerftTests();
            }
        }

        private bool TryParseAndApplyMove(string moveString)
        {
            try
            {
                // Find the matching legal move
                var legalMoves = board.GetLegalMoves();
                foreach (var legalMove in legalMoves)
                {
                    if (legalMove.ToString() == moveString)
                    {
                        board.MakeMove(legalMove);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void RunEval()
        {
            var evaluator = new Evaluation.SimpleEvaluator();
            int evaluation = evaluator.Evaluate(board);
            Console.WriteLine($"Evaluation: {evaluation} cp ({evaluation / 100.0:F2} pawns) from {(board.IsWhiteToMove ? "white" : "black")} perspective");
        }

        private void SetDepth(string[] parts)
        {
            if (parts.Length > 1 && int.TryParse(parts[1], out int depth))
            {
                if (bot is SimpleSearchBot simpleBot)
                {
                    simpleBot.SetDepth(depth);
                    Console.WriteLine($"info string Search depth set to {depth}");
                }
                else if (bot is AlphaBetaSearchBot alphabetaBot)
                {
                    alphabetaBot.SetDepth(depth);
                    Console.WriteLine($"info string Search depth set to {depth}");
                }
            }
            else
            {
                Console.WriteLine("Usage: depth <number>");
            }
        }

        private void SetEngine(string[] parts)
        {
            if (parts.Length > 1)
            {
                switch (parts[1].ToLower())
                {
                    case "random":
                        bot = new RandomBot();
                        Console.WriteLine("info string Engine set to RandomBot");
                        break;
                    case "simple":
                        bot = new SimpleSearchBot();
                        Console.WriteLine("info string Engine set to SimpleSearchBot (negamax)");
                        break;
                    case "alphabeta":
                    case "ab":
                        bot = new AlphaBetaSearchBot();
                        Console.WriteLine("info string Engine set to AlphaBetaSearchBot");
                        break;
                    default:
                        Console.WriteLine("Available engines: random, simple, alphabeta");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Current engine: " + bot.GetType().Name);
                Console.WriteLine("Available engines: random, simple, alphabeta");
            }
        }
    }
}
