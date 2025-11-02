# C0BR4 Chess Engine v2.9

A modern **production-ready** chess engine written in C# with UCI (Universal Chess Interface) support. Features **dual-layer architecture** with bitboard performance and proven tournament reliability.

## 🎯 Current Status: v2.9 Stable (Production)

**C0BR4 v2.9 is battle-tested and running 24/7 in cloud production**. This version provides proven tournament performance with comprehensive evaluation and reliable move generation.

### Production Deployment
- ✅ **Cloud Ready** - Native Linux build running on GCP
- ✅ **24/7 Operation** - Proven stability in production environment
- ✅ **Tournament Tested** - Active on Lichess with consistent ELO performance
- ✅ **Dual Platform** - Windows (development) + Linux (cloud deployment)

### Key Features
- ✅ **Bitboard Core** - High-performance move generation with magic bitboards
- ✅ **UCI Compliant** - Full Universal Chess Interface protocol support
- ✅ **Opening Book** - Integrated book with mainline variations
- ✅ **Advanced Search** - Alpha-beta with transposition table and move ordering
- ✅ **Multi-Phase Evaluation** - Game phase detection with specialized endgame logic

## 🚨 CRITICAL: Release Process & Regression Prevention

### v3.0 Regression Incident (November 2025)
A major regression was discovered in v3.0 that caused severe chess playing degradation:
- **Issue**: Engine played 2.Qh5 (Patzer-Parnham Opening) with positive evaluation
- **Impact**: Lost games against v2.9, time forfeit issues
- **Resolution**: Emergency rollback to v2.9 completed
- **Lesson**: Performance optimization ≠ chess strength improvement

### New Development Guidelines (Post-v3.0)

#### ⚠️ MANDATORY Release Process
1. **Incremental Changes Only** - One component at a time
2. **Battle Testing Required** - Engine vs engine validation before release
3. **Opening Book Validation** - Test in actual gameplay, not isolation
4. **Evaluation Accuracy Check** - Verify position assessments make chess sense
5. **Production Alignment** - Keep development synchronized with cloud deployment

#### 🔒 Quality Gates
- [ ] **Unit Tests Pass** - All move generation and evaluation tests
- [ ] **Performance Benchmarks** - No regression in search speed
- [ ] **Battle Test vs Previous Version** - Minimum 10 games, no strength loss
- [ ] **Opening Validation** - Test common opening positions
- [ ] **Time Management** - No forfeit or timeout issues
- [ ] **Cloud Build Compatibility** - Linux build tested

#### 📋 Pre-Release Checklist
```bash
# 1. Battle test against previous version
./test_engine_battle.sh v2.9 v3.1-candidate

# 2. Validate opening book behavior
echo "position startpos moves e2e4 e7e5" | ./engine_test
# Should NOT suggest Qh5!

# 3. Test both build configurations
dotnet build -c Release        # Windows dev
dotnet build -c Release-Linux  # Cloud deployment

# 4. Performance regression check
./performance_benchmark.sh     # Compare to baseline

# 5. Document changes and version bump
# Update CHECKLIST.md, version numbers, and changelog
```

### Version Rollback Procedure
If regression is discovered:
1. **Immediate Action**: Revert to last known good version
2. **Preserve Failed Version**: Backup for analysis
3. **Document Issues**: Detailed root cause analysis
4. **Staged Recovery**: Gradual reintroduction of improvements

## Engine Features

### Dual-Layer Architecture
- **External API**: Legacy `Board` class for UCI compatibility
- **Internal Engine**: Pure bitboard operations via `BitboardPosition` and `BitboardMoveGenerator`
- **Magic Bitboards**: High-performance attack/move generation
- **Defense-in-Depth**: Multiple validation layers prevent illegal moves

### Search & Algorithm
- **TranspositionSearchBot**: Main search engine with alpha-beta pruning
- **Iterative Deepening**: Depth 4-6 default, scalable to depth 10+
- **Move Ordering**: MVV-LVA with killer moves and history heuristic
- **Quiescence Search**: Tactical position handling to avoid horizon effects
- **Transposition Table**: Zobrist hashing with configurable size

### Evaluation System
- **Multi-Component**: Material + Piece-Square Tables + Advanced features
- **Game Phase Detection**: Opening/Middlegame/Endgame transitions
- **King Safety**: Position-based safety evaluation
- **Endgame Specialization**: Advanced endgame heuristics
- **Opening Book**: Integrated book with 5-8 move depth

### Production Infrastructure
- **Cloud Deployment**: Native Linux builds for GCP
- **Docker Support**: Containerized deployment with monitoring
- **24/7 Operation**: Proven stability in production environment
- **Automated Backups**: Game records and configuration preservation

## Performance & Validation

### Current Benchmarks (v2.9)
Test position: `r3k2r/p1ppqpb1/Bn2pnp1/3PN3/1p2P3/2N2Q2/PPPB1PpP/R3K2R w KQkq - 0 1`

| Component | Performance | Status |
|-----------|-------------|---------|
| **Move Generation** | 7000+ NPS | ✅ Optimized |
| **Search Depth** | 4-6 default, 10+ capable | ✅ Scalable |
| **Opening Book** | 5-8 moves deep | ✅ Integrated |
| **Time Management** | No forfeit issues | ✅ Reliable |
| **Cloud Performance** | 24/7 operation | ✅ Proven |

### Quality Assurance

#### Built-in Testing Commands
```bash
# Performance benchmark
echo "bench" | ./C0BR4_v2.9.exe

# Move generation validation
echo "perft 4" | ./C0BR4_v2.9.exe

# Position evaluation test
echo "eval" | ./C0BR4_v2.9.exe

# Opening book validation
echo "testbook" | ./C0BR4_v2.9.exe
```

#### Regression Testing
```bash
# Battle test against previous version
./engine_battle.sh C0BR4_v2.9.exe C0BR4_v3.0-candidate.exe

# Opening position validation
echo "position startpos moves e2e4 e7e5" | ./C0BR4_v2.9.exe
# Should suggest: Bc4, Nf3, d3, f4, etc.
# Should NEVER suggest: Qh5 (regression indicator)

# Time management test
echo "go movetime 1000" | ./C0BR4_v2.9.exe
# Should respond within time limit
```

### Production Monitoring
- **Cloud Instance**: c0br4-production-bot on GCP
- **Uptime**: 99.95% SLA with auto-restart
- **Performance**: Real-time monitoring via unified dashboard
- **Game Records**: Persistent storage with automated backups

## Build Instructions

### Prerequisites
- **.NET 6.0 SDK** - Cross-platform development framework
- **Git** - Version control for source management

### Development Build (Windows)
```bash
# Clone and setup
git clone <repository-url>
cd cobra-chess-engine/src

# Install dependencies and initialize magic bitboards
dotnet restore

# Debug build for development
dotnet build

# Release build for testing
dotnet build -c Release

# Run from build output
cd bin/Release/net6.0/win-x64
./C0BR4_v2.9.exe
```

### Production Build (Linux Cloud)
```bash
# Build for Linux deployment
dotnet build -c Release-Linux

# Output location
cd bin/Release-Linux/net6.0/linux-x64
./C0BR4_v2.9  # Native Linux executable
```

### Version Management
```bash
# Update version for new release
# 1. Edit src/C0BR4ChessEngine.csproj:
#    <AssemblyName>C0BR4_v2.10</AssemblyName>
#    <Version>2.10.0</Version>

# 2. Update CHECKLIST.md with new features
# 3. Follow release process checklist above
# 4. Battle test before deployment
```

### Cloud Deployment
The engine runs in production on Google Cloud Platform:
```bash
# Production environment
- Instance: c0br4-production-bot (e2-micro)
- OS: Alpine Linux + .NET 6.0 Runtime
- Engine: C0BR4_v2.9 (native Linux build)
- Management: Docker containerized
- Monitoring: Unified dashboard with V7P3R bot
```

## Development Workflow

### Project Structure
```
src/
├── C0BR4ChessEngine/
│   ├── Core/               # Bitboard engine (BitboardPosition, Move, etc.)
│   ├── Search/             # Search algorithms (TranspositionSearchBot, etc.)
│   ├── Evaluation/         # Position evaluation (SimpleEvaluator, GamePhase, etc.)
│   ├── Opening/            # Opening book (OpeningBook, AlgebraicNotation)
│   ├── UCI/                # UCI protocol (UCIEngine)
│   └── Testing/            # Benchmarks and validation
├── C0BR4ChessEngine.csproj # Project configuration
└── Program.cs              # Entry point with MagicBitboards.Initialize()

deployed/                   # Production deployment snapshots
├── v2.9/                  # Current production version
└── ...

rollback_backup/           # Emergency rollback preservations
├── v3.0_failed/          # Failed v3.0 implementation
└── ...
```

### Critical Development Rules

#### 🚨 ALWAYS Initialize Magic Bitboards
```csharp
// In Program.cs Main() - REQUIRED first line
MagicBitboards.Initialize();
```
**Failure to initialize causes illegal move generation!**

#### 🔍 Testing Workflow
```bash
# 1. Unit testing
dotnet test

# 2. Performance benchmark
echo "uci\nposition fen r3k2r/p1ppqpb1/Bn2pnp1/3PN3/1p2P3/2N2Q2/PPPB1PpP/R3K2R w KQkq - 0 1\ngo depth 4\nquit" | ./C0BR4_v2.9.exe

# 3. Opening validation
echo "uci\nposition startpos moves e2e4 e7e5\ngo depth 2\nquit" | ./C0BR4_v2.9.exe
# Expected: Should suggest reasonable moves (Bc4, Nf3, d3, etc.)
# NEVER: Should not suggest Qh5 (Patzer-Parnham Opening)

# 4. Battle testing
# Run engine vs engine matches before any release
```

#### 📋 Feature Development Process
1. **Check CHECKLIST.md** - Review current priorities
2. **One Component at a Time** - Isolate changes for easier debugging
3. **Battle Test Early** - Test against previous version frequently
4. **Document Changes** - Update relevant documentation
5. **Version Appropriately** - Follow semantic versioning

## Engine Architecture

### Search Algorithm
- **Negamax framework** with alpha-beta pruning
- **Iterative deepening** for time management
- **Principal variation** tracking
- **Move ordering**: Captures > Promotions > Checks > Quiet moves
- **Quiescence search**: Extends search in tactical positions
- **Transposition table**: Zobrist hashing with 100K entries

### Position Evaluation  
- **Material counting** with standard values
- **Piece-square tables** for positional factors
- **Game phase detection** based on material count
- **King safety** considerations
- **Endgame specialization** for simplified positions

### Move Generation
- **Pseudo-legal generation** with legal filtering
- **Special moves**: Castling, en passant, promotion
- **Attack detection** for check/checkmate/stalemate
- **Move validation** with king safety verification

## Performance Tuning

### Engine Settings
The engine uses fixed parameters optimized for rapid play:
- **Search depth**: 4 ply default
- **Transposition table**: 100,000 entries
- **Move ordering**: MVV-LVA + basic heuristics
- **Quiescence depth**: Unlimited (until quiet)

### Hardware Requirements
- **Minimum**: 1 core, 100MB RAM
- **Recommended**: 2+ cores, 512MB+ RAM  
- **Performance scales** with CPU speed and memory bandwidth

## UCI Commands

### Basic UCI Interface
```bash
# Initialize engine
uci                                    # Engine info and options
isready                               # Confirm ready state

# Position setup
position startpos                     # Starting position
position startpos moves e2e4 e7e5     # Apply moves
position fen [fen-string]             # Custom position

# Search commands
go depth 5                           # Fixed depth search
go movetime 5000                     # Time-limited search (5 seconds)
go wtime 60000 btime 60000          # Time control (1 minute each)

# Engine management
stop                                 # Stop current search
quit                                # Exit engine
```

### Engine-Specific Commands
```bash
# Performance testing
bench                               # Performance benchmark
perft 4                            # Move generation validation

# Analysis tools
eval                               # Static evaluation of current position
debug on/off                       # Toggle debug output

# Opening book
testbook                           # Validate opening book integration
testmove e2e4                     # Test specific move validity
```

### Chess GUI Integration
Compatible with UCI chess GUIs:
- **Arena Chess GUI** - Windows tournament play
- **Cute Chess** - Cross-platform engine testing
- **Lucas Chess** - Training and analysis
- **BanksiaGUI** - Tournament management
- **ChessBase/Fritz** - Commercial analysis

## Roadmap & Version History

### Current Status: v2.9 (Stable Production)
- ✅ **Proven Tournament Performance** - Active on Lichess
- ✅ **Cloud Deployment** - 24/7 operation on GCP
- ✅ **Opening Book Integration** - 5-8 move coverage
- ✅ **Advanced Search** - Transposition table with move ordering
- ✅ **Multi-Phase Evaluation** - Game phase specialization

### Version History
| Version | Key Features | Status |
|---------|-------------|---------|
| **v2.9** | Production stability, cloud deployment | ✅ **Current** |
| v2.8 | Enhanced UCI output, PV display | ✅ Stable |
| v2.7 | Bug fixes, move validation | ✅ Stable |
| v2.6 | Performance optimizations | ✅ Stable |
| v2.5 | Illegal move elimination | ✅ Stable |
| ~~v3.0~~ | ❌ **REGRESSION** - Emergency rollback | 🚫 **Avoided** |

### Future Development (v3.1+)
**Following Incremental Development Principles:**

#### Short Term Goals
- [ ] **Enhanced Time Management** - Tournament-grade time allocation
- [ ] **Improved Move Ordering** - History heuristic and killer moves
- [ ] **Opening Book Expansion** - Deeper coverage with more variations
- [ ] **Evaluation Tuning** - Fine-tune piece-square tables

#### Medium Term Goals  
- [ ] **Tactical Pattern Recognition** - Pin, fork, skewer detection
- [ ] **Advanced Endgame** - Specialized endgame evaluation
- [ ] **Multi-Threading** - Parallel search implementation
- [ ] **Syzygy Tablebase** - Perfect endgame knowledge

#### Quality Assurance Requirements
- **Battle Testing**: Each version must not lose strength vs previous
- **Regression Prevention**: Automated testing for move quality
- **Cloud Compatibility**: Linux builds tested before release
- **Documentation**: Complete changelog and migration notes

### Lessons Learned (v3.0 Incident)
- **Performance ≠ Strength**: Fast move generation doesn't guarantee good moves
- **Opening Book Critical**: Search evaluation must handle book fallback scenarios
- **Incremental Development**: Large rewrites introduce too many variables
- **Production Alignment**: Development must stay synchronized with cloud deployment

## License & Acknowledgments

### License
[Specify license terms - MIT, GPL, etc.]

### Acknowledgments
- **Sebastian Lague**: Chess programming tutorial foundation
- **Chess Programming Wiki**: Algorithms and implementation techniques  
- **Magic Bitboards**: High-performance move generation methodology
- **UCI Protocol**: Universal Chess Interface specification
- **Lichess**: Platform for tournament testing and validation
- **Chess Community**: Feedback and testing support

### Contributors
- **Core Development**: C0BR4 Chess Engine Project
- **Production Infrastructure**: Cloud deployment and monitoring
- **Quality Assurance**: Battle testing and regression prevention

---

**Current Version**: v2.9 (Production Stable)  
**Deployment**: 24/7 Cloud Operation on GCP  
**Last Updated**: November 2025  
**Next Version**: v3.1 (Incremental Improvements)

For development questions, check **CHECKLIST.md** for current priorities.  
For deployment issues, see **c0br4-lichess-engine/** folder documentation.  
For rollback procedures, see **ROLLBACK_COMPLETED.md** for emergency protocols.
