# Changelog

## Development update - 2026-11-07

- Replaced the perft placeholder with recursive legal move counting and
  exception-safe board restoration. Built-in startpos checks now assert counts.
- Added recursive search deadline checks; incomplete iterations retain the last
  completed result and unwind all board mutations, rather than overshoot until
  the next iterative-deepening boundary.
- Release build passed; startpos depth 4 = 197281, Kiwipete depth 3 = 97862.
  Two interrupted searches retained Kiwipete depth-2 count 2039 afterward.
- Added a repeatable PowerShell regression for these cases. Full search parity,
  UCI stop, and the time manager's existing 100ms minimum remain pending.

## [4.0.0-dev] - 2026-11-06

### Changed

- Default transposition search and UCI evaluation use the pinned V7P3R v18.3
  fast evaluation path, including exact material/PST weighting and phase gates.
- Candidate assembly and UCI identity now identify v4 development explicitly.
- Legacy evaluation helpers and alternate search bots remain available.

### Rationale

Separate language/runtime performance from heuristic differences. Extra Cobra
evaluation terms must not silently affect a purported V7P3R twin.

### Validation

- Release build succeeded with zero errors; existing .NET 6 EOL warning remains.
- 7,832 Python-oracle cases matched all components and final evaluation scores.
- UCI identity and readiness passed. A non-book depth-2 search returned
  `a1a8` in the rook-versus-pawns smoke position.
- Editor checks reported no errors in the new evaluator or parity test.

### Known limitations

Search parity, complete move-generation regression testing, performance matches,
and live deployment are pending. The existing `perft 3` command returned 20
nodes rather than the standard 8,902; that smoke check is NOT a passed perft
regression. Investigate the existing command/core before strength testing.

Earlier release history remains in README, CHECKLIST, docs, and Git history;
this new changelog does not reconstruct unverified historical entries.
