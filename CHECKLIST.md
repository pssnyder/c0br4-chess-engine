# ChessAI checklist

Purpose
-------
This file is the single place to track small tasks and priorities for the `chess-ai` engine. Add items, reorder, and check them off as we go. Keep each item short and actionable so we can implement exactly what you request.


**🚨 IMPORTANT - v2.1+ Engine Renaming**
----------------------------------------
I have decided to rename ChessAI to C0BR4. I have already updated all internal references to reflect this name change. However, current src files should be verified. We may also need to update any external references to git remotes. I also did not perform any rebuilds, those won't be necessary unless they are simple, for isntance the most recent version may be easy to rebuild with the correct name without disrupting its functionality, only renaming of EXE filenames, I will remapp the ChessAI results to the C0BR4 name in my results analyzer and will rename it in Arena moving forward. Here are some additional naming convention notes:
- Formal engine name: `C0BR4` (Replaces `ChessAI`)
- Executable naming example: `C0BR4_v2.1` (Replaces `ChessAI_v2.0`)
- Versioning scheme: `C0BR4 v2.1`
- Alternative syntax usage within code: `c0br4` (lowercase)
- Repository name: `c0br4-chess-ai`
- Contextual usage: `C0BR4ChessAI` (self-defining; this is a chess AI named C0BR4)

**v1.0 Complete:**

- ✅ **Basic UCI interface** — handles `uci`, `isready`, `position`, `go`, `stop`, `quit` and emits `info`/`bestmove`. Foundation implemented, move parsing needs work.
- ✅ **Basic project structure and foundation** — C# project using Chess Challenge API as reference, removed token limitations, basic UCI interface working
- ✅ **Move generation (pseudo-legal)** — Generates correct moves for all piece types, performance: 383K positions/sec baseline
- ✅ **Board management using simple data structures** — Make/unmake moves implemented with full state preservation
- ✅ **Legal move validation** — Filter pseudo-legal moves that leave king in check, complete attack detection for all piece types
- ✅ **Simple evaluation**: material score
- ✅ **Simple search**: Negamax search function as base algorithm. Tested on
- ✅ **Alpha-beta Pruning**: clear, readable implementation. Additional alpha-beta performance testing.
- ✅ **Move Ordering**: captures first and a small killer-move mechanism for storing beta cutoffs. Additional move priority for checks, pawn promotions, and a slight penalty for moving our piece to a square attacked by an opponent piece.
- ✅ **Staged Testing**: search, search + pruning, search + pruning + move ordering and should meet the following performance criteria on this test position:
   - FEN: r3k2r/p1ppqpb1/Bn2pnp1/3PN3/1p2P3/2N2Q2/PPPB1PpP/R3K2R w KQkq - 0 1
   - Search only: Expect ~3.5M positions, <1.5s
   - Search + pruning: Expect ~460k positions, <0.5s
   - Search + pruning + move ordering: Expect ~5000 positions, <0.05s
- ✅ **Quiescence Search**: for captures and checks until position becomes quiet. (should prevent sacrifices and material loss beyond initial depth, initial depth > 3 <= 10)
- ❌ ~~Small transposition table (FEN-keyed) for reuse.~~
- ✅ ~~Replace FEN-keyed~~ **TT w/ Zobrist Hashing**: faster, less memory.
- ✅ **Transposition Table Testing**: for more complex positions using zobrist hashing.
- ✅ **v1.0 Preparation**: `requirements.txt` and `README.md` updated.
- ✅ **Portable v1.0 Build**: GUI compatible, fully portable .exe build process for testing purpose in Arena interactive auto-tournaments.
- ✅ **Illegal Move Bug**: v1.0 engine provided illegal move in tournament play, got adjudicated. ~~Need to investigate move generation/legal validation logic.~~ **FIXED in v1.1**: Bug was in GenerateSlidingMoves() - diagonal move validation was incorrect, allowing bishop to move from c1 to a8 (not a diagonal). Fixed edge detection logic to properly validate diagonal moves.

Medium Priority Backlog v2.0
-----------------------------------

- ✅ **Time management** (wtime/btime/movetime/movestogo) — convert clock info into safe per-move time allocation with increment handling for 2/1 and 5/5 games.
- ✅ **Game phase detection** — tactical vs endgame differentiation based on piece count (14 pieces or less = endgame).
- ✅ **Enhanced Evaluation**: material + piece-square tables (PST) for p, n, b, q + game phase interpolation. King PST focuses on safety in middlegame, activity in endgame.
- ✅ **Piece Square Tables**: for pawn (middlegame focus on safe advance on non-castled side/endgame second rank and promotion focused), knight (middlegame center focused/endgame check focused), bishop (middlegame long diagonal focused/endgame check focused), and queen (middlegame focus on safe piece attacks/endgame check focused) for those pieces only
- ✅ **Rook Coordination**: middlegame focus on rank and file alignment, if already castled or castling rights already lost, give a rook incentive for being on the same file or rank. endgame should incentivise checks, adding an increased bonus for being on the opponents second rank during the endgame phase. ✅ v1.2
- ✅ **King Safety**: middlegame hiding focused, favoring positions where it has pawns in front of it or where it has minimally exposed sightlines (e.g. there are 8 possible sight lines to the king, the more of those lines that have one of our pieces blocking them the better, the more of those lines that fall off the board the better meaning we are protected on more sides and not exposed in the middle of the board, thus minimizing attack lanes) ✅ v1.2
- ✅ **King Endgame**: favor positions with opponents king near edge or corners of board. Keep our king relatively immobile until we hit the endgame, then keep our king close to the opponent king — material weighted endgame phase. ✅ v1.2
- ✅ **Castling Incentive**: should handle rook and king opening development, disables after castling or rights lost ✅ v1.2
- ✅ **Castling Rights**: preservation receives a slight bonus, castling moves are highly incentivized though to prevent preparation from overriding actual castling, and king should remain on rank 1 or rank 2 until King Endgame weight kicks in. ✅ v1.2
- ✅ **Opening book** for e4 or d4: London System, Caro-Kann, Vienna Gambit, Dutch Defense, should include a few moves (up to 5-8 moves) mainline only. ✅ v2.0

Low Priority Backlog
--------------------
- [ ] Enhanced Endgame Tactics: expand endgame piece profiles to include strategies for pawn advances, rook and queen positioning relative to opponent king ("closing the box" method), and knight/bishop coordination.
- [ ] Robust UCI options support — implement `setoption` and expose parameters like search depth, time, nodes, nodes/sec, value, mainline.
- [ ] Add perft function for move-generation verification.
- [ ] Enhanced opening book: expand to include more of the most popular "first moves" for white, and "best defenses" for black.


Parking Lot / Future Ideas
--------------------------

- [ ] Symmetrical tactical evaluation, positive for our moves resulting in, negative for opponent moves resulting in, pins, forks, skewers, discovered attacks, removing the guard, styles of tactics.
- [ ] Improve evaluation: pawn structure, board coverage, coordination/defense, and mobility.
- [ ] Integrate tablebase probing for simple endgames (optional depending on current king endgame performance).

Notes / conventions
-------------------
- Keep implementations human-readable, well-commented, and avoid premature micro-optimizations.
- Small, focused changes per commit. I will only implement items you check off or explicitly request next.

Add new items below this line.

----------------------------------------

