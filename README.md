# Chess AI C++ Engine

A high-performance chess engine written in C++, converted from the original Python implementation while preserving the core architectural principles and design philosophy.

## Features

- **Negamax Search with Alpha-Beta Pruning**: Efficient tree search algorithm
- **Iterative Deepening**: Progressive depth increase for time management
- **Quiescence Search**: Tactical extension to avoid horizon effects
- **Move Ordering**: Advanced heuristics including:
  - Hash table moves (PV moves)
  - Captures ordered by Most Valuable Victim - Least Valuable Attacker (MVV-LVA)
  - Killer moves
  - Pawn advance bonuses
  - Penalty for squares attacked by opponent pawns
- **Transposition Table**: Hash-based position caching for efficiency
- **Game Phase Awareness**: Opening/middlegame/endgame detection
- **Specialized Endgame Evaluation**: King activity and opposition
- **UCI Protocol Compliance**: Standard chess engine interface

## Architecture

The engine maintains the same clean, modular design as the original Python version:

- `Position`: Board representation and move generation
- `Evaluator`: Position evaluation with material, piece-square tables, and game phase awareness
- `Searcher`: Search algorithms with multiple configurations
- `UCIEngine`: UCI protocol implementation for chess GUI integration

## Building

### Requirements

- C++17 compatible compiler (GCC 7+, Clang 5+, MSVC 2019+)
- CMake 3.16 or later

### Linux/Mac

```bash
# Clone and build
git clone <repository>
cd chess-ai/cpp

# Quick build
./build.sh

# Build with tests
./build.sh test

# Debug build
./build.sh debug

# Clean build
./build.sh clean release
```

### Windows

```cmd
# Clone and build
git clone <repository>
cd chess-ai\cpp

# Quick build
build.bat

# Build with tests
build.bat test

# Debug build
build.bat debug
```

### Manual CMake Build

```bash
mkdir build && cd build
cmake .. -DCMAKE_BUILD_TYPE=Release
cmake --build . --config Release
```

## Usage

### Command Line

```bash
# Run the engine
./build/bin/chess-ai

# The engine will start in UCI mode
# Send UCI commands:
uci
isready
position startpos moves e2e4 e7e5
go depth 6
```

### Chess GUIs

The engine is compatible with any UCI-compliant chess GUI:

- **Arena Chess**: Free Windows GUI
- **ChessBase**: Commercial Windows GUI
- **Scid vs. PC**: Cross-platform, open source
- **PyChess**: Linux GUI
- **Cute Chess**: Cross-platform tournament manager

## Performance

The C++ implementation provides significant performance improvements over the Python version:

| Configuration | Target Performance |
|---------------|-------------------|
| Simple Negamax | >2M nodes/sec |
| Alpha-Beta | >500K nodes/sec |
| Full Optimizations | >50K nodes/sec at depth 4 |

Actual performance will vary based on:
- CPU architecture (benefits from modern CPUs with fast memory)
- Compiler optimizations (-O3, -march=native)
- Position complexity
- Hash table size

## Engine Options

Configure via UCI setoption commands:

- `Hash`: Transposition table size in MB (1-1024, default: 32)
- `Threads`: Number of search threads (1 only, default: 1)
- `Debug`: Enable debug output (true/false, default: false)

## Testing

Run the test suite to verify correct implementation:

```bash
# Build and run tests
./build.sh test

# Or manually
cd build
./chess-ai-test
```

Tests verify:
- Position representation and FEN parsing
- Move generation correctness
- Evaluation function sanity
- Search algorithm performance
- Game phase detection

## Performance Benchmarks

Compare against the Python implementation:

```bash
# C++ engine
echo "go depth 4" | ./build/bin/chess-ai

# Python engine (from parent directory)
echo "go depth 4" | python interface.py
```

Expected improvements:
- **10-50x faster** node throughput
- **Reduced memory usage** through efficient data structures
- **Better cache locality** with array-based board representation

## Engine Strength

The engine implements the same evaluation and search algorithms as the Python version:

- **Material Evaluation**: Standard piece values
- **Piece-Square Tables**: Positional bonuses for piece placement
- **Game Phase Adaptation**: Different evaluation in opening/middlegame/endgame
- **Endgame Knowledge**: King activity and centralization

Estimated strength: ~1800-2000 Elo (depending on time control and hardware)

## Future Enhancements

Potential improvements while maintaining the engine's core philosophy:

- **Parallel Search**: Multi-threaded search
- **Opening Book**: Database of opening moves
- **Endgame Tablebases**: Perfect endgame knowledge
- **Advanced Evaluation**: Pawn structure, king safety, mobility
- **Search Extensions**: Check extensions, singular extensions
- **Time Management**: Better time allocation algorithms

## Development Notes

### Maintaining Python Compatibility

The C++ engine preserves the original design principles:

- Same evaluation function coefficients
- Identical search algorithm logic
- Equivalent move ordering heuristics
- Compatible UCI interface

### Code Organization

- **Headers in `include/`**: Clean interface definitions
- **Implementation in `src/`**: Efficient C++ implementations
- **Tests in `tests/`**: Verification of correctness
- **Build system**: CMake for cross-platform compilation

### Performance Philosophy

Optimizations focus on:
1. **Algorithmic efficiency** (alpha-beta, transposition table)
2. **Data structure optimization** (compact move representation)
3. **Cache-friendly access patterns**
4. **Avoiding premature micro-optimizations**

The goal is maintainable, fast code that preserves the engine's readable architecture.

## License

Same license as the original Python implementation.

## Contributing

Contributions welcome! Please:

1. Maintain compatibility with the Python version's core algorithms
2. Include tests for new features
3. Follow the existing code style
4. Update documentation for significant changes
