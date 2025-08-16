# ChessAI checklist

Purpose
-------
This file is the single place to track small tasks and priorities for the
`chess-ai` engine. Add items, reorder, and check them off as we go. Keep each
item short and actionable so we can implement exactly what you request.

How to use
----------
- Mark completed items with `- [x]` and open items with `- [ ]`.
- Add a one-line note after the item in parentheses if you want to specify a
  file to edit or any constraints.

Implemented (done)
-------------------

- [x] Basic UCI interface (`interface.py`) — handles `uci`, `isready`, `position`, `go`, `stop`, `quit` and emits `info`/`bestmove`.
- [x] Board management using `python-chess` (`interface.py`) — position application from `startpos`/`fen` and moves.
- [x] Simple search driver (`chess_ai.py`) — iterative deepening wrapper calling negamax.
- [x] Negamax with alpha-beta pruning (`chess_ai.py`) — clear, readable implementation.
- [x] Quiescence search (captures only) (`chess_ai.py`).
- [x] Simple evaluation: material + piece-square tables (PST) (`chess_ai.py`).
- [x] Basic move ordering: captures first and a small killer-move mechanism (`chess_ai.py`).
- [x] Small transposition table (FEN-keyed) for reuse (`chess_ai.py`).
- [x] `requirements.txt` and `README.md` added.
- [x] User Interface Integration using chess_core module (`chess_core.py`) — enhanced testing interface with live search metrics, efficiency testing, and human vs engine play.
- [x] Code cleanup — removed duplicate functions from `chess_ai.py`, verified all modules work together.

High Priority Backlog (next items to work on)
--------------------------------------------

- [x] Time management (wtime/btime/movetime/movestogo) — convert clock info into safe per-move time allocation with increment handling for 2/1 and 5/5 games.
- [x] Game phase detection (`chess_ai.py`) — tactical vs endgame differentiation based on piece count (14 pieces or less = endgame).
- [x] Endgame evaluation function, favor positions with opponents king near edge or corners of board and keeping our king close to opponent king (`chess_ai.py`) — material weighted endgame phase.
- [x] Modify move ordering to prioritize checks, captures, pawn promotions, and penalize for moving our piece to a square attacked by an opponent pawn (`chess_ai.py`).
- [x] Visual testing GUI with comprehensive engine interface (`chess_testing_gui.py`) — universal chess engine testing framework with UCI compatibility, real-time move/evaluation logging, session-based data collection, and exportable test results.

Medium Priority Backlog
-----------------------

- [ ] Robust UCI options support (`interface.py`) — implement `setoption` and expose parameters like search depth, time, nodes, nodes/sec, value, mainline.
- [ ] Replace FEN-keyed TT with Zobrist hashing (faster, less memory) — (would edit `chess_ai.py`).
- [ ] Transposition table testing for more complex positions using zobrist hashing (`chess_ai.py`).
- [ ] Quiescence expansion to captures and checks until position becomes quiet, using time control to limit search instead of max depth (if efficiency testing goes well) (`chess_ai.py`).
- [ ] Unit tests and small test harness (perft tests, evaluation smoke tests) — (add `tests/`).
- [ ] Add perft function for move-generation verification (`chess_ai.py` or util module).
- [ ] Castling rights, castling, and king safety (`chess_ai.py`).

Lower Priority / Nice to Have
-----------------------------

- [ ] Benchmarks and logging (nodes/sec, time breakdown) — (add small runner script).
- [ ] GUI compatible .bat or .exe for testing purposes.

Parking Lot / Future Ideas
- [ ] Symmetrical tactical evaluation, positive for our moves resulting in, negative for opponent moves resulting in, pins, forks, skewers, discovered attacks, removing the guard, styles of tactics (`chess_ai.py`).
- [ ] Persistent opening book support / book reader (PGN or polyglot) — (new module / data files).
- [ ] Improve evaluation: pawn structure metrics, king safety, and mobility (`chess_ai.py`).
- [ ] Integrate tablebase probing for simple endgames (optional).

Notes / conventions
-------------------
- Keep implementations human-readable, well-commented, and avoid premature micro-optimizations.
- Small, focused changes per commit. I will only implement items you check off or explicitly request next.

Add new items below this line.

----------------------------------------

