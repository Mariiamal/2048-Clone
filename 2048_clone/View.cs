using System;
using System.Drawing;
using System.Windows.Forms;

namespace _2048_clone {
    public partial class View : Form {

        /* Private fields */
        Game game;
        Label[,] grid = new Label[Configs.GRID_DIMENSION, Configs.GRID_DIMENSION];

        // main fonts - main text, for title text, for buttons
        Font MainFont = new Font("Arial", 25);
        Font TitleFont = new Font("Arial", 52, FontStyle.Bold);
        Font ButtonsFont = new Font("Arial", 40, FontStyle.Bold); 

        Label titleLabel;
        Label gameResultsLabel; 
        Button playerButton;
        Button aiButton;
        Button restartButton;
        AI ai; // AI mode as a second game mode 
        Timer aiTimer = new Timer(); // Timer for automatic moves of AI player (time interval = 300 milliseconds)

        /* Initializes UI; 
           Creates Game object, initializes start menu, draws the first frame */
        public View() {
            InitializeComponent();
            this.BackColor = Color.LavenderBlush; 
            // Allows a form to intercept keyboard events (keydown/up/..) before they are passed to the control in focus 
            // priority of reacting to keys pressed are given to moves of tiles
            this.KeyPreview = true;
            this.KeyDown += View_KeyDown;
            InitializeStartMenu();
        }

        /* Initializes start menu at the beginning of the game
           Creates and sets label "2048 Clone" and buttons "Player Mode", "AI Mode" */
        void InitializeStartMenu() {
            titleLabel = new Label();
            titleLabel.Text = "2048 Clone"; 
            titleLabel.Font = TitleFont;
            titleLabel.ForeColor = Color.Indigo;
            titleLabel.AutoSize = false;  // do not automatically resize the label based on its content (text) 
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Top; // label is placed at the top of the form, stretched horizontally
            titleLabel.Height = Configs.TITLE_LABEL_HEIGHT; 
            this.Controls.Add(titleLabel); // label is added to the form's collection of controls, displayed on the form

            playerButton = new Button();
            playerButton.Text = "Player Mode";
            playerButton.Font = ButtonsFont;
            playerButton.ForeColor = Color.White;
            playerButton.BackColor = Color.Indigo;
            playerButton.AutoSize = true;
            playerButton.Size = new Size(Configs.MENU_BUTTON_WIDTH, Configs.MENU_BUTTON_HEIGHT);
            playerButton.Anchor = AnchorStyles.None; // button does not stick to any edge, stays centered 
            this.Controls.Add(playerButton);
            playerButton.Click += StartPlayerMode; // event handler for StartPlayerMode is attached

            aiButton = new Button();
            aiButton.Text = "AI Mode";
            aiButton.Font = ButtonsFont;
            aiButton.ForeColor = Color.White;
            aiButton.BackColor = Color.Indigo; 
            aiButton.AutoSize = true;
            aiButton.Size = new Size(Configs.MENU_BUTTON_WIDTH, Configs.MENU_BUTTON_HEIGHT);
            aiButton.Anchor = AnchorStyles.None;
            this.Controls.Add(aiButton);
            aiButton.Click += StartAIMode; 

            /* manually counting the right positions for the buttons with the padding included between them */
            int totalWidth = playerButton.Width + Configs.PADDING + aiButton.Width;
            // clientsize - size that represents the size of the form's client area 
            int startX = (this.ClientSize.Width - totalWidth) / 2;
            int centerY = (this.ClientSize.Height - playerButton.Height) / 2;
            playerButton.Location = new Point(startX, centerY);
            aiButton.Location = new Point(startX + playerButton.Width + Configs.PADDING, centerY);
        }

        /* Looks at the game grid tiles (each tile is a label), sets the displayed text and tile color based on the value */
        void UpdateUI() {
            for (int x = 0; x < Configs.GRID_DIMENSION; x++) {
                for (int y = 0; y < Configs.GRID_DIMENSION; y++) {
                    int value = game.grid[x, y];

                    if (value == 0) grid[x, y].Text = "";
                    else {
                        grid[x, y].Text = value.ToString();
                    }
                    grid[x, y].BackColor = GetTileColor(value);
                }
            }
        }

        /* Shared settings for both Player Mode and AI mode in the game */
        void SharedStartSettings() {
            titleLabel.Visible = true;
            playerButton.Visible = false;
            aiButton.Visible = false;

            game = new Game();
            game.Changed += UpdateUI;
            game.GameOver += ShowGameIsOver;

            InitializeGrid();
            game.StartNewGame();
            UpdateUI();
        }

        /* Settings for the AI mode
           Creates AI class instance, sets timer with interval 300 ms - every interval timer fires Tick event */
        void StartAIMode(object sender, EventArgs e) {
            SharedStartSettings();
            
            aiTimer.Interval = Configs.TIMER_INTERVAL;
            ai = new AI();
            aiTimer.Tick += AITimerTick; 
            aiTimer.Start(); // timer begins its cycle 
        }

        /* One tick of the timer: if game is not null or over - evaluates the grid, makes best move
           Each move is shown on the game grid automatically */
        void AITimerTick(object sender, EventArgs e) {
            if (game == null || game.GameIsOver) {
                aiTimer.Stop(); // timer ends its cycle 
                return;
            }
            Direction bestDir = ai.GetBestMove(game);
            game.Move(bestDir);
        }

        /* Settings for Player Mode */
        void StartPlayerMode(object sender, EventArgs e) {
            SharedStartSettings(); 
        }

        /* Initializes the 4x4 grid; 
           Goes over the grid[,] and places each tile on the position (x, y) */
        void InitializeGrid()
        {
            int totalWidthGrid = Configs.TILE_LABEL_DIMENSION * Configs.GRID_DIMENSION;
            int startXGrid = (this.ClientSize.Width - totalWidthGrid) / 2;
            int startYGrid = (this.ClientSize.Height - totalWidthGrid) / 2;
            for (int x = 0; x < Configs.GRID_DIMENSION; x++)
            {
                for (int y = 0; y < Configs.GRID_DIMENSION; y++)
                {
                    grid[x, y] = new Label();

                    grid[x, y].Size = new Size(Configs.TILE_LABEL_DIMENSION, Configs.TILE_LABEL_DIMENSION);
                    grid[x, y].TextAlign = ContentAlignment.MiddleCenter;
                    grid[x, y].BorderStyle = BorderStyle.FixedSingle;
                    grid[x, y].BackColor = Color.OldLace;
                    grid[x, y].ForeColor = Color.White;
                    grid[x, y].Font = MainFont;
                    this.Controls.Add(grid[x, y]); // adds created label to the form

                    int xCoord = startXGrid + x * Configs.TILE_LABEL_DIMENSION;
                    int yCoord = startYGrid + y * Configs.TILE_LABEL_DIMENSION;
                    grid[x, y].Location = new Point(xCoord, yCoord);
                }
            }
        }

        /* "Game is over" state of the game; 
            Creates new label with the scores of the game and restart button; grid, title are unchanged; 
            Updates UI */
        void ShowGameIsOver() {
            gameResultsLabel = new Label();
            gameResultsLabel.Text = $"Game is over! {game.Score} points scored in {game.Moves} moves.";
            gameResultsLabel.Font = MainFont;
            gameResultsLabel.ForeColor = Color.Indigo;
            gameResultsLabel.AutoSize = false;
            gameResultsLabel.Width = this.ClientSize.Width;
            gameResultsLabel.Height = Configs.RESULTS_LABEL_HEIGHT; 
            gameResultsLabel.TextAlign = ContentAlignment.MiddleCenter; 
            int yCoord = Configs.TITLE_LABEL_HEIGHT + Configs.PADDING; 
            gameResultsLabel.Location = new Point(0, yCoord); 
            this.Controls.Add(gameResultsLabel);

            restartButton = new Button();
            restartButton.Text = "Restart the game";
            restartButton.Font = ButtonsFont;
            restartButton.ForeColor = Color.White;
            restartButton.BackColor = Color.Indigo;
            restartButton.AutoSize = false;
            restartButton.TextAlign = ContentAlignment.MiddleCenter;
            restartButton.Dock = DockStyle.Bottom;
            restartButton.Height = Configs.RESTART_BUTTON_HEIGHT; 
            this.Controls.Add(restartButton);
            restartButton.Click += RestartGame;
            UpdateUI(); 
        }

        /* Cleans UI and rebuilds it - everything on the form is deleted (UI), logic remains the same 
           Start Menu is initialised */
        void RestartGame(object sender, EventArgs e) {
            this.Controls.Clear();
            InitializeStartMenu(); 
        }

        /*  Detects which key was pressed by the user, tells the game object to move and redraws the grid */
        private void View_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Left:
                    game.Move(Direction.Left);
                    break;
                case Keys.Right:
                    game.Move(Direction.Right);
                    break;
                case Keys.Up:
                    game.Move(Direction.Up);
                    break;
                case Keys.Down:
                    game.Move(Direction.Down);
                    break;
            }
        }

        /* Detects the corresponding color for the tile based on its value */
        Color GetTileColor(int tileValue) {
            switch (tileValue) {
                case 2: return Color.Thistle;
                case 4: return Color.Plum;
                case 8: return Color.MediumOrchid;
                case 16: return Color.Orchid;
                case 32: return Color.MediumSlateBlue;
                case 64: return Color.SlateBlue;
                case 128: return Color.BlueViolet;
                case 256: return Color.DarkMagenta;
                case 512: return Color.Indigo;
                case 1024: return Color.MediumBlue;
                case 2048: return Color.Navy;
                default: return Color.White;
            }
        }

        private void View_Load(object sender, EventArgs e) {

        }
    }
}
