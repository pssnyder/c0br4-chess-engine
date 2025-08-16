"""
Simple, human-readable search and evaluation core implementing the essentials
from your spec.

Features included (minimal, readable implementations):
- Negamax with alpha-beta pruning
- Iterative deepening driver
- Quiescence search (captures only)
- Basic move ordering: captures first (sorted by victim value) and killer moves
- Transposition table (simple dict keyed by FEN)
- Evaluation: material + piece-square tables (PST)

This file favors clarity over micro-optimizations. It is easy to read and
adapt as the next steps in development.
"""

import time
from typing import Optional, Callable, List, Tuple, Dict

import chess

# ---------------------------------------------------------------------------
# Simple evaluation components
# ---------------------------------------------------------------------------
# Material values (centipawns)
PIECE_VALUES = {
	chess.PAWN: 100,
	chess.KNIGHT: 320,
	chess.BISHOP: 330,
	chess.ROOK: 500,
	chess.QUEEN: 900,
	chess.KING: 20000,
}

# Piece-square tables (very small, illustrative). Index 0 = a1, 63 = h8.
# These are for white; for black we mirror the index.
PST = {
	chess.PAWN: [
		0, 0, 0, 0, 0, 0, 0, 0,
		5, 10, 10, -20, -20, 10, 10, 5,
		5, -5, -10, 0, 0, -10, -5, 5,
		0, 0, 0, 20, 20, 0, 0, 0,
		5, 5, 10, 25, 25, 10, 5, 5,
		10, 10, 20, 30, 30, 20, 10, 10,
		50, 50, 50, 50, 50, 50, 50, 50,
		0, 0, 0, 0, 0, 0, 0, 0,
	],
	chess.KNIGHT: [
		-50, -40, -30, -30, -30, -30, -40, -50,
		-40, -20, 0, 5, 5, 0, -20, -40,
		-30, 5, 10, 15, 15, 10, 5, -30,
		-30, 0, 15, 20, 20, 15, 0, -30,
		-30, 5, 15, 20, 20, 15, 5, -30,
		-30, 0, 10, 15, 15, 10, 0, -30,
		-40, -20, 0, 0, 0, 0, -20, -40,
		-50, -40, -30, -30, -30, -30, -40, -50,
	],
	chess.BISHOP: [
		-20, -10, -10, -10, -10, -10, -10, -20,
		-10, 0, 0, 0, 0, 0, 0, -10,
		-10, 0, 5, 10, 10, 5, 0, -10,
		-10, 5, 5, 10, 10, 5, 5, -10,
		-10, 0, 10, 10, 10, 10, 0, -10,
		-10, 10, 10, 10, 10, 10, 10, -10,
		-10, 5, 0, 0, 0, 0, 5, -10,
		-20, -10, -10, -10, -10, -10, -10, -20,
	],
	chess.ROOK: [
		0, 0, 5, 10, 10, 5, 0, 0,
		-5, 0, 0, 0, 0, 0, 0, -5,
		-5, 0, 0, 0, 0, 0, 0, -5,
		-5, 0, 0, 0, 0, 0, 0, -5,
		-5, 0, 0, 0, 0, 0, 0, -5,
		-5, 0, 0, 0, 0, 0, 0, -5,
		5, 10, 10, 10, 10, 10, 10, 5,
		0, 0, 0, 0, 0, 0, 0, 0,
	],
	chess.QUEEN: [
		-20, -10, -10, -5, -5, -10, -10, -20,
		-10, 0, 5, 0, 0, 0, 0, -10,
		-10, 5, 5, 5, 5, 5, 0, -10,
		0, 0, 5, 5, 5, 5, 0, -5,
		-5, 0, 5, 5, 5, 5, 0, -5,
		-10, 0, 5, 5, 5, 5, 0, -10,
		-10, 0, 0, 0, 0, 0, 0, -10,
		-20, -10, -10, -5, -5, -10, -10, -20,
	],
	chess.KING: [
		-30, -40, -40, -50, -50, -40, -40, -30,
		-30, -40, -40, -50, -50, -40, -40, -30,
		-30, -40, -40, -50, -50, -40, -40, -30,
		-30, -40, -40, -50, -50, -40, -40, -30,
		-20, -30, -30, -40, -40, -30, -30, -20,
		-10, -20, -20, -20, -20, -20, -20, -10,
		20, 20, 0, 0, 0, 0, 20, 20,
		20, 30, 10, 0, 0, 10, 30, 20,
	],
}


def pst_value(piece: chess.Piece, square: int) -> int:
	"""Return PST bonus for given piece and square (white perspective).

	For black pieces we mirror the table.
	"""
	table = PST.get(piece.piece_type)
	if table is None:
		return 0
	if piece.color == chess.WHITE:
		return table[square]
	else:
		# mirror for black
		return table[chess.square_mirror(square)]


def evaluate(board: chess.Board) -> int:
	"""Simple evaluation: material + PST, positive means advantage for White.

	This intentionally stays small and easy to read.
	"""
	score = 0
	for sq, piece in board.piece_map().items():
		v = PIECE_VALUES.get(piece.piece_type, 0)
		v += pst_value(piece, sq)
		score += v if piece.color == chess.WHITE else -v

	return score


# ---------------------------------------------------------------------------
# Search: negamax with alpha-beta, quiescence, iterative deepening
# ---------------------------------------------------------------------------

# Transposition table entry
class TTEntry:
	def __init__(self, depth: int, score: int, flag: str, bestmove: Optional[str]):
		self.depth = depth
		self.score = score
		self.flag = flag  # 'exact', 'lower', 'upper'
		self.bestmove = bestmove


def _fen_key(board: chess.Board) -> str:
	# Use the full FEN as a key for simplicity and readability
	return board.fen()


def _order_moves(board: chess.Board, moves: List[chess.Move], killer_table: Dict[int, List[Optional[chess.Move]]]) -> List[chess.Move]:
	"""Order moves: captures first (by victim value), then killers, then others.

	This is intentionally simple and deterministic.
	"""
	def move_score(mv: chess.Move) -> int:
		# captures: sort by victim value (higher first)
		if board.is_capture(mv):
			victim = board.piece_at(mv.to_square)
			attacker = board.piece_at(mv.from_square)
			victim_value = PIECE_VALUES.get(victim.piece_type, 0) if victim else 0
			attacker_value = PIECE_VALUES.get(attacker.piece_type, 0) if attacker else 0
			# prefer captures of high-value victims and low-value attackers
			return 10000 + victim_value - attacker_value

		# killer moves get a moderate bonus
		# check killer table for this depth (we don't have depth here, so it's approximate)
		for depth, killers in killer_table.items():
			if killers and mv in killers:
				return 5000

		# default: 0
		return 0

	return sorted(moves, key=move_score, reverse=True)


def quiescence(board: chess.Board, alpha: int, beta: int, nodes: List[int], stop_check: Callable[[], bool]) -> int:
	"""Search that extends only captures until position is quiet."""
	nodes[0] += 1
	if stop_check():
		return 0

	stand_pat = evaluate(board)
	if stand_pat >= beta:
		return beta
	if alpha < stand_pat:
		alpha = stand_pat

	for mv in board.legal_moves:
		if not board.is_capture(mv):
			continue
		board.push(mv)
		score = -quiescence(board, -beta, -alpha, nodes, stop_check)
		board.pop()

		if score >= beta:
			return beta
		if score > alpha:
			alpha = score

	return alpha


def negamax_ab(
	board: chess.Board,
	depth: int,
	alpha: int,
	beta: int,
	nodes: List[int],
	tt: Dict[str, TTEntry],
	killer_table: Dict[int, List[Optional[chess.Move]]],
	ply: int,
	stop_check: Callable[[], bool],
) -> Tuple[int, List[str]]:
	"""Negamax with alpha-beta, using a simple transposition table and killers.

	Returns (score, pv_list_of_uci_moves).
	"""
	if stop_check():
		return 0, []

	nodes[0] += 1

	key = _fen_key(board)
	tt_entry = tt.get(key)
	if tt_entry and tt_entry.depth >= depth:
		if tt_entry.flag == 'exact':
			return tt_entry.score, [tt_entry.bestmove] if tt_entry.bestmove else []
		elif tt_entry.flag == 'lower':
			alpha = max(alpha, tt_entry.score)
		elif tt_entry.flag == 'upper':
			beta = min(beta, tt_entry.score)
		if alpha >= beta:
			return tt_entry.score, [tt_entry.bestmove] if tt_entry.bestmove else []

	if depth == 0:
		# at leaf, use quiescence to avoid horizon effect
		qs = quiescence(board, alpha, beta, nodes, stop_check)
		return qs, []

	# keep original alpha for TT flag decisions later
	original_alpha = alpha

	best_score = -9999999
	best_line: List[str] = []

	# generate and order moves
	moves = list(board.legal_moves)
	moves = _order_moves(board, moves, killer_table)

	for mv in moves:
		board.push(mv)
		score, line = negamax_ab(board, depth - 1, -beta, -alpha, nodes, tt, killer_table, ply + 1, stop_check)
		score = -score
		board.pop()

		if score > best_score:
			best_score = score
			best_line = [mv.uci()] + line

		if score > alpha:
			alpha = score

		if alpha >= beta:
			# record killer if move is not a capture
			if not board.is_capture(mv):
				killers = killer_table.get(ply, [])
				if mv not in killers:
					killers = [mv] + (killers[:1] if killers else [])
					killer_table[ply] = killers
			break


	# store in transposition table
	if best_score <= original_alpha:
		flag = 'upper'
	elif best_score >= beta:
		flag = 'lower'
	else:
		flag = 'exact'

	tt[key] = TTEntry(depth=depth, score=best_score, flag=flag, bestmove=best_line[0] if best_line else None)

	return best_score, best_line


def search(
	board: chess.Board,
	depth: Optional[int] = 3,
	time_limit: Optional[float] = None,
	info_callback: Optional[Callable] = None,
	stop_event=None,
) -> Optional[str]:
	"""Iterative deepening driver exposing a minimal but useful feature set.

	info_callback(depth=..., score=..., pv=[...], nodes=..., time_ms=..., nps=...)
	"""
	if depth is None:
		depth = 3

	start_time = time.time()
	best_move = None
	total_nodes = 0

	# transposition table and killer moves
	tt: Dict[str, TTEntry] = {}
	killer_table: Dict[int, List[Optional[chess.Move]]] = {}

	def stop_check() -> bool:
		if stop_event is not None and getattr(stop_event, 'is_set', lambda: False)():
			return True
		if time_limit is not None and (time.time() - start_time) > time_limit:
			return True
		return False

	for d in range(1, depth + 1):
		if stop_check():
			break

		nodes = [0]
		t0 = time.time()

		score, pv = negamax_ab(board, d, -9999999, 9999999, nodes, tt, killer_table, ply=0, stop_check=stop_check)

		t1 = time.time()
		total_nodes += nodes[0]
		elapsed_ms = (t1 - start_time) * 1000.0
		nps = int(nodes[0] / max(1e-6, (t1 - t0))) if (t1 - t0) > 0 else 0

		if info_callback:
			info_callback(depth=d, score=score, pv=pv, nodes=total_nodes, time_ms=elapsed_ms, nps=nps)

		if pv:
			best_move = pv[0]

		if stop_check():
			break

	return best_move

