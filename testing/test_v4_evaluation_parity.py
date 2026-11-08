"""Compare the C0BR4 candidate against the pinned V7P3R v18.3 evaluator."""

import argparse
import hashlib
import importlib.util
import random
import re
import subprocess
from pathlib import Path

import chess


REFERENCE_SHA256 = "af554d64d6e9482f03740409a69e50182379eae1113f98ff585f1cc2b04ace35"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--engine", type=Path, required=True)
    parser.add_argument("--reference", type=Path, required=True,
                        help="Archived v18.3 src directory")
    args = parser.parse_args()
    source = args.reference / "v7p3r_fast_evaluator.py"
    if hashlib.sha256(source.read_bytes()).hexdigest() != REFERENCE_SHA256:
        raise AssertionError("Reference evaluator fingerprint does not match v18.3")
    spec = importlib.util.spec_from_file_location("v18_3_reference", source)
    if spec is None or spec.loader is None:
        raise RuntimeError("Cannot load reference evaluator")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    reference = module.V7P3RFastEvaluator()

    positions = [chess.Board()]
    # Queen present, no queens, and exactly 800 non-pawn material.
    for fen in [
        "6k1/8/8/8/8/8/8/6K1 w - - 0 10",
        "6k1/8/8/8/8/8/P7/6K1 w - - 0 10",
        "6k1/8/8/8/8/8/8/2B3K1 w - - 0 10",
        "r2q2k1/5ppp/8/3P4/2P5/2P5/5PPP/R2Q2K1 w - - 0 9",
        "6k1/8/8/8/8/8/8/RNQ3K1 w - - 0 10",
        "6k1/8/8/8/8/8/8/RN4KQ w - - 0 10",
        "6k1/8/8/8/8/8/8/RB4KQ w - - 0 10",
    ]:
        board = chess.Board(fen)
        for move_number in (9, 10, 11):
            board.fullmove_number = move_number
            positions.append(board.copy())

    # Cover every usable PST square, with kings required by the engine's FEN parser.
    # Synthetic boards here are intentional evaluator inputs, not search inputs.
    for piece_type in chess.PIECE_TYPES:
        for color in chess.COLORS:
            for square in chess.SQUARES:
                if piece_type == chess.PAWN and chess.square_rank(square) in (0, 7):
                    continue
                board = chess.Board.empty()
                board.set_piece_at(square, chess.Piece(piece_type, color))
                for king_color in chess.COLORS:
                    if board.king(king_color) is None:
                        king_square = next(
                            sq for sq in chess.SQUARES if board.piece_at(sq) is None
                        )
                        board.set_piece_at(king_square, chess.Piece(chess.KING, king_color))
                positions.append(board)

    rng = random.Random(183)
    board = chess.Board()
    for _ in range(1200):
        if board.is_game_over() or board.ply() >= 160:
            board = chess.Board()
        board.push(rng.choice(list(board.legal_moves)))
        positions.append(board.copy())

    cases = []
    for board in positions:
        for variant in (board, board.mirror()):
            for turn in chess.COLORS:
                variant = variant.copy()
                variant.turn = turn
                cases.append(variant)

    commands = ["uci", "isready"]
    expected = []
    for board in cases:
        commands.extend([f"position fen {board.fen()}", "eval"])
        expected.append((
            reference.evaluate_material(board),
            reference.evaluate_pst(board),
            reference.evaluate_strategic(board),
            reference.evaluate(board),
        ))
    commands.append("quit")
    result = subprocess.run(
        [str(args.engine.resolve())], input="\n".join(commands) + "\n",
        text=True, capture_output=True, timeout=120, check=False,
    )
    if result.returncode:
        raise AssertionError(
            f"Engine exited with code {result.returncode}; "
            f"stderr: {result.stderr[-3000:]}; stdout tail: {result.stdout[-1000:]}"
        )
    if "id name C0BR4 v4.0.0-dev" not in result.stdout or "readyok" not in result.stdout:
        raise AssertionError("UCI candidate identity/readiness check failed")
    actual = [
        tuple(map(int, match))
        for match in re.findall(
            r"info string evaluation material (-?\d+) pst (-?\d+) "
            r"strategic (-?\d+) score (-?\d+)", result.stdout,
        )
    ]
    if len(actual) != len(expected):
        raise AssertionError(
            f"Expected {len(expected)} evaluations, received {len(actual)}; "
            f"stderr: {result.stderr[:1000]}"
        )
    for board, wanted, received in zip(cases, expected, actual):
        if wanted != received:
            raise AssertionError(
                f"{board.fen()}: expected {wanted}, received {received}"
            )
    print(f"PASS: {len(cases)} evaluations, all components and totals match v18.3")


if __name__ == "__main__":
    main()
