This guide walks through the process of building a chess engine in C\# based on Seb Lague's "Coding Adventure: Chess" video series. It's a complete walkthrough, starting from the most basic components, just as the videos do.

### **Part 1: The Board and Pieces**

* **Project Setup:** The video series uses the Unity game engine for the visual representation, but the core logic is pure C\#. You can begin with the provided starter project from the GitHub repository.  
* **Board Representation (Initial):** The creator first uses a 64-element byte array to represent the board. Each square corresponds to an index, and the value at that index represents the piece. Pieces are defined as a byte with bit flags to encode both type and color. For example, a Piece struct might have a Type (Pawn, Knight, etc.) and a Color (White, Black), but the value is stored as a single byte. Piece.IsWhite \= (piece & Piece.WhiteFlag) \!= 0 is a bitwise operation that quickly checks the color of the piece.  
* **Move Struct:** A Move is defined as a simple struct to keep data lightweight. It contains the start and end squares, along with other essential information like the piece that moved, the piece that was captured, and flags for special moves like castling, en passant, and pawn promotion.

### **Part 2: Move Generation and Validation**

* **Legal Move Generation:** The MoveGenerator class is responsible for creating a list of all pseudo-legal moves for a given position. This involves:  
  * Iterating through each piece on the board for the current player.  
  * For each piece, calculating its possible moves based on its type (e.g., a knight's L-shaped moves).  
  * Adding moves to a MoveList.  
  * Special attention is given to pawns due to their unique moves (captures, en passant, promotion).  
* **Making and Unmaking Moves:** This is a crucial step for the search algorithm. The Board class has a MakeMove method that updates the board's internal state (piece positions, castling rights, en passant square, etc.) and a corresponding UnmakeMove method that reverts the board to its previous state. This allows the search to explore branches of the game tree without creating new board objects for every position.  
* **Validating Moves:** After generating pseudo-legal moves, the engine must check for legality. A move is legal if it does not leave the king in check. The creator implements this by performing a MakeMove, checking if the king is attacked, and if so, marking the move as illegal and UnmakeMoveing it.

### **Part 3: The AI Engine and Optimization**

* **Random Bot:** The first AI is a simple random bot. It's used as a baseline to ensure the MoveGenerator and board logic are working correctly.  
* **Search (Negamax):** The core of the AI is a **Negamax** search algorithm. It's a recursive function that explores the game tree to a specified depth. It works by assigning a score to each position and assuming both players will make the best possible move. The Negamax function's power comes from its ability to represent both sides of the game with a single function, by negating the score on each recursive call.  
* **Evaluation Function:** The evaluation function assigns a numerical score to a board position. It is the "brain" of the AI.  
  * **Simple Evaluation:** A basic evaluation starts with material value (pawn \= 1, knight \= 3, etc.).  
  * **Piece-Square Tables (PSTs):** To add positional understanding, the creator uses PSTs. These are arrays that assign a bonus or penalty to each square for a given piece. For example, a pawn on a central square has a higher value than a pawn on the edge. The evaluation function sums the base material value and the PST values for all pieces on the board.  
* **Performance Optimizations:**  
  * **Bitboards:** To drastically increase performance, the creator transitions from the 64-element array to **bitboards**. A bitboard is a 64-bit integer where each bit represents a square on the board. This allows for extremely fast, hardware-level bitwise operations to generate and check moves.  
  * **Move Ordering:** Before the Negamax search, the moves are ordered to be searched from "most promising" to "least promising." This makes Alpha-Beta Pruning far more effective by finding the best moves early.  
  * **Alpha-Beta Pruning:** This algorithm significantly reduces the search space. Once a move is found that is "good enough" to beat an opponent's best response, the search can "prune" (skip) all other moves in that branch because they won't change the outcome.  
  * **Transposition Tables:** To avoid redundant calculations, a **transposition table** is used to store previously evaluated positions. When the search encounters a position it has already seen, it can retrieve the stored evaluation instead of re-calculating everything.  
  * **Search Extensions:** The creator adds **search extensions**, such as a **Quiescence Search**, to handle "noisy" positions. A quiescence search looks at only a specific subset of moves (like captures) to prevent the search from ending just before a major material exchange, which would result in an inaccurate evaluation.  
  * **UCI Protocol & Lichess Integration:** The video shows how to integrate the engine with the **UCI (Universal Chess Interface)** protocol, which allows the bot to communicate with standard chess interfaces and play on platforms like Lichess.

### **Competition Resources**

The user, Seb Lague, provided a starter kit for a chess bot challenge. You can use these resources to get started.

* **Starter Repository:** This is the code framework used for the challenge. You can clone this to get started on your own bot.  
  * **Repository URL:** https://github.com/SebLague/Chess-Challenge  
* **Competition Results:** This repository contains the results and source code of the bots that competed in the challenge. This is an excellent resource for seeing how others approached the problem and for finding advanced techniques.  
  * **Repository URL:** https://github.com/SebLague/Tiny-Chess-Bot-Challenge-Results  
* **Opponent Bots:** This contains the source code for the opponent bots used in the Tiny Chess Bots game. You can use these to test your own bot.  
  * **Repository URL:** https://github.com/SebLague/Tiny-Chess-Godot

*Remember to save and back up your work\! 💾*