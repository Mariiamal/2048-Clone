using System;
using System.Collections.Generic;

namespace _2048_clone {
    /* Event handler (delegate), gets notification when the event is fired */
    public delegate void Notify();

    /* Class Tile - 2 private members and constructor, to refer to the coordinates of the grid */
    public class Tile {
        public int x, y; 
        public Tile(int x, int y) {
            this.x = x;
            this.y = y; 
        }
    }

    /* Class with all used constants */
    public static class Configs {
        public const int GRID_DIMENSION = 4;
        public const int TILE_LABEL_DIMENSION = 100;
        public const int TITLE_LABEL_HEIGHT = 100;
        public const int MENU_BUTTON_WIDTH = 400;
        public const int MENU_BUTTON_HEIGHT = 60;
        public const int PADDING = 40;
        public const int RESTART_BUTTON_HEIGHT = 100;
        public const int RESULTS_LABEL_HEIGHT = 50;
        public const int MOVE_EVALUATION_COEFF = 10;
        public const int TIMER_INTERVAL = 300; 
    }

    /* Enum with all 4 possible moves in the game */
    public enum Direction { Left, Right, Up, Down } 

    /* Main Game class.
       Contains logic for the game, main methods, private fields */
    public class Game {
        public event Notify Changed;
        public event Notify GameOver; 
        static Random random = new Random();
        public int[,] grid = new int[Configs.GRID_DIMENSION, Configs.GRID_DIMENSION];

        int moves;  
        int score; // total score gained, score = sum of two merged tiles (result tile) 
        bool gameIsOver; // no available moves + grid is full  
        bool reached; // true if 2048 was reached 
        int mergesCounter; // number of merged tiles

        /* Property, gets value whether the last move changed the grid; only Game class can modify its value */ 
        public bool LastMoveChanged { get; private set; } 

        /* Access to read-only properties for the current game state */
        public int Moves => moves;
         
        public int Score => score;
        public bool GameIsOver => gameIsOver;
        public bool Reached => reached;

        public int[,] Grid => grid; 

        public int MergesCounter => mergesCounter; 

        /* Indexer, read-only for accessing individual grid tiles */
        public int this[int x, int y] {
            get => grid[x, y];
        }
        
        /* Starts the new game; 
         Clears the grid, resets private fields to default values, generates 2 random tiles with values 2 or 4, fires an event */
        public void StartNewGame() {
            Array.Clear(grid, 0, grid.Length); 
            moves = 0;
            score = 0;
            gameIsOver = false;
            reached = false; 
            GenerateRandomTile();
            GenerateRandomTile();
            Changed?.Invoke(); 
        }

        /* Returns a list with coordinates (x, y) of all empty tiles (value 0) */ 
        public List<Tile> AllEmptyTiles() {
            List<Tile> tiles = new List<Tile>();
            for (int x = 0; x < Configs.GRID_DIMENSION; x++) {
                for (int y = 0; y < Configs.GRID_DIMENSION; y++) {
                    if (grid[x, y] == 0) tiles.Add(new Tile(x, y)); 
                }
            }
            return tiles;
        }
  
        /* Generates random tile, new tile: 90% to get value 2, 10% to get value 4 */
        void GenerateRandomTile() {
            List<Tile> tiles = AllEmptyTiles();
            if (tiles.Count == 0) return; 

            int randIndex = random.Next(tiles.Count);
            Tile randTile = tiles[randIndex]; 
            int randNumber = random.Next(100);

            int value = (randNumber < 90) ? 2 : 4;
            grid[randTile.x, randTile.y] = value; 
        }

        /* Makes a move: extracts the line, processes it (shift+merge), writes back on the grid;
           If board changed - random new tile is generated, fires an event */
        public void Move(Direction dir) {
            if (gameIsOver) return;

            bool gridChanged = false;
            bool needReverse = dir == Direction.Right || dir == Direction.Down; 
            for (int idx = 0; idx < Configs.GRID_DIMENSION; idx++) {
                int[] extractedLine = ExtractLine(idx, dir);
                int[] originalLine = (int[])extractedLine.Clone();

                if (needReverse) Array.Reverse(extractedLine); // Array.Reverse - built-in method, reverses elements in place
                ProcessLine(extractedLine);
                if (needReverse) Array.Reverse(extractedLine);

                WriteLine(idx, extractedLine, dir);

                if (!AreEqual(originalLine, extractedLine)) {
                    gridChanged = true;
                }
            }
            LastMoveChanged = gridChanged;
            if (!gridChanged) return; // if grid does not changed after the move - return 
            else {
                moves++;
                GenerateRandomTile();
                if ((AllEmptyTiles().Count == 0) && !CanMerge()) {  // check whether player can play after that move
                    gameIsOver = true;
                    GameOver?.Invoke(); // fire an event (game is over) 
                }
                Changed?.Invoke(); // fire an event (grid changed)
            }
        }

        /* Checks if at least 2 tiles can be merged together; 
           Compares values of the neighbouring tiles and checks if there exist such neighbour */
        bool CanMerge() {
            for (int x = 0; x < Configs.GRID_DIMENSION; x++) { 
                for (int y = 0; y < Configs.GRID_DIMENSION; y++) {
                    if (x < Configs.GRID_DIMENSION - 1 && grid[x, y] == grid[x + 1, y]) return true;
                    if (y < Configs.GRID_DIMENSION - 1 && grid[x, y] == grid[x, y + 1]) return true; 
                }
            }
            return false; 
        }

        /* Checks if 2 lines are equal - if both of them contains tiles with the same values */
        bool AreEqual(int[] line1, int[] line2) {
            for (int idx = 0; idx < Configs.GRID_DIMENSION; idx++) {
                if (line1[idx] != line2[idx]) return false; 
            }
            return true; 
        }

        /* Processes the line: shift - merge - shift; 
           Second shift to clean the gaps (empty tiles) that cpuld appear after merge */
        void ProcessLine(int[] line) {
            ShiftLine(line);
            MergeLine(line);
            ShiftLine(line); 
        }

        /* Shifts the line; 
           Assume that we shift always left (shifting towards the index 0), shift = moves all non-empty tiles to the left; 
           The order is preserved */
        void ShiftLine(int[] line) {
            int freePos = 0;
            for (int idx = 0; idx < Configs.GRID_DIMENSION; idx++) {
                if (line[idx] != 0) {
                    line[freePos] = line[idx];
                    if (freePos != idx) line[idx] = 0;
                    freePos++;
                }
            }  
        }

        /* Merges a line, each tile can be merged only once per one merge
           When two tiles with the same values merge, it results in one tile with doubled value 
           If we get tile with value 2048, reached = true */
        void MergeLine(int[] line) {
            for (int idx = 0; idx < Configs.GRID_DIMENSION - 1; idx++) {
                if (line[idx] == line[idx + 1] && line[idx] != 0) {
                    line[idx] *= 2;
                    line[idx + 1] = 0;

                    score += line[idx];
                    mergesCounter++; 
                    if (line[idx] == 2048) reached = true;
                    idx++;
                } 
            }
        }

        /* Extracts the line; 
           Makes a copy of the line with the index passed */
        int[] ExtractLine(int idx, Direction dir) {
            int[] extractedLine = new int[Configs.GRID_DIMENSION];
            // works with rows 
            if (dir == Direction.Left || dir == Direction.Right) {
                for (int x = 0; x < Configs.GRID_DIMENSION; x++) {
                    extractedLine[x] = grid[x, idx];
                }
            }
            // works with columns 
            else if (dir == Direction.Up || dir == Direction.Down) {
                for (int y = 0; y < Configs.GRID_DIMENSION; y++) {
                    extractedLine[y] = grid[idx, y];
                }
            }
            return extractedLine;
        }

        /* Writes processed line back in the playing grid */
        void WriteLine(int idx, int[] line, Direction dir) {
            //works with rows
            if (dir == Direction.Left || dir == Direction.Right) {
                for (int x = 0; x < Configs.GRID_DIMENSION; x++) {
                    grid[x, idx] = line[x]; 
                }
            }
            //works with columns
            else if (dir == Direction.Up || dir == Direction.Down) {
                for (int y = 0; y < Configs.GRID_DIMENSION; y++) {
                    grid[idx, y] = line[y];
                }
            }
        }

        /* Makes deep copy of the game; 
          Creates new Game and copies the current game state, grid returns copied Game */
        public Game DeepCopy(Game game) {
            Game deepCopy = new Game();
            deepCopy.grid = (int[,])this.grid.Clone();
            deepCopy.score = this.Score;
            deepCopy.mergesCounter = this.MergesCounter;
            deepCopy.moves = this.Moves;
            deepCopy.gameIsOver = this.gameIsOver;
            deepCopy.reached = this.Reached; 

            return deepCopy; 
        }
    }
}