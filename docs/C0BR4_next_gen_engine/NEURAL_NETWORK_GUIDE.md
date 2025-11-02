# Neural Network Integration Guide

## 🧠 **Overview**

This document details the implementation of dual neural networks to enhance C0BR4's chess engine capabilities through **complexity assessment** and **opening/endgame mastery**.

## 🎯 **Dual Neural Network Strategy**

### **Philosophy: Grandmaster-Inspired Intelligence**

The system mimics grandmaster thinking patterns:
1. **Instant pattern recognition**: Known openings and endgames
2. **Complexity awareness**: Understanding when positions require deep thought
3. **Resource allocation**: Spending time where it matters most

---

## 🔍 **Network 1: Complexity Assessment Neural Network**

### **Purpose**
Provide real-time complexity scoring to guide engine resource allocation, mimicking a grandmaster's intuitive understanding of when a position requires deep analysis.

### **Architecture**
```yaml
Model Type: Feedforward Neural Network
Framework: TensorFlow Lite (for fast inference)
Input Size: 32 features
Hidden Layers: [64, 32, 16]
Output Size: 1 (complexity score 0.0-1.0)
Model Size: ~5-10MB
Inference Time: <50ms
```

### **Input Features (32 dimensions)**

#### **Basic Position Metrics (8 features)**
```python
basic_features = [
    'legal_moves_count',        # Total legal moves available
    'white_pieces_count',       # Active pieces on board
    'black_pieces_count',       # Active pieces on board  
    'total_piece_value',        # Sum of piece values
    'material_balance',         # White - Black material
    'piece_mobility_total',     # Sum of all piece mobilities
    'center_control_score',     # Control of e4,e5,d4,d5
    'king_safety_combined'      # Both kings' safety metrics
]
```

#### **Tactical Density Metrics (8 features)**
```python
tactical_features = [
    'pins_and_skewers_count',   # Tactical motifs present
    'forks_possible_count',     # Fork opportunities
    'discovered_attacks_count', # Discovered attack potential
    'double_attacks_count',     # Multiple attack patterns
    'hanging_pieces_count',     # Undefended pieces
    'attacked_pieces_value',    # Total value under attack  
    'defended_pieces_value',    # Total value defended
    'tactical_shots_available'  # Immediate tactical opportunities
]
```

#### **Positional Complexity (8 features)**
```python
positional_features = [
    'pawn_structure_tension',   # Pawn chain complexity
    'weak_squares_count',       # Holes in position
    'piece_coordination_score', # How well pieces work together
    'space_advantage',          # Territory control
    'bishop_pair_bonus',        # Bishop pair advantage
    'knight_outpost_count',     # Strong knight positions
    'rook_open_files',          # Open/semi-open files
    'queen_activity_score'      # Queen's influence on position
]
```

#### **Dynamic Evaluation Metrics (8 features)**
```python
dynamic_features = [
    'evaluation_volatility',    # How much eval changes with moves
    'forcing_moves_ratio',      # Percentage of forcing moves
    'quiet_moves_ratio',        # Percentage of quiet moves
    'capture_sequence_depth',   # Length of capture sequences
    'check_escape_options',     # King mobility under check
    'promotion_threats',        # Pawn promotion possibilities
    'endgame_proximity',        # How close to endgame
    'time_pressure_factor'      # Remaining time influence
]
```

### **Training Data Collection**

#### **Grandmaster Game Analysis**
```python
class ComplexityDataCollector:
    def __init__(self):
        self.games_database = load_grandmaster_games()
        self.position_analyzer = PositionAnalyzer()
        
    def collect_training_data(self):
        training_samples = []
        
        for game in self.games_database:
            for move_number, position in enumerate(game.positions):
                # Extract position features
                features = self.position_analyzer.extract_features(position)
                
                # Calculate complexity score from grandmaster behavior
                thinking_time = game.get_thinking_time(move_number)
                baseline_time = game.get_average_thinking_time()
                
                # Complexity score based on time allocation
                complexity_score = min(1.0, thinking_time / (baseline_time * 2))
                
                training_samples.append({
                    'features': features,
                    'complexity': complexity_score,
                    'game_outcome': game.result,
                    'player_rating': game.player_rating
                })
                
        return training_samples
```

#### **Engine Self-Play Data**
```python
class EngineComplexityCollector:
    def collect_engine_data(self):
        """Collect complexity data from engine self-play"""
        
        for position in self.test_positions:
            # Run engine at different time controls
            shallow_result = engine.search(position, time=0.1)
            deep_result = engine.search(position, time=2.0)
            
            # Calculate evaluation stability
            eval_change = abs(deep_result.score - shallow_result.score)
            search_stability = 1.0 - min(1.0, eval_change / 100)
            
            # Complexity inversely related to stability
            complexity_score = 1.0 - search_stability
            
            return {
                'position': position,
                'complexity': complexity_score,
                'eval_volatility': eval_change
            }
```

### **Model Training Pipeline**
```python
import tensorflow as tf
from tensorflow import keras

class ComplexityNeuralNetwork:
    def __init__(self):
        self.model = self.build_model()
        
    def build_model(self):
        model = keras.Sequential([
            keras.layers.Dense(64, activation='relu', input_shape=(32,)),
            keras.layers.Dropout(0.3),
            keras.layers.Dense(32, activation='relu'),
            keras.layers.Dropout(0.2), 
            keras.layers.Dense(16, activation='relu'),
            keras.layers.Dense(1, activation='sigmoid')  # Output 0-1
        ])
        
        model.compile(
            optimizer='adam',
            loss='mse',
            metrics=['mae']
        )
        
        return model
        
    def train(self, training_data):
        X = np.array([sample['features'] for sample in training_data])
        y = np.array([sample['complexity'] for sample in training_data])
        
        # Split data
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2)
        
        # Train model
        history = self.model.fit(
            X_train, y_train,
            validation_data=(X_test, y_test),
            epochs=100,
            batch_size=32,
            callbacks=[
                keras.callbacks.EarlyStopping(patience=10),
                keras.callbacks.ReduceLROnPlateau(patience=5)
            ]
        )
        
        return history
        
    def save_for_deployment(self, path):
        # Convert to TensorFlow Lite for fast inference
        converter = tf.lite.TFLiteConverter.from_keras_model(self.model)
        tflite_model = converter.convert()
        
        with open(f"{path}/complexity_model.tflite", "wb") as f:
            f.write(tflite_model)
```

### **Engine Integration**
```python
class ComplexityAwareEngine:
    def __init__(self, base_engine, complexity_model):
        self.engine = base_engine
        self.complexity_nn = complexity_model
        
    def search_with_complexity(self, position, base_time):
        # Get complexity score
        features = self.extract_position_features(position)
        complexity = self.complexity_nn.predict(features)
        
        # Adjust search parameters based on complexity
        adjusted_time = base_time * (1 + complexity * 1.5)  # Up to 2.5x time
        adjusted_depth = self.base_depth + int(complexity * 6)  # Up to +6 depth
        
        # Enhanced move ordering for complex positions
        if complexity > 0.7:
            self.enable_enhanced_move_ordering()
            self.increase_quiescence_depth()
            
        # Run search with adjusted parameters
        result = self.engine.search(
            position, 
            time=adjusted_time,
            depth=adjusted_depth
        )
        
        return result, complexity
```

---

## 🏆 **Network 2: Opening/Endgame Master Neural Network**

### **Purpose**
Provide instant, high-confidence move suggestions for opening and endgame positions, bypassing search entirely when possible.

### **Architecture**
```yaml
Model Type: Transformer-based Position Classifier + Move Predictor
Framework: TensorFlow / PyTorch
Input: Position representation (768 dimensions)
Output: Phase classification + Move prediction + Confidence scores
Model Size: ~50MB
Inference Time: <100ms
```

### **Input Representation**

#### **Position Encoding**
```python
class PositionEncoder:
    def encode_position(self, fen):
        """Convert FEN to neural network input"""
        
        # Board representation (8x8x12 = 768 features)
        # 12 channels: 6 piece types × 2 colors
        board_tensor = np.zeros((8, 8, 12))
        
        # Parse FEN and populate tensor
        position = chess.Board(fen)
        
        for square in chess.SQUARES:
            piece = position.piece_at(square)
            if piece:
                row, col = divmod(square, 8)
                channel = piece.piece_type - 1 + (6 if piece.color else 0)
                board_tensor[row][col][channel] = 1
                
        # Additional features
        metadata = [
            int(position.turn),                    # Side to move
            int(position.has_kingside_castling_rights(chess.WHITE)),
            int(position.has_queenside_castling_rights(chess.WHITE)),
            int(position.has_kingside_castling_rights(chess.BLACK)),
            int(position.has_queenside_castling_rights(chess.BLACK)),
            position.halfmove_clock / 50.0,        # 50-move rule progress
            min(position.fullmove_number / 100.0, 1.0)  # Game progress
        ]
        
        return np.concatenate([board_tensor.flatten(), metadata])
```

### **Training Data Sources**

#### **Opening Database Integration**
```python
class OpeningDatabaseProcessor:
    def __init__(self):
        self.opening_databases = [
            'lichess_opening_explorer',
            'chess_com_opening_book', 
            'eco_opening_codes',
            'grandmaster_repertoires'
        ]
        
    def create_opening_training_data(self):
        training_data = []
        
        for opening_line in self.get_opening_lines():
            for position, best_move in opening_line.positions:
                # Only include well-established theory
                if opening_line.game_count > 1000:
                    training_data.append({
                        'position': position,
                        'best_move': best_move,
                        'phase': 'opening',
                        'confidence': min(1.0, opening_line.win_rate),
                        'popularity': opening_line.game_count
                    })
                    
        return training_data
```

#### **Endgame Tablebase Integration**
```python
class EndgameTablebaseProcessor:
    def __init__(self):
        self.tablebases = ['syzygy_6men', 'syzygy_7men', 'nalimov_5men']
        
    def create_endgame_training_data(self):
        training_data = []
        
        for tablebase in self.tablebases:
            for position in tablebase.get_all_positions():
                if tablebase.is_winning(position):
                    best_move = tablebase.get_best_move(position)
                    moves_to_mate = tablebase.get_dtm(position)
                    
                    training_data.append({
                        'position': position,
                        'best_move': best_move,
                        'phase': 'endgame',
                        'confidence': 1.0,  # Tablebase = perfect
                        'moves_to_mate': moves_to_mate
                    })
                    
        return training_data
```

### **Model Architecture**
```python
class OpeningEndgameNetwork:
    def __init__(self):
        self.model = self.build_transformer_model()
        
    def build_transformer_model(self):
        # Input layer
        position_input = keras.layers.Input(shape=(775,))  # 768 + 7 metadata
        
        # Embedding and reshaping for transformer
        embedded = keras.layers.Dense(256, activation='relu')(position_input)
        reshaped = keras.layers.Reshape((64, 4))(embedded)  # 8x8 board attention
        
        # Transformer blocks
        attention_output = keras.layers.MultiHeadAttention(
            num_heads=8, key_dim=32
        )(reshaped, reshaped)
        
        attention_output = keras.layers.LayerNormalization()(attention_output)
        
        # Feed forward network
        ffn_output = keras.layers.Dense(128, activation='relu')(attention_output)
        ffn_output = keras.layers.Dense(64)(ffn_output)
        ffn_output = keras.layers.LayerNormalization()(ffn_output)
        
        # Global pooling
        pooled = keras.layers.GlobalAveragePooling1D()(ffn_output)
        
        # Output heads
        phase_output = keras.layers.Dense(3, activation='softmax', name='phase')(pooled)
        move_output = keras.layers.Dense(4096, activation='softmax', name='move')(pooled)
        confidence_output = keras.layers.Dense(1, activation='sigmoid', name='confidence')(pooled)
        
        model = keras.Model(
            inputs=position_input,
            outputs=[phase_output, move_output, confidence_output]
        )
        
        model.compile(
            optimizer='adam',
            loss={
                'phase': 'categorical_crossentropy',
                'move': 'categorical_crossentropy', 
                'confidence': 'mse'
            },
            metrics={
                'phase': 'accuracy',
                'move': 'top_k_categorical_accuracy',
                'confidence': 'mae'
            }
        )
        
        return model
```

### **Race Condition Implementation**
```python
import asyncio

class MoveSelector:
    def __init__(self, engine, opening_endgame_nn, complexity_nn):
        self.engine = engine
        self.opening_endgame_nn = opening_endgame_nn
        self.complexity_nn = complexity_nn
        
    async def get_optimal_move(self, position, time_limit):
        """Race neural network against engine search"""
        
        # Start neural network prediction
        nn_task = asyncio.create_task(
            self.get_nn_prediction(position)
        )
        
        # Give NN a head start (100ms)
        try:
            nn_result = await asyncio.wait_for(nn_task, timeout=0.1)
            
            # If NN is highly confident, use its move
            if nn_result['confidence'] > 0.95:
                return {
                    'move': nn_result['move'],
                    'source': 'neural_network',
                    'confidence': nn_result['confidence'],
                    'time_used': 0.1
                }
        except asyncio.TimeoutError:
            pass  # NN taking too long, proceed with engine
            
        # Start engine search
        engine_task = asyncio.create_task(
            self.get_engine_move(position, time_limit)
        )
        
        # Wait for engine result
        engine_result = await engine_task
        
        # Cancel NN task if still running
        if not nn_task.done():
            nn_task.cancel()
            
        return {
            'move': engine_result['move'],
            'source': 'engine_search',
            'evaluation': engine_result['evaluation'],
            'time_used': engine_result['time_used']
        }
        
    async def get_nn_prediction(self, position):
        """Get neural network move prediction"""
        features = self.encode_position(position)
        
        phase_pred, move_pred, confidence = self.opening_endgame_nn.predict(features)
        
        # Convert move prediction to UCI format
        move_uci = self.decode_move_prediction(move_pred, position)
        
        return {
            'move': move_uci,
            'phase': phase_pred,
            'confidence': confidence[0]
        }
        
    async def get_engine_move(self, position, time_limit):
        """Get engine search result with complexity awareness"""
        
        # Get complexity score
        complexity = self.complexity_nn.predict(position)
        
        # Adjust search time based on complexity
        adjusted_time = time_limit * (1 + complexity * 0.5)
        
        result = await self.engine.search_async(position, time=adjusted_time)
        
        return {
            'move': result.move,
            'evaluation': result.score,
            'time_used': result.time_used,
            'complexity': complexity
        }
```

---

## 🚀 **Deployment Strategy**

### **Model Optimization for Cloud Deployment**

#### **TensorFlow Lite Conversion**
```python
def optimize_models_for_deployment():
    # Convert complexity model
    complexity_converter = tf.lite.TFLiteConverter.from_keras_model(complexity_model)
    complexity_converter.optimizations = [tf.lite.Optimize.DEFAULT]
    complexity_tflite = complexity_converter.convert()
    
    # Convert opening/endgame model
    opening_converter = tf.lite.TFLiteConverter.from_keras_model(opening_endgame_model)
    opening_converter.optimizations = [tf.lite.Optimize.DEFAULT]
    opening_tflite = opening_converter.convert()
    
    # Save optimized models
    save_model(complexity_tflite, 'complexity_model.tflite')
    save_model(opening_tflite, 'opening_endgame_model.tflite')
```

#### **Docker Integration**
```dockerfile
# Add to existing Dockerfile
COPY neural_networks/ ./neural_networks/
RUN pip install tensorflow-lite-runtime

# Set environment variables for model paths
ENV COMPLEXITY_MODEL_PATH="/lichess-bot/neural_networks/complexity_model.tflite"
ENV OPENING_ENDGAME_MODEL_PATH="/lichess-bot/neural_networks/opening_endgame_model.tflite"
```

### **Configuration Management**
```yaml
# Enhanced config for neural network integration
neural_networks:
  complexity_assessment:
    enabled: true
    model_path: "./neural_networks/complexity_model.tflite"
    confidence_threshold: 0.3
    time_scaling_factor: 1.5
    
  opening_endgame_master:
    enabled: true
    model_path: "./neural_networks/opening_endgame_model.tflite" 
    confidence_threshold: 0.95
    max_inference_time: 100  # milliseconds
    
  fallback_strategy: "engine_search"  # Always fall back to engine
```

---

## 📊 **Performance Monitoring**

### **Neural Network Metrics**
```python
class NeuralNetworkMonitor:
    def __init__(self):
        self.metrics = {
            'complexity_nn': {
                'predictions_count': 0,
                'average_inference_time': 0,
                'accuracy_score': 0
            },
            'opening_endgame_nn': {
                'predictions_count': 0,
                'successful_predictions': 0,
                'average_confidence': 0,
                'games_won_with_nn': 0
            }
        }
        
    def log_complexity_prediction(self, inference_time, accuracy):
        self.metrics['complexity_nn']['predictions_count'] += 1
        self.metrics['complexity_nn']['average_inference_time'] = (
            self.metrics['complexity_nn']['average_inference_time'] * 0.9 + 
            inference_time * 0.1
        )
        
    def evaluate_nn_contribution(self):
        """Evaluate how much neural networks improve gameplay"""
        return {
            'time_savings': self.calculate_time_savings(),
            'move_accuracy_improvement': self.calculate_accuracy_gain(),
            'resource_efficiency': self.calculate_resource_efficiency()
        }
```

---

This neural network integration transforms C0BR4 into a **hybrid engine** that combines traditional search with AI-enhanced pattern recognition and complexity awareness, mimicking grandmaster-level chess intuition.