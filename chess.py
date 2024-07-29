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
        return "Minimax Strategy"
    elif strategy == 2:
        return "Monte Carlo Tree Search"
    else:
        logging.error(f"Strategy Definition Not Found for Strategy #{strategy}")
        return "Unknown Strategy"

class ChessGame:
    def __init__(self):
        self.board = chess.Board()
        self.engine = chess.engine.SimpleEngine.popen_uci("/path/to/your/engine")
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
        if strategy == 0:
            return random.choice(self.get_possible_moves())
        elif strategy == 1:
            # Implement Minimax or other strategy
            return self.engine.play(self.board, chess.engine.Limit(time=0.1)).move
        elif strategy == 2:
            # Implement MCTS or other strategy
            pass

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
            self.make_move(move)

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