# ChessAI checklist

Purpose
-------
This file is the single place to track small tasks and priorities for the
`chess-ai` engine. Add items, reorder, and check them off as we go. Keep each
item short and actionable so we can implement exactly what you request.



High Priority Backlog (Must Go)
-------------------------------

- [ ] Board management using simple data structures, similar to python-chess usage (we may just need to build our own) — position application from `startpos`/`fen` and moves.
- [ ] User Interface using dark/grey dark mode theme with provided piece images.
- [ ] Simple evaluation: material + piece-square tables (PST) for pawn, knight, bishop, and queen + static exchange evaluation (medium priority) + castling (medium priority) + rook coordination (medium priority) + king endgame (medium priority).
- [ ] Simple search driver — iterative deepening wrapper calling negamax. Initial node and timing testing of base search to the specifications I provide earlier, which I assume you still can recall.
- [ ] Alpha-beta pruning option — clear, readable implementation. Additional alpha-beta performance testing.
- [ ] Basic move ordering: captures first and a small killer-move mechanism for storing beta cutoffs. Additional move priority for checks, pawn promotions, and a slight penalty for moving our piece to a square attacked by an opponent piece.
- [ ] Quiescence search for captures and checks until position becomes quiet.
- [ ] Small transposition table (FEN-keyed) for reuse.
- [ ] `requirements.txt` and `README.md` updated.
- [ ] Basic UCI interface — handles `uci`, `isready`, `position`, `go`, `stop`, `quit` and emits `info`/`bestmove`.


Medium Priority Backlog (Should Go)
-----------------------------------

- [ ] Time management (wtime/btime/movetime/movestogo) — convert clock info into safe per-move time allocation with increment handling for 2/1 and 5/5 games.
- [ ] Game phase detection — tactical vs endgame differentiation based on piece count (14 pieces or less = endgame).
- [ ] King endgame evaluation function, favor positions with opponents king near edge or corners of board. Keep our king relatively immobile until we hit the endgame, then keep our king close to the opponent king — material weighted endgame phase.
- [ ] Castling rights preservation receives a slight bonus, castling moves are highly incentivized, and king should remain on rank 1 or rank 2 until endgame, favoring positions where it has pawns in front of it or where it has minimally exposed sightlines (e.g. there are 8 possible sight lines to the king, the more of those lines that have one of our pieces blocking them the better, the more of those lines that fall off the board the better meaning we are protected on more sides and not exposed in the middle of the board).
- [ ] Rook coordination, if already castled or castling rights already lost, give a rook incentive for being on the same file or rank. Then add an increased bonus for being on the opponents second rank during the endgame phase.
- [ ] Visual testing GUI with comprehensive engine interface — universal chess engine testing framework with UCI compatibility, real-time move/evaluation logging, session-based data collection, and exportable test results. Providing all testing and live statistics just like the testing harness built earlier for the python version of the engine.
- Opening book for e4 or d4: London System, Caro-Kann, Vienna Gambit, Dutch Defense, mainlines only.

Low Priority Backlog
-----------------------

- [ ] Robust UCI options support — implement `setoption` and expose parameters like search depth, time, nodes, nodes/sec, value, mainline.
- [ ] Replace FEN-keyed TT with Zobrist hashing (faster, less memory).
- [ ] Transposition table testing for more complex positions using zobrist hashing.
- [ ] Unit tests and small test harness (perft tests, evaluation smoke tests) — (add `tests/`).
- [ ] Add perft function for move-generation verification.
- [ ] GUI compatible .bat or .exe for testing purposes.

Parking Lot / Future Ideas
- [ ] Symmetrical tactical evaluation, positive for our moves resulting in, negative for opponent moves resulting in, pins, forks, skewers, discovered attacks, removing the guard, styles of tactics.
- [ ] Improve evaluation: pawn structure, board coverage, coordination/defense, and mobility.
- [ ] Integrate tablebase probing for simple endgames (optional depending on current king endgame performance).

Notes / conventions
-------------------
- Keep implementations human-readable, well-commented, and avoid premature micro-optimizations.
- Small, focused changes per commit. I will only implement items you check off or explicitly request next.

Add new items below this line.

----------------------------------------

