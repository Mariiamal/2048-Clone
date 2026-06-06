2048 Game Clone
User Guide: How To Use 


1. Starting the game:
   To start playing the game, run Game.cs file.

2. Start game window:
   After the user launches the game, the main start window appears. It includes:
   - Game title
   - 2 modes of the game for choosing: 'Player Mode' and 'AI Mode'.
     Choose one by clicking the respective button to play.

3. Overall rules for 2048 Game:
   - there is 4x4 main grid, 16 tiles in total. Initially 2 tiles are generated and placed in random positions.
     90% for tile with value 2 and 10% for tile with value 4.
   - making a move: click the buttons (arrows) to move for one tile in Left/Right/Up/Down direction.
   - merging of the tiles: two neighbouring tiles with the same value merge in one tile with doubled value. 
   - game is over when there is no valid moves anymore (tile with value 2048 was reached/there is no empty spaces for further moves).

4. Player Mode:
   Regular grid with 2 values is displayed, player controls the game manually and makes all moves.

5. AI Mode:
   Regular grid with 2 values is displayed, but AI player evaluates the grid and makes the current best one, each move is shown automatically on the screen.

6. Finishing the game:
   When the game is over, game results are shown: "Game is over! {game.Score} points scored in {game.Moves} moves.", where game.Score is a sum of all tiles values,
   game.Moves is total number of moves that were made.
   - The 'Restart' button is displayed - click it to return to the start menu of the game.
   - User can end the game by closing the WinForms window of the game.
