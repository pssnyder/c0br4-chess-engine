# C0BR4 Chess Engine - AI Coding Guidelines

## Architecture Overview

C0BR4 is a UCI-compliant chess engine built in C# with a **dual-layer architecture**:
- **External API**: Legacy `Board` class for UCI compatibility 
- **Internal Engine**: Pure bitboard operations via `BitboardPosition` and `BitboardMoveGenerator`

This design allows UCI protocol compatibility while leveraging high-performance bitboard operations internally.

## Critical Initialization Pattern

**ALWAYS initialize `MagicBitboards.Initialize()` before ANY chess operations**. This was the root cause of illegal move bugs in earlier versions:

```csharp
// In Program.cs Main() - REQUIRED first line
MagicBitboards.Initialize();
```

## Project Structure & Boundaries

```
src/C0BR4ChessEngine/
├── Core/          # Board representation, move generation, pieces
├── Search/        # Search algorithms implementing IChessBot interface  
├── Evaluation/    # Position evaluation, piece-square tables, game phases
├── UCI/           # Universal Chess Interface protocol handling
├── Opening/       # Opening book and algebraic notation parsing
└── Testing/       # Performance benchmarks, validation, perft testing
```

**Key Principle**: Core bitboard operations are isolated from UCI protocol - the `Board` class acts as a facade that delegates to bitboard internals.

## Move Validation Patterns

The engine uses **defense-in-depth** validation to prevent illegal moves:

1. **Generation Level**: `BitboardMoveGenerator.GenerateLegalMoves()` - only produces legal moves
2. **Parsing Level**: `UCIEngine.ParseUciMove()` - validates UCI moves against legal move list
3. **Execution Level**: `Board.IsLegalMove()` - final validation before making moves

```csharp
// CORRECT: Always validate against legal moves list
var move = ParseUciMove(moveString);
if (move == null || !board.IsLegalMove(move.Value)) {
    // Handle invalid move
}
```

## Search Bot Architecture

All search algorithms implement `IChessBot` interface:
- `RandomBot` - Random legal move selection
- `SimpleSearchBot` - Basic negamax
- `AlphaBetaSearchBot` - Alpha-beta with move ordering
- `QuiescenceSearchBot` - Extended tactical search  
- `TranspositionSearchBot` - Full engine with TT (default)

**Search Pattern**: Use iterative deepening with UCI info output:

```csharp
for (int depth = 1; depth <= maxDepth; depth++) {
    var (move, score, pv) = SearchWithPV(board, depth);
    Console.WriteLine($"info depth {depth} score cp {score} pv {pvString}");
}
```

## Version & Build Conventions

- **Version Format**: `v3.0` (current with advanced endgame heuristics and enhanced perft)
- **Executable Naming**: `C0BR4_v3.0.exe` 
- **Build Target**: Single-file deployment with `PublishSingleFile=true`
- **Platform**: Primary target is `win-x64` but supports cross-platform

## Testing & Validation Workflows

### Performance Benchmarking
Use standard test position for consistency:
```csharp
// FEN: r3k2r/p1ppqpb1/Bn2pnp1/3PN3/1p2P3/2N2Q2/PPPB1PpP/R3K2R w KQkq - 0 1
PerformanceBenchmark.BenchmarkMoveGeneration(1000);
```

### Move Generation Testing
```bash
# Built-in commands via UCI
bench                    # Run performance benchmark
perft 4                 # Perft testing to depth 4
testmove e2e4           # Validate specific move
testall                 # Full bitboard validation
```

## UCI Protocol Patterns

**Key UCI Commands Implemented**:
- Standard: `uci`, `isready`, `position`, `go`, `quit`
- Engine-specific: `bench`, `perft`, `eval`, `debug`
- Move parsing validates against legal moves list (critical fix)

**Time Management**: Uses `TimeManager.ParseTimeControl()` with game phase awareness via `GamePhase.CalculatePhase()`.

## Development Workflow

1. **Check `CHECKLIST.md`** for current priorities and completed features
2. **Test against benchmarks** - performance regressions are tracked
3. **Version appropriately** - update `AssemblyVersion` in `.csproj`
4. **Validate move generation** - use built-in test commands
5. **Test in Arena GUI** - engine is designed for tournament play

## Critical Bug Prevention

- **Never** construct `Move` objects directly - use `ParseUciMove()` or legal move generation
- **Always** validate moves against `board.GetLegalMoves()` before execution
- **Remember** that `MagicBitboards.Initialize()` is required for bitboard operations
- **Use** defensive validation at UCI parsing layer - tournament failures occur from invalid moves

## Opening Book Integration

Opening book is separate from search - check `OpeningBook.GetOpeningMove()` before falling back to engine search:

```csharp
string? bookMove = OpeningBook.GetOpeningMove(board);
if (bookMove != null && OpeningBook.IsInOpeningPhase(board)) {
    // Use book move
} else {
    // Fall back to search
}
```

This architecture ensures the engine is immediately productive for UCI tournament play while maintaining clean separation between chess logic and protocol handling.