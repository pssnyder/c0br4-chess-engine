"""
Simple UCI-compliant interface for the Chess AI project.

This module is the primary entry point. It speaks a subset of UCI, manages
the board (via python-chess), and delegates search to the `chess_ai` module.

The `chess_ai.search` function is expected to accept a board, depth/time
limits, an info callback (to emit UCI `info` lines), and a stop_event.
This file provides a working stub that prints detailed `info` lines from
the search callback and returns `bestmove` when the search finishes.
"""

import sys
import threading
import time
import logging

import chess

import chess_ai

logging.basicConfig(level=logging.INFO)


class UCIEngine:
	def __init__(self):
		self.board = chess.Board()
		self.stop_event = threading.Event()
		self.search_thread = None
		# store last known search stats
		self._nodes = 0

	def send(self, line: str):
		# centralize stdout writes so tests can capture them if needed
		print(line, flush=True)

	def send_info(self, *, depth=None, score=None, pv=None, nodes=None, time_ms=None, nps=None):
		# Build a simple UCI info line from supplied data
		parts = ["info"]
		if depth is not None:
			parts += ["depth", str(depth)]
		if score is not None:
			# score expected to be centipawns (int) or mate distance tuple
			if isinstance(score, tuple) and score[0] == 'mate':
				parts += ["score", "mate", str(score[1])]
			elif isinstance(score, (int, float)):
				parts += ["score", "cp", str(int(score))]
		if nodes is not None:
			parts += ["nodes", str(nodes)]
		if nps is not None:
			parts += ["nps", str(nps)]
		if time_ms is not None:
			parts += ["time", str(int(time_ms))]
		if pv:
			parts += ["pv"] + pv

		self.send(" ".join(parts))

	def handle_uci(self):
		self.send("id name ChessAI-Stub")
		self.send("id author pssnyder")
		# minimal options can be extended later
		self.send("uciok")

	def handle_isready(self):
		self.send("readyok")

	def handle_ucinewgame(self):
		self.board.reset()

	def handle_position(self, tokens):
		# tokens is list after the word 'position'
		# examples: startpos moves e2e4 e7e5
		# or: fen <fenstring> moves ...
		if not tokens:
			return
		i = 0
		if tokens[0] == "startpos":
			self.board.set_fen(chess.STARTING_FEN)
			i = 1
		elif tokens[0] == "fen":
			# fen string may be 6 tokens long
			fen = " ".join(tokens[1:7])
			try:
				self.board.set_fen(fen)
			except Exception as e:
				logging.exception("Invalid FEN: %s", fen)
			i = 7

		# optional moves
		if i < len(tokens) and tokens[i] == "moves":
			for mv in tokens[i + 1 :]:
				try:
					move = chess.Move.from_uci(mv)
					if move not in self.board.legal_moves:
						# try to be forgiving: push if legal by promotion syntax
						pass
					self.board.push(move)
				except Exception:
					logging.exception("Failed to apply move: %s", mv)

	def _calculate_time_allocation(self, movetime=None, wtime=None, inc=0, movestogo=None):
		"""Calculate safe time allocation for this move with increment handling.
		
		Designed for 2/1 and 5/5 time controls (2 min + 1 sec increment, etc.)
		"""
		if movetime is not None:
			# Fixed time per move - use 90% to leave buffer
			return (movetime / 1000.0) * 0.9
		
		if wtime is None:
			# No time control specified
			return None
			
		wtime_sec = wtime / 1000.0
		inc_sec = inc / 1000.0
		
		# For increment games, we can use more time early since we get increment back
		if inc_sec > 0:
			# With increment: use a fraction of remaining time + most of the increment
			# This works well for 2+1, 5+5 time controls
			if movestogo and movestogo > 0:
				# If moves to go is specified, divide remaining time
				base_time = wtime_sec / max(movestogo, 20)  # At least plan for 20 moves
			else:
				# Standard increment formula: plan for ~40 moves in middlegame
				moves_remaining = max(20, 40 - self.board.fullmove_number // 2)
				base_time = wtime_sec / moves_remaining
			
			# Add most of increment (90%) since we get it back
			time_for_move = base_time + (inc_sec * 0.9)
			
			# Safety: never use more than 1/8 of remaining time in one move
			max_time = wtime_sec / 8.0
			return min(time_for_move, max_time)
		else:
			# No increment: be more conservative
			if movestogo and movestogo > 0:
				# Divide remaining time by moves to go, with buffer
				return (wtime_sec / movestogo) * 0.8
			else:
				# Estimate moves remaining and be conservative
				moves_remaining = max(25, 50 - self.board.fullmove_number // 2)
				return (wtime_sec / moves_remaining) * 0.7

	def _search_and_report(self, params):
		# Called inside a background thread. Prepares a board copy for search
		board_copy = self.board.copy()
		depth = params.get("depth")
		movetime = params.get("movetime")
		wtime = params.get("wtime")
		btime = params.get("btime")
		winc = params.get("winc", 0)
		binc = params.get("binc", 0)
		movestogo = params.get("movestogo")

		time_limit = self._calculate_time_allocation(
			movetime=movetime,
			wtime=wtime if self.board.turn == chess.WHITE else btime,
			inc=winc if self.board.turn == chess.WHITE else binc,
			movestogo=movestogo
		)

		try:
			best = chess_ai.search(
				board=board_copy,
				depth=depth,
				time_limit=time_limit,
				info_callback=self.send_info,
				stop_event=self.stop_event,
			)
			if best is None:
				self.send("bestmove (none)")
			else:
				self.send(f"bestmove {best}")
		except Exception:
			logging.exception("Error during search")
			self.send("bestmove (none)")
		finally:
			# clear stop flag after search completes
			self.stop_event.clear()

	def handle_go(self, tokens):
		# parse limited subset of go params
		params = {}
		i = 0
		while i < len(tokens):
			t = tokens[i]
			if t in ("wtime", "btime", "winc", "binc", "movestogo", "movetime", "depth", "nodes"):
				if i + 1 < len(tokens):
					try:
						params[t] = int(tokens[i + 1])
					except ValueError:
						params[t] = None
				i += 2
			else:
				# skip unsupported tokens for now
				i += 1

		# normalize names
		if "depth" in params:
			params["depth"] = params["depth"]
		if "movetime" in params:
			params["movetime"] = params["movetime"]

		# start background search thread
		if self.search_thread and self.search_thread.is_alive():
			# already searching; ignore new go or stop previous
			self.send("info string already searching")
			return

		self.stop_event.clear()
		self.search_thread = threading.Thread(target=self._search_and_report, args=(params,))
		self.search_thread.daemon = True
		self.search_thread.start()

	def handle_stop(self):
		# signal search to stop and wait for thread
		self.stop_event.set()
		if self.search_thread:
			self.search_thread.join(timeout=1.0)

	def loop(self):
		# main read loop
		while True:
			try:
				line = sys.stdin.readline()
				if not line:
					break
				line = line.strip()
				if not line:
					continue
				parts = line.split()
				cmd = parts[0]
				args = parts[1:]

				if cmd == "uci":
					self.handle_uci()
				elif cmd == "isready":
					self.handle_isready()
				elif cmd == "ucinewgame":
					self.handle_ucinewgame()
				elif cmd == "position":
					self.handle_position(args)
				elif cmd == "go":
					self.handle_go(args)
				elif cmd == "stop":
					self.handle_stop()
				elif cmd == "quit":
					# try to stop any running search
					self.handle_stop()
					break
				elif cmd == "ponderhit":
					# not supported yet
					pass
				elif cmd == "setoption":
					# no options supported yet
					pass
				else:
					# unknown command - ignore or log
					logging.debug("Unknown UCI command: %s", cmd)

			except Exception:
				logging.exception("Error in main loop")


def main():
	engine = UCIEngine()
	engine.loop()


if __name__ == "__main__":
	main()

