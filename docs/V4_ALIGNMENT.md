# C0BR4 v4 alignment

Version: 4.0.0-dev, unreleased. Evaluation port only; search parity is pending.

## Reference

The V7P3R deployment changelog identifies v18.3 as the strongest documented
historical candidate (110-day first run, 1722 peak, 1661 stable rating).
These historical ratings do not prove superiority in a controlled match.
The later deployment also suffered time forfeits; preserve its heuristics,
not its operational defects.

Use commit `98706258b8bd093b144f11d286df8b9d31d9df7d` in the V7P3R repository,
directory `lichess/engines/V7P3R_v18.3_20251229/src/`.
That commit's root source identifies as v18.2 and is NOT the selected archive.
Do not mix current Python dependencies into the reference.

## Implemented

The default transposition search and UCI `eval` use `ViperEvaluator`:

- Material: pawn 100, knight 300, bishop 325, rook 500, queen 900.
- Exact reference PSTs, with a1 at index zero and Black ranks mirrored.
- Endgame if neither side has a queen, or both sides have less than 800
  non-pawn/non-king material.
- Strategic terms only outside the endgame and from fullmove 10: open and
  semi-open rook files, king pawn shield, doubled pawns, passed pawns.
- Truncate `0.6 * PST + 0.4 * material + strategic` toward zero before applying
  the side-to-move sign.

The archived engine's default fast evaluation path adds no further terms.
Legacy Cobra evaluation classes and alternate search bots remain available,
but their extra terms are deliberately not mixed into default v4 evaluation.
This removes a source of confounding in the language comparison.

`eval` prints White-relative components and the side-to-move total.

## Validation

Build `src\C0BR4ChessEngine.csproj` in Release, then run with the project's
Python interpreter (python-chess is an existing requirement):

```powershell
python testing\test_v4_evaluation_parity.py `
  --engine src\bin\Release\net6.0\win-x64\C0BR4_v4.0.exe `
  --reference "<V7P3R repository>\lichess\engines\V7P3R_v18.3_20251229\src"
```

The test checks the reference evaluator SHA256, all usable piece/color/PST squares,
phase boundaries, asymmetric positions, deterministic legal playouts, mirrored
positions, both turns, individual components, and final scores. Synthetic
sparse inputs include kings to satisfy the FEN parser and test evaluation only,
not move legality. Pawns on promotion/back ranks are excluded.

## Remaining release blockers

- Search ordering, TT semantics, quiescence/check handling, repetition, mate
  distance and hard deadline enforcement have not been aligned.
- The perft placeholder has been replaced with legal recursive counting.
  Startpos through depth 4 and Kiwipete through depth 3 match known counts.
  Broader move-generation regression coverage remains necessary.
- The reference evaluation cache omits fullmove number although strategic
  evaluation depends on it. The C# port intentionally evaluates current phase
  directly; it does not reproduce that cache defect.
- Existing .NET 6 target is out of support; runtime modernization is separate.
- No production deployment, strength claim, or live comparison is approved by
  an evaluation parity result alone.

Pre-change source snapshot was hash-verified outside the repository. No
credentials, runtime configuration, active games, or VM state were included;
this is a local source freeze, not a live operational backup.

## Recorded results (2026-11-06)

Release build passed (existing .NET 6 EOL warning). All 7,832 oracle evaluations
matched material, PST, strategic, and total scores. UCI readiness and a non-book
depth-2 search passed. These checks do not establish search parity or strength.

## Search groundwork (2026-11-07)

Recursive deadline checks now abort incomplete iterations and preserve the last
completed result. Every search make/unmake pair restores the board in `finally`,
including quiescence. This does not yet implement asynchronous UCI `stop`.
The UCI time manager currently floors `movetime 1` to 100ms; full time-control
alignment remains pending. Deadline checks cannot preempt a single expensive
move-generation/evaluation operation.

Run `testing\test_v4_perft_deadline.ps1` after building to verify known perft
counts and board restoration following two interrupted 100ms searches.
