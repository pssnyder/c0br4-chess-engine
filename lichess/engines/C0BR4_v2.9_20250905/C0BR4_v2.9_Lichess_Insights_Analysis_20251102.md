# C0BR4 v2.9 Lichess Performance Analysis (2025-11-02)

## Performance by Opponent Strength

| Opponent Strength | Victory | Draw | Defeat | Games |
|------------------|---------|------|--------|-------|
| Much weaker      | 74.5%   | 19.4%| 6.1%   | 98    |
| Weaker           | 59.5%   | 31.6%| 8.9%   | 79    |
| Similar          | 29.5%   | 41%  | 29.5%  | 200   |
| Stronger         | 17.8%   | 27.9%| 54.3%  | 129   |
| Much stronger    | 2.1%    | 7.1% | 90.8%  | 239   |

## Performance by Time Control

| Variant   | Victory | Draw | Defeat | Games |
|-----------|---------|------|--------|-------|
| Bullet    | 27.1%   | 22.2%| 50.7%  | 284   |
| Blitz     | 26.4%   | 26.1%| 47.5%  | 394   |
| Rapid     | 36.4%   | 18.2%| 45.5%  | 132   |
| Classical | 20%     | 0%   | 80%    | 10    |

## Performance by Color

| Color | Victory | Draw | Defeat | Games |
|-------|---------|------|--------|-------|
| White | 27.4%   | 20.1%| 52.5%  | 398   |
| Black | 28.9%   | 26.1%| 45%    | 422   |

## Performance by Opening Family

| Opening Family        | Victory | Draw | Defeat | Games |
|----------------------|---------|------|--------|-------|
| Queen's Pawn Game    | 20.5%   | 18.1%| 61.4%  | 83    |
| Caro-Kann Defense    | 18.3%   | 45.1%| 36.6%  | 71    |
| Sicilian Defense     | 25.9%   | 17.2%| 56.9%  | 58    |
| Indian Defense       | 21.7%   | 23.9%| 54.3%  | 46    |
| Dutch Defense        | 42.9%   | 21.4%| 35.7%  | 42    |
| Zukertort Opening    | 28.6%   | 38.1%| 33.3%  | 42    |
| English Opening      | 30.8%   | 23.1%| 46.2%  | 39    |
| French Defense       | 21.1%   | 28.9%| 50%    | 38    |
| Scandinavian Defense | 12.1%   | 18.2%| 69.7%  | 33    |
| Horwitz Defense      | 44.4%   | 22.2%| 33.3%  | 27    |
| Van't Kruijs Opening | 56%     | 4%   | 40%    | 25    |
| Saragossa Opening    | 20%     | 44%  | 36%    | 25    |

## Performance by Opening Variation

| Opening Variation                           | Victory | Draw | Defeat | Games |
|--------------------------------------------|---------|------|--------|-------|
| Horwitz Defense: Other variations          | 44.4%   | 22.2%| 33.3%  | 27    |
| Queen's Pawn Game: Other variations        | 34.6%   | 42.3%| 23.1%  | 26    |
| Saragossa Opening: Other variations        | 20%     | 44%  | 36%    | 25    |
| Van't Kruijs Opening: Other variations     | 56%     | 4%   | 40%    | 25    |
| Sicilian Defense: Bowdler Attack           | 23.8%   | 19%  | 57.1%  | 21    |
| Caro-Kann Defense: Other variations        | 28.6%   | 38.1%| 33.3%  | 21    |
| Caro-Kann Defense: Masi Variation          | 19%     | 28.6%| 52.4%  | 21    |
| Queen's Pawn Game: Symmetrical Variation   | 15%     | 5%   | 80%    | 20    |
| Scandinavian Defense: Other variations     | 5.3%    | 21.1%| 73.7%  | 19    |
| Duras Gambit: Other variations             | 21.1%   | 10.5%| 68.4%  | 19    |
| French Defense: Knight Variation           | 21.1%   | 42.1%| 36.8%  | 19    |
| Sicilian Defense: Closed                   | 44.4%   | 5.6% | 50%    | 18    |

## Queen Trade Analysis

| Queen Trade    | Victory | Draw | Defeat | Games |
|----------------|---------|------|--------|-------|
| Queen trade    | 16.6%   | 32.2%| 51.2%  | 367   |
| No queen trade | 37.5%   | 15.9%| 46.6%  | 453   |

## Move Time by Piece

| Piece  | Avg Move Time (s) | Number of Moves |
|--------|------------------|-----------------|
| Pawn   | 0.87             | 5,077           |
| Knight | 1.07             | 6,153           |
| Bishop | 1.07             | 3,868           |
| Rook   | 0.89             | 6,233           |
| Queen  | 1.1              | 4,954           |
| King   | 0.81             | 4,300           |

## Time Pressure Analysis

| Time Remaining    | Avg Move Time (s) | Number of Moves |
|------------------|------------------|-----------------|
| ≤3% time left    | 2.74             | 55              |
| 3% to 10%        | 2.07             | 79              |
| 10% to 25%       | 2.44             | 222             |
| 25% to 50%       | 1.48             | 2,855           |
| ≥50% time left   | 0.89             | 27,374          |

## Move Time by Game Phase

| Game Phase | Avg Move Time (s) | Number of Moves |
|------------|------------------|-----------------|
| Opening    | 1.05             | 9,032           |
| Middlegame | 1.15             | 11,078          |
| Endgame    | 0.71             | 10,475          |

## Luck by Game Phase

| Game Phase | Luck % | Number of Moves |
|------------|--------|-----------------|
| Opening    | 30%    | 10              |
| Middlegame | 57.1%  | 7               |
| Endgame    | 25%    | 4               |

## Tactical Awareness by Piece

| Piece  | Tactical Awareness % | Number of Moves |
|--------|---------------------|-----------------|
| Pawn   | 100%                | 2               |
| Knight | 75%                 | 4               |
| Bishop | 50%                 | 2               |
| Rook   | 66.7%               | 6               |
| Queen  | 87.5%               | 8               |
| King   | 100%                | 1               |

## Tactical Awareness by Move Time

| Move Time    | Tactical Awareness % | Number of Moves |
|-------------|---------------------|-----------------|
| 0 to 1 sec  | 82.4%               | 17              |
| 1 to 3 sec  | 66.7%               | 6               |

## Accuracy by Piece

| Piece  | Accuracy % | Number of Moves |
|--------|-----------|-----------------|
| Pawn   | 90.8%     | 34              |
| Knight | 85.4%     | 46              |
| Bishop | 79.9%     | 26              |
| Rook   | 74.2%     | 35              |
| Queen  | 86.9%     | 41              |
| King   | 92%       | 13              |

## Accuracy by Game Phase

| Game Phase | Accuracy % | Number of Moves |
|------------|-----------|-----------------|
| Opening    | 81.3%     | 68              |
| Middlegame | 88.8%     | 88              |
| Endgame    | 78.7%     | 39              |

## Accuracy by Game Result

| Game Result | Accuracy % | Number of Moves |
|-------------|-----------|-----------------|
| Victory     | 87.1%     | 136             |
| Defeat      | 77.4%     | 59              |

## Material Imbalance by Winning Chances

| Winning Chances | Material Imbalance | Number of Moves |
|----------------|-------------------|-----------------|
| 0% to 10%      | -4.6              | 10              |
| 10% to 20%     | -2.09             | 23              |
| 20% to 30%     | -1.44             | 27              |
| 30% to 40%     | -0.18             | 28              |
| 40% to 50%     | -0.03             | 38              |
| 50% to 60%     | 0.5               | 24              |
| 60% to 70%     | 0.71              | 14              |
| 70% to 80%     | 1                 | 3               |
| 80% to 90%     | 0.63              | 16              |
| 90% to 100%    | 3.17              | 12              |

## Accuracy by Move Time

| Move Time     | Accuracy % | Number of Moves |
|--------------|-----------|-----------------|
| 0 to 1 sec   | 84.2%     | 123             |
| 1 to 3 sec   | 84.8%     | 62              |
| 3 to 5 sec   | 80.9%     | 8               |
| 5 to 10 sec  | 63.8%     | 1               |
| 10 to 30 sec | 56.2%     | 1               |

## Accuracy by Opening Variation

| Opening Variation                        | Accuracy % | Number of Moves |
|-----------------------------------------|-----------|-----------------|
| Dutch Defense: Other variations         | 86.4%     | 55              |
| Pirc Defense: Other variations          | 87.4%     | 41              |
| Sicilian Defense: Modern Variations     | 84.5%     | 31              |
| Queen's Pawn Game: Anti-Torre           | 70.8%     | 28              |
| Caro-Kann Defense: Other variations     | 89.9%     | 21              |
| Queen's Gambit Accepted: Winawer Defense| 85.3%     | 19              |

## Accuracy by Winning Chances

| Winning Chances | Accuracy % | Number of Moves |
|----------------|-----------|-----------------|
| 0% to 10%      | 95.6%     | 10              |
| 10% to 20%     | 91.2%     | 23              |
| 20% to 30%     | 87.4%     | 27              |
| 30% to 40%     | 75.5%     | 28              |
| 40% to 50%     | 74.6%     | 38              |
| 50% to 60%     | 81.1%     | 24              |
| 60% to 70%     | 90.8%     | 14              |
| 70% to 80%     | 80%       | 3               |
| 80% to 90%     | 94.6%     | 16              |
| 90% to 100%    | 99.1%     | 12              |

---

## Key Performance Insights

- **Strength-dependent performance**: Clear correlation between opponent strength and win rate (74.5% vs much weaker, 2.1% vs much stronger)
- **Time control preference**: Best performance in Rapid games (36.4% win rate), struggles in Classical (20% win rate)
- **Opening weaknesses**: Poor performance against Queen's Pawn Game (20.5% win rate) and Scandinavian Defense (12.1% win rate)
- **Queen trade disadvantage**: Significantly worse results when queens are traded (16.6% vs 37.5% win rate)
- **Move accuracy**: Highest accuracy with King moves (92%), lowest with Rook moves (74.2%)
- **Time management**: Best tactical awareness in fast moves (82.4% in 0-1 second moves)

## Critical Search Algorithm Defect - Accuracy Degradation with Time

**MAJOR DISCOVERY**: C0BR4's accuracy **degrades significantly** with longer thinking times:
- 0-1 sec: 84.2% accuracy (baseline)
- 1-3 sec: 84.8% accuracy (slight improvement)
- 3-5 sec: 80.9% accuracy (**4% drop**)
- 5-10 sec: 63.8% accuracy (**20% drop**)
- 10-30 sec: 56.2% accuracy (**28% drop**)

**Root Cause Analysis**:
1. **Quiescence Search Flaw**: Currently only searches captures/promotions but **excludes all checking moves**, causing tactical blindness in deeper searches
2. **Search Explosion**: No late move reductions or advanced pruning techniques - longer searches explore too many irrelevant branches
3. **Transposition Table Pollution**: Conflicting evaluations from different depths corrupt move selection
4. **Missing Check Extensions**: Critical tactical sequences involving checks are cut off at horizon

**v3.1 Priority Fixes**:
1. Enable checking moves in quiescence search (currently commented out in `GetTacticalMoves()`)
2. Implement late move reductions for non-critical moves at higher depths  
3. Add aspiration windows to improve search efficiency
4. Implement null move pruning for better cutoffs
5. Add futility pruning in near-leaf nodes

This explains why C0BR4 performs better in bullet chess - the shallow searches avoid the algorithm's deep-search weaknesses!

## Positional Play Deficiency - "Drawish Position Passivity"

**Pattern Identified**: C0BR4 shows **excellent accuracy at extremes** but **poor decision-making in balanced positions**:

**Strong Performance (Fighting/Converting)**:
- 0-10% winning chances: **95.6% accuracy** (excellent defense when losing)
- 90-100% winning chances: **99.1% accuracy** (perfect conversion of winning positions)

**Weak Performance (Imbalanced but Unclear)**:
- 30-40% winning chances: **75.5% accuracy** (20% drop from extremes)
- 40-50% winning chances: **74.6% accuracy** (25% drop from extremes)

**Strategic Implication**: C0BR4 is too passive in "slightly favorable but unclear" positions - exactly where games are won or lost. The engine maintains equality instead of creating winning chances.

**v3.1 Positional Enhancement Goals**:
1. **Drawishness Detection**: Identify positions with evaluation near 0.00 and encourage activity over equality
2. **Exchange Evaluation**: Bonus for piece trades that lead to favorable pawn endgames or king activity
3. **Activity Over Material**: In balanced positions, prioritize piece mobility and central control
4. **Pawn Break Incentives**: Encourage central pawn advances (d4/d5, e4/e5) to open closed positions
5. **King Activity Bonuses**: Reward active king play in simplified middlegames/endgames
6. **Anti-Stalemate Logic**: Prefer dynamic complexity over sterile draws in balanced positions

**Target**: Improve accuracy in 30-50% winning chance positions from ~75% to 85%+ by making C0BR4 more "fighting" and less "drawish".

## Opening Book Performance Analysis - Book Quality Crisis

**CRITICAL DISCOVERY**: Opening book coverage does NOT guarantee performance! Major quality issues identified:

### Opening Family Performance vs Book Status

| Opening Family | Victory % | Draw % | Defeat % | Games | Book Status | Assessment |
|---------------|-----------|--------|-----------|-------|-------------|------------|
| **Dutch Defense** | **42.9%** | 21.4% | 35.7% | 42 | ✅ Programmed | **EXCELLENT** |
| **Vienna Game** | **25%** | 0% | **75%** | 8 | ✅ Programmed | **DISASTER** |
| **Caro-Kann Defense** | **18.3%** | 45.1% | 36.6% | 71 | ✅ Programmed | **POOR** |
| **Queen's Pawn Game** | **20.5%** | 18.1% | 61.4% | 83 | ❌ Missing | **POOR** |

### Key Insights

1. **Dutch Defense Success**: 42.9% win rate proves good opening book lines work
2. **Vienna Game Disaster**: 25% win rate (75% defeat!) suggests **fundamentally flawed book lines**
3. **Caro-Kann Failure**: 18.3% win rate despite being programmed - **book quality issue**
4. **Queen's Pawn Gap**: Large sample (83 games) with poor results - **coverage gap**

## v3.1 Incremental Development Plan

**Following New Release Process: ONE CHANGE AT A TIME**

### Phase 1: Vienna Game Emergency Fix (Immediate)
- **Problem**: 75% defeat rate with programmed lines (catastrophic)
- **Sample Size**: Small (8 games) but 100% negative trend
- **Action**: Debug existing Vienna lines or replace with solid alternative
- **Expected Impact**: Eliminate worst-performing opening system

### Phase 2: Enable Quiescence Checking Moves (High Impact)
- **Problem**: Accuracy degrades with longer thinking time
- **Root Cause**: Checks disabled in quiescence search
- **Action**: One-line fix to enable checking moves in `GetTacticalMoves()`
- **Expected Impact**: Fix deep search tactical blindness

### Phase 3: Queen's Pawn Coverage (Largest Sample)
- **Problem**: 83 games with 20.5% win rate, no book coverage
- **Action**: Add basic Queen's Pawn Game responses
- **Expected Impact**: Improve most frequent problematic opening

### Phase 4: Caro-Kann Book Quality Review
- **Problem**: 71 games with 18.3% win rate despite book coverage
- **Action**: Analyze and improve existing Caro-Kann lines
- **Expected Impact**: Fix second-largest sample of poor results

**Development Principle**: Test each change independently with battle testing before proceeding to next phase.