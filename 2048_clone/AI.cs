using System;
using System.Collections.Generic;
using System.Linq;

namespace _2048_clone {
    public class AI {       

        /* Method that evaluates the grid, gets best move and returns the direction to do that move */
        public Direction GetBestMove(Game game) {
            Dictionary<Direction, int> movesEvaluation = new Dictionary<Direction, int>(); 
            // typeof - gets the type object for the enumeration
            // Enum.GetValues - returns array with values of the constants in specified enumeration 
            foreach (Direction dir in Enum.GetValues(typeof(Direction))) {

                Game copiedGame = game.DeepCopy(game);
                int beforeMoves = copiedGame.Moves; 
                copiedGame.Move(dir);
                // check if grid changed after ai did a move 
                if (!copiedGame.LastMoveChanged) {
                    continue; 
                }
                int moveValue = EvaluateGrid(copiedGame);
                movesEvaluation[dir] = moveValue;  
            }

            /* If mo moves in any possible direction changed the grid - return the default direction - Left */
            if (movesEvaluation.Count == 0) {
                return Direction.Left; 
            }
            Direction bestDir = movesEvaluation.OrderByDescending(kvp => kvp.Value).First().Key; 
            return bestDir; 
        }

        /* Evaluation of the grid; 
           for each move there is a value that was produced by this move, obtained by 
           count of all empty tiles, count of merged tiles, current max value of the tile on the grid */
        int EvaluateGrid(Game aiGame) { 
            int gridValue = 0;

            gridValue += (aiGame.AllEmptyTiles().Count) * Configs.MOVE_EVALUATION_COEFF; 
            gridValue += (aiGame.MergesCounter) * (2 * Configs.MOVE_EVALUATION_COEFF);
            gridValue += aiGame.Grid.Cast<int>().Max() * 2;  // Cast<int> converts array into IEnumarable<int>, so we can take Max()
            return gridValue; 
        }
    }
}
