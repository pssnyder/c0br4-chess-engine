using System;

namespace C0BR4ChessEngine.Search
{
    /// <summary>
    /// Manages time allocation for chess engine searches based on time controls
    /// </summary>
    public class TimeManager
    {
        public struct TimeControl
        {
            public int WhiteTime { get; set; }     // Milliseconds remaining for white
            public int BlackTime { get; set; }     // Milliseconds remaining for black
            public int WhiteIncrement { get; set; } // Milliseconds increment per move for white
            public int BlackIncrement { get; set; } // Milliseconds increment per move for black
            public int MovesToGo { get; set; }     // Moves to next time control (0 = no limit)
            public int MoveTime { get; set; }      // Fixed time per move (0 = use time control)
            public int Depth { get; set; }        // Fixed depth (0 = use time control)
            public bool Infinite { get; set; }    // Search until stopped
        }

        /// <summary>
        /// Calculate optimal time allocation for the current move
        /// </summary>
        /// <param name="timeControl">Time control parameters</param>
        /// <param name="isWhiteToMove">Whether white is to move</param>
        /// <param name="gamePhase">Estimated game phase (0.0 = endgame, 1.0 = opening)</param>
        /// <returns>Recommended time allocation in milliseconds</returns>
        public static int CalculateTimeAllocation(TimeControl timeControl, bool isWhiteToMove, double gamePhase = 0.5)
        {
            // Fixed time per move has highest priority
            if (timeControl.MoveTime > 0)
            {
                return Math.Max(100, timeControl.MoveTime - 50); // Reserve 50ms for overhead
            }

            // Fixed depth or infinite search - use generous time
            if (timeControl.Depth > 0 || timeControl.Infinite)
            {
                return 30000; // 30 seconds for analysis
            }

            // Get remaining time for current player
            int remainingTime = isWhiteToMove ? timeControl.WhiteTime : timeControl.BlackTime;
            int increment = isWhiteToMove ? timeControl.WhiteIncrement : timeControl.BlackIncrement;

            // Emergency time - if we have less than 3 seconds, play VERY quickly
            if (remainingTime < 3000)
            {
                return Math.Max(30, remainingTime / 30); // Use 3% of remaining time, minimum 30ms
            }

            // Low time - if we have less than 15 seconds, be MORE conservative
            if (remainingTime < 15000)
            {
                return Math.Max(80, remainingTime / 20 + increment / 3); // ~5% of time + 1/3 increment
            }

            // Calculate base time allocation - MORE CONSERVATIVE for deeper searches
            int baseTime;
            
            if (timeControl.MovesToGo > 0)
            {
                // Classical time control - divide remaining time by moves to go
                baseTime = remainingTime / Math.Max(1, timeControl.MovesToGo);
                // Add increment since we'll get it back
                baseTime += increment;
            }
            else
            {
                // Increment-based time control
                // Use a MORE CONSERVATIVE fraction of remaining time plus most of the increment
                // OLD: estimatedMovesLeft based on game phase (20-40 moves)
                // NEW: Use more conservative estimates to save time for deeper search
                int estimatedMovesLeft = EstimateMovesRemaining(gamePhase, remainingTime);
                
                // More conservative time usage - save time for deeper searches
                // In longer games (30+ min), aim for 50-60 moves total
                baseTime = remainingTime / estimatedMovesLeft + (increment * 3) / 4;
            }

            // Apply game phase adjustments - REDUCED multipliers for more consistent time usage
            double phaseMultiplier = CalculatePhaseMultiplier(gamePhase);
            baseTime = (int)(baseTime * phaseMultiplier);

            // Apply safety margins
            baseTime = ApplySafetyMargins(baseTime, remainingTime);

            // Ensure minimum and maximum bounds - limit to 1/5 of remaining time for safety
            return Math.Max(50, Math.Min(baseTime, remainingTime / 5));
        }

        /// <summary>
        /// Estimate remaining moves based on game phase and remaining time
        /// More conservative estimates for longer time controls
        /// </summary>
        private static int EstimateMovesRemaining(double gamePhase, int remainingTime)
        {
            // Base estimates by game phase
            // Opening: ~50 moves, Middlegame: ~40 moves, Endgame: ~25 moves
            int phaseEstimate = (int)(25 + gamePhase * 25);
            
            // Adjust based on remaining time - longer games = more conservative
            if (remainingTime > 1800000) // 30+ minutes
            {
                phaseEstimate = Math.Max(phaseEstimate, 50); // Assume at least 50 moves left
            }
            else if (remainingTime > 600000) // 10+ minutes
            {
                phaseEstimate = Math.Max(phaseEstimate, 40); // Assume at least 40 moves left
            }
            else if (remainingTime > 300000) // 5+ minutes
            {
                phaseEstimate = Math.Max(phaseEstimate, 30); // Assume at least 30 moves left
            }
            
            return phaseEstimate;
        }

        /// <summary>
        /// Calculate time multiplier based on game phase
        /// REDUCED multipliers for more consistent time usage across game phases
        /// </summary>
        private static double CalculatePhaseMultiplier(double gamePhase)
        {
            // Spend slightly more time in middlegame but less variation overall
            // Opening: 0.95x, Middlegame: 1.1x, Endgame: 0.9x (reduced from 0.9/1.2/0.8)
            if (gamePhase > 0.7) // Opening
                return 0.95;
            else if (gamePhase > 0.3) // Middlegame
                return 1.1;
            else // Endgame
                return 0.9;
        }

        /// <summary>
        /// Apply safety margins to prevent time troubles
        /// </summary>
        private static int ApplySafetyMargins(int baseTime, int remainingTime)
        {
            // Never use more than 1/3 of remaining time on a single move
            if (baseTime > remainingTime / 3)
                baseTime = remainingTime / 3;

            // Reserve some time for communication overhead
            baseTime = Math.Max(100, baseTime - 50);

            return baseTime;
        }

        /// <summary>
        /// Calculate optimal search depth based on time control and game phase
        /// Adaptive depth targeting: depth 6 minimum, depth 10+ for longer games
        /// </summary>
        /// <param name="timeControl">Time control parameters</param>
        /// <param name="isWhiteToMove">Whether white is to move</param>
        /// <param name="gamePhase">Estimated game phase</param>
        /// <returns>Recommended search depth (1-12)</returns>
        public static int CalculateSearchDepth(TimeControl timeControl, bool isWhiteToMove, double gamePhase = 0.5)
        {
            // Fixed depth has highest priority
            if (timeControl.Depth > 0)
            {
                return Math.Min(timeControl.Depth, 12); // Cap at depth 12 for safety
            }

            // Infinite search - aim for maximum depth
            if (timeControl.Infinite)
            {
                return 12;
            }

            // Get remaining time for current player
            int remainingTime = isWhiteToMove ? timeControl.WhiteTime : timeControl.BlackTime;
            int increment = isWhiteToMove ? timeControl.WhiteIncrement : timeControl.BlackIncrement;
            
            // Fixed move time - calculate depth based on time available
            if (timeControl.MoveTime > 0)
            {
                return DepthFromMoveTime(timeControl.MoveTime);
            }

            // Calculate depth based on total time available
            // Consider both remaining time and increment
            int effectiveTime = remainingTime + (increment * 20); // Rough estimate including future increments

            // Depth calculation based on time control
            if (effectiveTime >= 1800000) // 30+ minutes total
            {
                return 10; // Deep search for long games
            }
            else if (effectiveTime >= 900000) // 15+ minutes
            {
                return 9;
            }
            else if (effectiveTime >= 600000) // 10+ minutes
            {
                return 8;
            }
            else if (effectiveTime >= 300000) // 5+ minutes
            {
                return 7;
            }
            else if (effectiveTime >= 180000) // 3+ minutes
            {
                return 6; // Target minimum depth
            }
            else if (effectiveTime >= 60000) // 1+ minute
            {
                return 5;
            }
            else if (effectiveTime >= 30000) // 30+ seconds
            {
                return 4;
            }
            else if (effectiveTime >= 10000) // 10+ seconds
            {
                return 3;
            }
            else // Bullet chess - emergency mode
            {
                return 2;
            }
        }

        /// <summary>
        /// Calculate appropriate depth for a fixed move time
        /// </summary>
        private static int DepthFromMoveTime(int moveTime)
        {
            if (moveTime >= 30000) // 30+ seconds per move
                return 10;
            else if (moveTime >= 15000) // 15+ seconds
                return 9;
            else if (moveTime >= 10000) // 10+ seconds
                return 8;
            else if (moveTime >= 5000) // 5+ seconds
                return 7;
            else if (moveTime >= 3000) // 3+ seconds
                return 6;
            else if (moveTime >= 1000) // 1+ second
                return 5;
            else if (moveTime >= 500) // 500+ ms
                return 4;
            else
                return 3;
        }

        /// <summary>
        /// Check if search should be extended due to tactical complexity
        /// </summary>
        public static bool ShouldExtendSearch(int nodes, int timeUsed, int timeAllocated, bool inCheck = false)
        {
            // Don't extend if we're already over time
            if (timeUsed >= timeAllocated * 1.5)
                return false;

            // Extend if we're in check and haven't used much time
            if (inCheck && timeUsed < timeAllocated * 0.8)
                return true;

            // Extend if node count suggests we're in a tactical position
            // (high branching factor indicates complex position)
            double averageNps = nodes / Math.Max(1.0, timeUsed / 1000.0);
            if (averageNps < 10000 && timeUsed < timeAllocated * 0.9) // Low NPS = complex position
                return true;

            return false;
        }

        /// <summary>
        /// Parse UCI time control parameters
        /// </summary>
        public static TimeControl ParseTimeControl(string[] parts)
        {
            var timeControl = new TimeControl();

            for (int i = 0; i < parts.Length - 1; i++)
            {
                switch (parts[i])
                {
                    case "wtime":
                        if (int.TryParse(parts[i + 1], out int wtime))
                            timeControl.WhiteTime = wtime;
                        break;
                    case "btime":
                        if (int.TryParse(parts[i + 1], out int btime))
                            timeControl.BlackTime = btime;
                        break;
                    case "winc":
                        if (int.TryParse(parts[i + 1], out int winc))
                            timeControl.WhiteIncrement = winc;
                        break;
                    case "binc":
                        if (int.TryParse(parts[i + 1], out int binc))
                            timeControl.BlackIncrement = binc;
                        break;
                    case "movestogo":
                        if (int.TryParse(parts[i + 1], out int movestogo))
                            timeControl.MovesToGo = movestogo;
                        break;
                    case "movetime":
                        if (int.TryParse(parts[i + 1], out int movetime))
                            timeControl.MoveTime = movetime;
                        break;
                    case "depth":
                        if (int.TryParse(parts[i + 1], out int depth))
                            timeControl.Depth = depth;
                        break;
                    case "infinite":
                        timeControl.Infinite = true;
                        break;
                }
            }

            return timeControl;
        }
    }
}
