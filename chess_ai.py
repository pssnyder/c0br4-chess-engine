import os
import chess
import chess.engine
import logging
import csv
import json
import datetime
import random

# Configuration
PLAY_TYPE = 1  # 0 for player control, 1 for AI control
NUM_GAMES = 10
STRATEGIES = [0, 1, 2]  # Example strategies
LOG_FILE = f'./logs/chess_game_{datetime.datetime.now().strftime("%Y%m%d-%H%M%S")}.log'
CSV_RESULTS_FILE = f'./results/chess_ai_results_{datetime.datetime.now().strftime("%Y%m%d-%H%M%S")}.csv'
JSON_RESULTS_FILE = f'./results/chess_ai_results_{datetime.datetime.now().strftime("%Y%m%d-%H%M%S")}.json'

# Configure logging
logging.basicConfig(filename=LOG_FILE, level=logging.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s')

def define_strategy(strategy):
    """Translate the strategy into a user-friendly readable name."""
    if strategy == 0:
        return "Random Choice Strategy"
    elif strategy == 1:
        return "Strategic Move Priority (Captures & Center)"
    elif strategy == 2:
        return "Move Evaluation Strategy"
    else:
        logging.error(f"Strategy Definition Not Found for Strategy #{strategy}")
        return "Unknown Strategy"

class ChessGame:
    def __init__(self):
        self.board = chess.Board()
        self.engine = None  # We'll implement strategies without external engines
        self.game_over = False

    def display_board(self):
        print(self.board)

    def is_valid_move(self, move):
        try:
            chess.Move.from_uci(move)
            return True
        except:
            return False

    def make_move(self, move):
        self.board.push(chess.Move.from_uci(move))

    def get_possible_moves(self):
        return list(self.board.legal_moves)

    def ai_move(self, strategy):
        possible_moves = self.get_possible_moves()
        if not possible_moves:
            return None
            
        if strategy == 0:
            # Random strategy
            return random.choice(possible_moves)
        elif strategy == 1:
            # Simple minimax-like strategy: prioritize captures and central moves
            return self.simple_strategic_move(possible_moves)
        elif strategy == 2:
            # Simple evaluation strategy: pick the move that results in the best immediate position
            return self.evaluate_moves(possible_moves)
        else:
            return random.choice(possible_moves)
    
    def simple_strategic_move(self, possible_moves):
        """Simple strategy that prioritizes captures, then center control"""
        # First, try to find capturing moves
        captures = [move for move in possible_moves if self.board.is_capture(move)]
        if captures:
            return random.choice(captures)
        
        # If no captures, prefer moves towards the center
        center_squares = [chess.D4, chess.D5, chess.E4, chess.E5]
        center_moves = [move for move in possible_moves if move.to_square in center_squares]
        if center_moves:
            return random.choice(center_moves)
        
        # Otherwise, random move
        return random.choice(possible_moves)
    
    def evaluate_moves(self, possible_moves):
        """Evaluate moves based on simple piece values"""
        piece_values = {
            chess.PAWN: 1,
            chess.KNIGHT: 3,
            chess.BISHOP: 3,
            chess.ROOK: 5,
            chess.QUEEN: 9,
            chess.KING: 0
        }
        
        best_move = None
        best_score = float('-inf')
        
        for move in possible_moves:
            score = 0
            
            # Bonus for captures
            if self.board.is_capture(move):
                captured_piece = self.board.piece_at(move.to_square)
                if captured_piece:
                    score += piece_values.get(captured_piece.piece_type, 0)
            
            # Small bonus for center control
            center_squares = [chess.D4, chess.D5, chess.E4, chess.E5]
            if move.to_square in center_squares:
                score += 0.5
            
            if score > best_score:
                best_score = score
                best_move = move
        
        return best_move if best_move else random.choice(possible_moves)

    def play_turn(self, strategy):
        if PLAY_TYPE == 0:
            self.display_board()
            move = input("Enter your move (e.g., 'e2e4'): ")
            if self.is_valid_move(move):
                self.make_move(move)
            else:
                print("Invalid move. Try again.")
        else:
            move = self.ai_move(strategy)
            if move:
                self.make_move(move)
            else:
                logging.error("No valid move found by AI")

    def play_game(self, strategy):
        while not self.board.is_game_over():
            self.play_turn(strategy)
        result = self.board.result()
        logging.debug(f"Game result: {result}")
        return result

def simulate_games(num_games, strategy):
    results = []
    for i in range(num_games):
        logging.debug(f"Starting game {i+1} with strategy {define_strategy(strategy)}")
        game = ChessGame()
        result = game.play_game(strategy)
        results.append({
            'game_number': i + 1,
            'strategy': strategy,
            'result': result,
            'moves': [move.uci() for move in game.board.move_stack]
        })
        logging.debug(f"Game {i+1} ended with result: {result}")
    return results

def save_results_to_csv(results):
    if not os.path.exists(CSV_RESULTS_FILE):
        with open(CSV_RESULTS_FILE, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(['Strategy', 'Game Number', 'Result', 'Moves'])
    with open(CSV_RESULTS_FILE, mode='a', newline='') as file:
        writer = csv.writer(file)
        for result in results:
            writer.writerow([result['strategy'], result['game_number'], result['result'], result['moves']])
    logging.debug(f'Results appended to {CSV_RESULTS_FILE}')

def save_results_to_json(results):
    if os.path.exists(JSON_RESULTS_FILE):
        with open(JSON_RESULTS_FILE, 'r') as file:
            existing_data = json.load(file)
    else:
        existing_data = []
    existing_data.extend(results)
    with open(JSON_RESULTS_FILE, 'w') as file:
        json.dump(existing_data, file, indent=4)
    logging.debug(f'Results saved to {JSON_RESULTS_FILE}')

if __name__ == '__main__':
    logging.debug(f"#### Beginning {len(STRATEGIES)} Strategy Testing ####")
    print("#### Beginning", len(STRATEGIES), "Strategy Tests ####")
    for strategy in STRATEGIES:
        logging.debug(f"## ## Simulation beginning for {NUM_GAMES} games using strategy #{strategy} ## ##")
        results = simulate_games(NUM_GAMES, strategy)
        save_results_to_csv(results)
        save_results_to_json(results)
        avg_result = sum(1 if result['result'] == '1-0' else 0 for result in results) / NUM_GAMES
        logging.debug(f"## ## Simulation ended for {NUM_GAMES} games using strategy #{define_strategy(strategy)} ## ##")
        print(f"Strategy Deployed: {define_strategy(strategy)}, Games Simulated: {NUM_GAMES}, Win Rate: {round(avg_result * 100, 2)}%")
    print("#### All Simulations Have Ended ####")
    logging.debug(f"#### #### All Simulations Have Ended #### ####")