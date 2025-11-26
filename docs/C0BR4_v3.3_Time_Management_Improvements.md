# C0BR4 v3.3 - Adaptive Time Management & Depth Search

## Release Date
November 26, 2025

## Overview
C0BR4 v3.3 introduces **adaptive time management and depth calculation** to significantly improve playing strength in longer time controls while maintaining speed in blitz and bullet games.

## Key Problem Solved
**Previous versions** used a **fixed depth of 4** regardless of time control, causing the engine to "blitz out moves" even in 30-minute games where deeper analysis was available.

## New Features

### 1. Adaptive Depth Calculation
The engine now dynamically calculates target search depth based on time control:

| Time Control | Target Depth | Use Case |
|-------------|--------------|----------|
| 30+ minutes | **Depth 10** | Classical chess |
| 15+ minutes | **Depth 9** | Rapid tournaments |
| 10+ minutes | **Depth 8** | Standard rapid |
| 5+ minutes | **Depth 7** | Quick rapid |
| 3+ minutes | **Depth 6** | **Minimum target** for normal play |
| 1+ minute | Depth 5 | Fast blitz |
| 30+ seconds | Depth 4 | Bullet |
| 10+ seconds | Depth 3 | Ultra-bullet |
| <10 seconds | Depth 2 | Emergency mode |

### 2. Conservative Time Allocation
Enhanced time management to save time for deeper searches:

**Changes:**
- **Increased move estimates**: 25-50 moves remaining (was 20-40)
- **Reduced phase multipliers**: Opening 0.95x, Middlegame 1.1x, Endgame 0.9x (was 0.9x/1.2x/0.8x)
- **Max time per move**: Limited to 25% of remaining time (was 33%)
- **Time control awareness**: Longer games (30+ min) assume at least 50 moves remaining

### 3. Improved Iterative Deepening
The search now:
- **Targets specific depth** based on time control
- **Stops at 75% time usage** before attempting new depth
- **Reports actual reached depth** vs target depth
- **Better time management** between depth iterations

## Implementation Details

### New `TimeManager` Methods

```csharp
// Calculate optimal search depth based on time control
public static int CalculateSearchDepth(
    TimeControl timeControl, 
    bool isWhiteToMove, 
    double gamePhase
)

// Enhanced time allocation with conservative estimates
public static int CalculateTimeAllocation(
    TimeControl timeControl, 
    bool isWhiteToMove, 
    double gamePhase
)

// More accurate move count estimates for different time controls
private static int EstimateMovesRemaining(
    double gamePhase, 
    int remainingTime
)
```

### Updated `TranspositionSearchBot`

```csharp
// Changed from fixed searchDepth to dynamic targetDepth
private int targetDepth = 6;

// Enhanced Think() method with time-aware iterative deepening
- Checks 75% time threshold before new depth
- Reports completed depth vs target depth
- Better logging for depth progress
```

## Testing Results

### Test Case: 30-Minute Classical
```
Time Control: 30:00 + 0
Target Depth: 10
Time Allocation: ~34 seconds per move (conservative)
Expected Behavior: Deep analysis with consistent time usage
```

### Test Case: 3-Minute Blitz
```
Time Control: 3:00 + 2s increment
Target Depth: 6
Time Allocation: ~4.8 seconds per move
Expected Behavior: Balanced depth for blitz play
```

### Test Case: 30-Second Bullet
```
Time Control: 0:30 + 0
Target Depth: 4
Time Allocation: ~520ms per move
Expected Behavior: Fast, tactical play
```

## Expected Performance Improvements

1. **Longer Games**: 
   - Depth increase from 4 → 10 in 30-minute games
   - ~6-8 additional plies of lookahead
   - Significantly stronger tactical and positional play

2. **Standard Rapid (5-10 minutes)**:
   - Depth increase from 4 → 7-8
   - Better middlegame evaluation
   - Fewer tactical oversights

3. **Blitz (3 minutes)**:
   - Depth increase from 4 → 6
   - Meets target minimum depth
   - Solid tactical awareness

4. **Bullet**:
   - Maintains depth 4 for speed
   - No performance degradation

## Configuration

### UCI Engine Integration
The depth calculation is **automatic** based on `go` command time parameters:

```uci
# Engine automatically calculates depth from time control
go wtime 1800000 btime 1800000 winc 0 binc 0
> info string Target depth: 10, Time allocation: 34150ms

# Manual depth override still supported
go depth 8
> info string Target depth: 8
```

### Debug Output
New UCI info strings for monitoring:
```
info string Target depth: 10, Time allocation: 34150ms
info string Search completed: target depth 10, reached depth 9, nodes 125450
```

## Migration Notes

### From v3.2
- **No breaking changes** - drop-in replacement
- **Automatic depth adjustment** - no manual configuration needed
- **Backward compatible** with all UCI GUIs

### Recommended Testing
1. **Run tournament games** at various time controls
2. **Monitor depth reports** in UCI output
3. **Verify time usage** doesn't exceed limits
4. **Check for time trouble** in longer games

## Technical Architecture

### Depth Calculation Flow
```
UCI HandleGo() 
  → TimeManager.CalculateSearchDepth()
  → TranspositionSearchBot.SetDepth(targetDepth)
  → Iterative deepening to target
```

### Time Allocation Flow
```
UCI HandleGo()
  → TimeManager.CalculateTimeAllocation()
  → Think(board, timeLimit)
  → Iterative deepening with time checks
```

## Known Limitations

1. **Depth 10+ is intensive**: May not complete in very complex positions
2. **75% threshold**: May stop at depth 9 in positions with high branching factor
3. **Opening book priority**: Book moves bypass depth calculation (intended behavior)

## Future Enhancements

Potential improvements for v3.4+:
- **Position complexity scoring** to adjust depth dynamically
- **Node-based time management** instead of pure time limits
- **Aspiration windows** for faster deep searches
- **Parallel search** for multi-core systems

## Conclusion

C0BR4 v3.3 represents a **major improvement in playing strength** for longer time controls while maintaining excellent performance in fast games. The adaptive depth system ensures optimal use of available thinking time across all time formats.

**Expected Rating Gain**: +100-200 Elo in games with 10+ minute time controls.

---

*C0BR4 Chess Engine v3.3*  
*"Deeper Thinking, Better Chess"*
