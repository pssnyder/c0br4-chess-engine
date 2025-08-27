using System;
using System.IO;
using System.Threading;
using ChessEngine.Core;
using ChessEngine.Search;
using ChessEngine.Testing;
using ChessEngine.Evaluation;
using ChessEngine.Opening;

namespace ChessEngine.UCI
{
    /// <summary>
    /// UCI (Universal Chess Interface) implementation
    /// Handles communication with chess GUIs
    /// </summary>
    public class UCIEngine
    {
        private Board board = new();
        private IChessBot bot = new TranspositionSearchBot(); // v0.6: Alpha-beta with move ordering, quiescence, and transposition table
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
                case "debug":
                    IllegalMoveDebugger.AnalyzePosition(board);
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
            // Parse time control parameters using TimeManager
            var timeControl = TimeManager.ParseTimeControl(parts);
            
            // Calculate game phase for time allocation
            double gamePhase = GamePhase.CalculatePhase(board);
            
            // Determine time allocation
            int timeAllocation;
            
            if (timeControl.Depth > 0)
            {
                // Fixed depth search - use generous time limit
                timeAllocation = 30000; // 30 seconds
            }
            else if (timeControl.Infinite)
            {
                // Infinite search - use very generous time limit
                timeAllocation = 300000; // 5 minutes
            }
            else
            {
                // Calculate optimal time allocation
                timeAllocation = TimeManager.CalculateTimeAllocation(timeControl, board.IsWhiteToMove, gamePhase);
            }
            
            // Apply material balance time multiplier
            double materialMultiplier = GamePhase.GetMaterialTimeMultiplier(board);
            timeAllocation = (int)(timeAllocation * materialMultiplier);
            
            // Output time management info for debugging
            Console.WriteLine($"info string Game phase: {GamePhase.GetPhaseName(board)} ({gamePhase:F2})");
            Console.WriteLine($"info string Time allocation: {timeAllocation}ms");
            if (timeControl.WhiteTime > 0 || timeControl.BlackTime > 0)
            {
                int remainingTime = board.IsWhiteToMove ? timeControl.WhiteTime : timeControl.BlackTime;
                Console.WriteLine($"info string Remaining time: {remainingTime}ms");
            }

            // Start search with time limit
            try
            {
                // Check opening book first
                string? bookMove = OpeningBook.GetOpeningMove(board);
                
                if (bookMove != null && OpeningBook.IsInOpeningPhase(board))
                {
                    // Try to parse and play the book move
                    var move = AlgebraicNotation.ParseMove(board, bookMove);
                    
                    if (move != null)
                    {
                        Console.WriteLine($"info string Opening book: {bookMove}");
                        Console.WriteLine($"bestmove {move}");
                        return;
                    }
                    
                    // If book move not found in legal moves, fall through to search
                    Console.WriteLine($"info string Book move {bookMove} not found, using search");
                }
                
                // Fall back to engine search
                var timeSpan = TimeSpan.FromMilliseconds(timeAllocation);
                Move bestMove = bot.Think(board, timeSpan);
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
                        // Use MoveValidator for additional verification
                        var validationResult = MoveValidator.ValidateMove(board, legalMove);
                        if (!validationResult.IsValid)
                        {
                            Console.WriteLine($"ERROR: Move {moveString} failed validation: {validationResult.ErrorMessage}");
                            IllegalMoveDebugger.LogIllegalMoveAttempt(board, legalMove, validationResult.ErrorMessage);
                            return false;
                        }
                        
                        board.MakeMove(legalMove);
                        return true;
                    }
                }
                
                // If we get here, the move wasn't found in legal moves
                Console.WriteLine($"ERROR: Move {moveString} is not in the list of legal moves");
                IllegalMoveDebugger.LogUnknownMoveAttempt(board, moveString);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception while parsing move {moveString}: {ex.Message}");
                IllegalMoveDebugger.LogMoveException(board, moveString, ex);
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
