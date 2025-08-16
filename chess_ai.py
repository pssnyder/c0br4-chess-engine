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
# Game phase detection
# ---------------------------------------------------------------------------

def detect_game_phase(board: chess.Board) -> str:
	"""Detect game phase based on piece count.
	
	Returns 'opening', 'middlegame', or 'endgame'.
	- Endgame: 14 pieces or less on board (excluding kings) - takes priority
	- Opening: First ~10 moves AND not endgame
	- Middlegame: Everything else
	"""
	# Count all pieces except kings
	piece_count = len(board.piece_map()) - 2  # subtract 2 kings
	
	# Endgame detection: 14 pieces or less (excluding kings) - highest priority
	if piece_count <= 14:
		return 'endgame'
	
	# Early opening detection (only if not endgame)
	if board.fullmove_number <= 10:
		return 'opening'
	
	# Everything else is middlegame
	return 'middlegame'


# ---------------------------------------------------------------------------
# Endgame evaluation helpers
# ---------------------------------------------------------------------------

def distance_to_center(square: int) -> int:
	"""Calculate Manhattan distance from square to center of board."""
	rank = chess.square_rank(square)
	file = chess.square_file(square)
	center_dist = max(abs(rank - 3.5), abs(file - 3.5))
	return int(center_dist)


def distance_between_squares(sq1: int, sq2: int) -> int:
	"""Calculate Manhattan distance between two squares."""
	rank1, file1 = chess.square_rank(sq1), chess.square_file(sq1)
	rank2, file2 = chess.square_rank(sq2), chess.square_file(sq2)
	return abs(rank1 - rank2) + abs(file1 - file2)


def evaluate_endgame(board: chess.Board) -> int:
	"""Specialized endgame evaluation.
	
	Favors:
	- Opponent king near edges/corners
	- Our king close to opponent king
	- Basic material advantage
	"""
	score = 0
	
	# Find kings
	white_king_sq = board.king(chess.WHITE)
	black_king_sq = board.king(chess.BLACK)
	
	if white_king_sq is None or black_king_sq is None:
		return 0  # Invalid position
	
	# Distance from opponent king to center (higher is better for us)
	white_king_center_dist = distance_to_center(white_king_sq)
	black_king_center_dist = distance_to_center(black_king_sq)
	
	# Encourage pushing opponent king to edges
	score += black_king_center_dist * 10  # Good for white
	score -= white_king_center_dist * 10  # Bad for white
	
	# Encourage king proximity (opposition and mating patterns)
	king_distance = distance_between_squares(white_king_sq, black_king_sq)
	if king_distance > 0:
		# Closer is generally better in endgames (for opposition, support, mating)
		score += (8 - king_distance) * 5
	
	return score
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
	Includes basic game phase awareness and endgame evaluation.
	"""
	score = 0
	game_phase = detect_game_phase(board)
	
	# Material and piece-square evaluation
	for sq, piece in board.piece_map().items():
		v = PIECE_VALUES.get(piece.piece_type, 0)
		
		# Apply piece-square table bonus
		pst_bonus = pst_value(piece, sq)
		
		# In endgame, reduce PST influence for non-king pieces
		# and increase king activity importance
		if game_phase == 'endgame':
			if piece.piece_type == chess.KING:
				pst_bonus *= 2  # King activity more important in endgame
			else:
				pst_bonus = int(pst_bonus * 0.5)  # Reduce PST influence for other pieces
		
		v += pst_bonus
		score += v if piece.color == chess.WHITE else -v

	# Add specialized endgame evaluation
	if game_phase == 'endgame':
		endgame_bonus = evaluate_endgame(board)
		score += endgame_bonus

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
	"""Order moves: checks, captures (by victim value), promotions, killers, then others.
	
	Penalizes moves to squares attacked by opponent pawns.
	This is intentionally simple and deterministic.
	"""
	def move_score(mv: chess.Move) -> int:
		score = 0
		
		# Make the move temporarily to check for checks
		board.push(mv)
		is_check = board.is_check()
		board.pop()
		
		# 1. Checks get highest priority
		if is_check:
			score += 30000
		
		# 2. Captures: sort by victim value minus attacker value (MVV-LVA)
		if board.is_capture(mv):
			victim = board.piece_at(mv.to_square)
			attacker = board.piece_at(mv.from_square)
			victim_value = PIECE_VALUES.get(victim.piece_type, 0) if victim else 0
			attacker_value = PIECE_VALUES.get(attacker.piece_type, 0) if attacker else 0
			# MVV-LVA: prefer captures of high-value victims with low-value attackers
			score += 20000 + victim_value - (attacker_value // 10)
		
		# 3. Pawn promotions
		if mv.promotion is not None:
			promotion_value = PIECE_VALUES.get(mv.promotion, 0)
			score += 25000 + promotion_value
		
		# 4. Killer moves get a moderate bonus
		for depth, killers in killer_table.items():
			if killers and mv in killers:
				score += 5000
				break
		
		# 5. Penalty for moving to squares attacked by opponent pawns
		if _is_square_attacked_by_pawn(board, mv.to_square, not board.turn):
			score -= 500
		
		# 6. Small bonus for advancing pawns (encourage development)
		moving_piece = board.piece_at(mv.from_square)
		if moving_piece and moving_piece.piece_type == chess.PAWN:
			if board.turn == chess.WHITE:
				score += chess.square_rank(mv.to_square) * 10
			else:
				score += (7 - chess.square_rank(mv.to_square)) * 10
		
		return score

	return sorted(moves, key=move_score, reverse=True)


def _is_square_attacked_by_pawn(board: chess.Board, square: int, by_color: chess.Color) -> bool:
	"""Check if a square is attacked by a pawn of the given color."""
	# Get pawn attack squares for the given color
	if by_color == chess.WHITE:
		# White pawns attack diagonally up
		attack_squares = []
		if chess.square_rank(square) > 0:  # Not on rank 1
			if chess.square_file(square) > 0:  # Not on a-file
				attack_squares.append(square - 9)  # Down-left
			if chess.square_file(square) < 7:  # Not on h-file
				attack_squares.append(square - 7)  # Down-right
	else:
		# Black pawns attack diagonally down
		attack_squares = []
		if chess.square_rank(square) < 7:  # Not on rank 8
			if chess.square_file(square) > 0:  # Not on a-file
				attack_squares.append(square + 7)  # Up-left
			if chess.square_file(square) < 7:  # Not on h-file
				attack_squares.append(square + 9)  # Up-right
	
	# Check if any of those squares contain an opponent pawn
	for attack_sq in attack_squares:
		piece = board.piece_at(attack_sq)
		if piece and piece.piece_type == chess.PAWN and piece.color == by_color:
			return True
	
	return False


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


def negamax_simple(
	board: chess.Board,
	depth: int,
	nodes: List[int],
	stop_check: Callable[[], bool],
) -> Tuple[int, List[str]]:
	"""Simple minimax (negamax) without alpha-beta pruning.
	
	This is for performance comparison to show the benefit of alpha-beta.
	"""
	if stop_check():
		return 0, []

	nodes[0] += 1

	if depth == 0:
		# Basic evaluation without quiescence for simplicity in base test
		return evaluate(board), []

	best_score = -9999999
	best_line: List[str] = []

	# No move ordering in base search
	moves = list(board.legal_moves)

	for mv in moves:
		board.push(mv)
		score, line = negamax_simple(board, depth - 1, nodes, stop_check)
		score = -score
		board.pop()

		if score > best_score:
			best_score = score
			best_line = [mv.uci()] + line

		if stop_check():
			break

	return best_score, best_line


def negamax_ab_basic(
	board: chess.Board,
	depth: int,
	alpha: int,
	beta: int,
	nodes: List[int],
	killer_table: Dict[int, List[Optional[chess.Move]]],
	ply: int,
	stop_check: Callable[[], bool],
) -> Tuple[int, List[str]]:
	"""Negamax with alpha-beta pruning but without transposition table.
	
	This shows the benefit of alpha-beta pruning alone.
	"""
	if stop_check():
		return 0, []

	nodes[0] += 1

	if depth == 0:
		# Use quiescence for fair comparison
		qs = quiescence(board, alpha, beta, nodes, stop_check)
		return qs, []

	best_score = -9999999
	best_line: List[str] = []

	# Basic move ordering (captures first, but no advanced ordering)
	moves = list(board.legal_moves)
	moves = _order_moves_basic(board, moves)

	for mv in moves:
		board.push(mv)
		score, line = negamax_ab_basic(board, depth - 1, -beta, -alpha, nodes, killer_table, ply + 1, stop_check)
		score = -score
		board.pop()

		if score > best_score:
			best_score = score
			best_line = [mv.uci()] + line

		alpha = max(alpha, score)
		if alpha >= beta:
			# Alpha-beta cutoff
			break

		if stop_check():
			break

	return best_score, best_line


def _order_moves_basic(board: chess.Board, moves: List[chess.Move]) -> List[chess.Move]:
	"""Basic move ordering: captures first, sorted by victim value."""
	captures = []
	non_captures = []

	for move in moves:
		if board.is_capture(move):
			# Get victim value for sorting
			victim = board.piece_at(move.to_square)
			victim_value = PIECE_VALUES.get(victim.piece_type, 0) if victim else 0
			captures.append((move, victim_value))
		else:
			non_captures.append(move)

	# Sort captures by victim value (descending)
	captures.sort(key=lambda x: x[1], reverse=True)
	ordered_captures = [move for move, _ in captures]

	return ordered_captures + non_captures


def search(
	board: chess.Board,
	depth: Optional[int] = 3,
	time_limit: Optional[float] = None,
	info_callback: Optional[Callable] = None,
	stop_event=None,
	config: str = "full"  # "base", "alphabeta", or "full"
) -> Optional[str]:
	"""Iterative deepening driver exposing a minimal but useful feature set.

	info_callback(depth=..., score=..., pv=[...], nodes=..., time_ms=..., nps=...)
	
	config options:
	- "base": Simple minimax without alpha-beta pruning
	- "alphabeta": Alpha-beta pruning enabled, basic move ordering
	- "full": Alpha-beta + advanced move ordering + all optimizations
	"""
	if depth is None:
		depth = 3

	start_time = time.time()
	best_move = None
	total_nodes = 0

	# transposition table and killer moves (only for appropriate configs)
	tt: Dict[str, TTEntry] = {} if config == "full" else {}
	killer_table: Dict[int, List[Optional[chess.Move]]] = {} if config in ["alphabeta", "full"] else {}

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

		if config == "base":
			# Simple minimax without alpha-beta
			score, pv = negamax_simple(board, d, nodes, stop_check)
		elif config == "alphabeta":
			# Alpha-beta with basic move ordering
			score, pv = negamax_ab_basic(board, d, -9999999, 9999999, nodes, killer_table, ply=0, stop_check=stop_check)
		else:  # "full"
			# Full optimizations
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

