# Chess AI Engine - Version History

This directory contains versioned builds of our chess engine as we progress through development.

## Version Roadmap

### v0.0 - Foundation ✅
**Current Status: COMPLETE**
- [x] Basic project structure and UCI interface  
- [x] Core data types (Piece, Square, Move, Board)
- [x] FEN loading and basic board representation
- [x] Move generation (pseudo-legal moves) 
- [x] Random bot implementation
- [x] Performance benchmarking framework

**Performance Baseline:**
- Starting position: 383,436 positions/sec, 7.6M moves/sec
- Correctly generates 20 moves from starting position
- Basic UCI commands working (uci, isready, position, go, d, bench, perft)

### v1.0 - Core Engine (In Progress)
**High Priority Backlog Items:**
- [ ] Board management with move application (make/unmake moves)
- [ ] Legal move validation (king not in check)
- [ ] Simple evaluation: material + piece-square tables
- [ ] Simple search driver (negamax with iterative deepening)
- [ ] Alpha-beta pruning
- [ ] Basic move ordering (captures first, killer moves)
- [ ] Quiescence search for captures and checks
- [ ] Small transposition table (FEN-keyed)

### v2.0 - Advanced Features (Future)
**Medium Priority Backlog Items:**
- [ ] Time management (wtime/btime/movetime/movestogo)
- [ ] Game phase detection (opening/middlegame/endgame)
- [ ] Advanced evaluation (king safety, castling, rook coordination)
- [ ] Enhanced move ordering and search extensions
- [ ] Opening book integration
- [ ] Visual testing GUI

### v3.0+ - Optimizations (Future)
**Low Priority & Future Ideas:**
- [ ] Zobrist hashing for transposition table
- [ ] Parallel search capabilities  
- [ ] Advanced tactical evaluation
- [ ] Endgame tablebase integration

## Testing Against Previous Versions

Each major version can be tested against previous versions:
```bash
# Test current version against v0.0
./dist/ChessAI_v0.0/ChessEngine.exe  # Random bot
./dist/ChessAI_v1.0/ChessEngine.exe  # Future: Smart search
```

## Tournament Integration

Only major versions (v*.0) will be packaged for external chess GUIs like Arena Chess for tournament play.
