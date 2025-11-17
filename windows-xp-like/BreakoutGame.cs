using System;
using System.Drawing;
using System.Windows.Forms;

namespace windows_xp_like
{
    public class BreakoutGame : Form, IAppFormChild
    {
        private const int GAME_WIDTH = 800;
        private const int GAME_HEIGHT = 450;

        bool goLeft;
        bool goRight;
        bool isGameOver;
        bool waitForSpace;

        int score;
        int ballx;
        int bally;
        int playerSpeed;

        Random rnd = new Random();

        Panel gamePanel;
        Label txtScore;
        Label txtMessage;
        PictureBox player;
        PictureBox ball;
        Timer gameTimer;

        PictureBox[] blockArray;

        private bool _isInputActive = true;

        public BreakoutGame()
        {
            this.Text = "벽돌 깨기";
            this.BackColor = Color.Black;

            Size fixedSize = new Size(840, 500);
            this.MinimumSize = fixedSize;
            this.Size = fixedSize;

            this.KeyPreview = true;
            this.DoubleBuffered = true;

            InitializeGameUI();
            PlaceBlocks();

            waitForSpace = true;
            gameTimer.Stop();
            UpdateMessage("Press Space to Start");
        }

        public void SetFocusState(bool isActive)
        {
            _isInputActive = isActive;

            if (!isActive)
            {
                goLeft = false;
                goRight = false;
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (!_isInputActive) return true;

            if (keyData == Keys.Left || keyData == Keys.Right)
            {
                return false;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void InitializeGameUI()
        {
            this.SuspendLayout();

            gamePanel = new Panel
            {
                Size = new Size(GAME_WIDTH, GAME_HEIGHT),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            gamePanel.Click += (s, e) => this.Focus();

            CenterGamePanel();
            this.Controls.Add(gamePanel);

            txtScore = new Label
            {
                Text = "Score: 0",
                Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            txtScore.Click += (s, e) => this.Focus();
            gamePanel.Controls.Add(txtScore);

            txtMessage = new Label
            {
                Text = "Press Space",
                Font = new Font("Verdana", 16F, FontStyle.Bold),
                ForeColor = Color.Yellow,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 50),
                Location = new Point((GAME_WIDTH - 400) / 2, (GAME_HEIGHT - 50) / 2),
                Cursor = Cursors.Hand
            };
            txtMessage.Click += (s, e) => this.Focus();
            gamePanel.Controls.Add(txtMessage);

            player = new PictureBox
            {
                BackColor = Color.White,
                Size = new Size(130, 25),
                Location = new Point((GAME_WIDTH - 130) / 2, GAME_HEIGHT - 50),
                Tag = "player"
            };
            gamePanel.Controls.Add(player);

            ball = new PictureBox
            {
                BackColor = Color.Yellow,
                Size = new Size(20, 20),
                Location = new Point((GAME_WIDTH - 20) / 2, GAME_HEIGHT - 80),
                Tag = "ball"
            };
            gamePanel.Controls.Add(ball);

            gameTimer = new Timer { Interval = 20 };
            gameTimer.Tick += MainGameTimerEvent;

            this.KeyDown += KeyIsDown;
            this.KeyUp += KeyIsUp;

            this.Load += (s, e) => this.Focus();
            this.Leave += (s, e) => ResetInputState();
            this.Deactivate += (s, e) => ResetInputState();

            this.ResumeLayout(false);
        }

        private void ResetInputState()
        {
            goLeft = false;
            goRight = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterGamePanel();
        }

        private void CenterGamePanel()
        {
            if (gamePanel != null)
            {
                int x = (this.ClientSize.Width - gamePanel.Width) / 2;
                int y = (this.ClientSize.Height - gamePanel.Height) / 2;

                if (x < 0) x = 0;
                if (y < 0) y = 0;

                gamePanel.Location = new Point(x, y);
            }
        }

        private void SetupGame()
        {
            isGameOver = false;
            score = 0;
            ballx = 5;
            bally = 5;
            playerSpeed = 12;
            txtScore.Text = "Score: " + score;
            txtMessage.Visible = false;

            player.Left = (GAME_WIDTH - player.Width) / 2;
            ball.Left = (GAME_WIDTH - ball.Width) / 2;
            ball.Top = player.Top - ball.Height - 10;

            foreach (Control x in gamePanel.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "blocks")
                {
                    x.BackColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                }
            }
        }

        private void GameOver(string message)
        {
            isGameOver = true;
            gameTimer.Stop();
            UpdateMessage(message + "\nPress Enter to Restart");
        }

        private void UpdateMessage(string msg)
        {
            txtMessage.Text = msg;
            txtMessage.Visible = true;
            txtMessage.BringToFront();
        }

        private void PlaceBlocks()
        {
            RemoveBlocks();

            blockArray = new PictureBox[18];

            int a = 0;
            int top = 50;
            int leftStart = 40;
            int left = leftStart;

            int blockW = 110;
            int blockH = 30;
            int gap = 10;

            for (int i = 0; i < blockArray.Length; i++)
            {
                blockArray[i] = new PictureBox();
                blockArray[i].Height = blockH;
                blockArray[i].Width = blockW;
                blockArray[i].Tag = "blocks";
                blockArray[i].BackColor = Color.White;

                if (a == 6)
                {
                    top = top + blockH + gap;
                    left = leftStart;
                    a = 0;
                }

                if (a < 6)
                {
                    a++;
                    blockArray[i].Left = left;
                    blockArray[i].Top = top;
                    gamePanel.Controls.Add(blockArray[i]);
                    left = left + blockW + gap;
                }
            }
            SetupGame();
        }

        private void RemoveBlocks()
        {
            if (blockArray == null) return;
            foreach (PictureBox x in blockArray)
            {
                if (x != null) gamePanel.Controls.Remove(x);
            }
        }

        private void MainGameTimerEvent(object sender, EventArgs e)
        {
            if (!_isInputActive)
            {
                goLeft = false;
                goRight = false;
            }

            txtScore.Text = "Score: " + score;

            if (goLeft && player.Left > 0)
            {
                player.Left -= playerSpeed;
            }
            if (goRight && player.Left < (GAME_WIDTH - player.Width))
            {
                player.Left += playerSpeed;
            }

            ball.Left += ballx;
            ball.Top += bally;

            if (ball.Left < 0 || ball.Left > (GAME_WIDTH - ball.Width))
            {
                ballx = -ballx;
            }
            if (ball.Top < 0)
            {
                bally = -bally;
            }

            if (ball.Bounds.IntersectsWith(player.Bounds))
            {
                if (bally > 0)
                {
                    ball.Top = player.Top - ball.Height;
                    bally = rnd.Next(5, 10) * -1;

                    if (ballx < 0) ballx = rnd.Next(5, 12) * -1;
                    else ballx = rnd.Next(5, 12);
                }
            }

            for (int i = gamePanel.Controls.Count - 1; i >= 0; i--)
            {
                Control x = gamePanel.Controls[i];
                if (x is PictureBox && (string)x.Tag == "blocks")
                {
                    if (ball.Bounds.IntersectsWith(x.Bounds))
                    {
                        score += 1;
                        bally = -bally;
                        gamePanel.Controls.Remove(x);
                    }
                }
            }

            if (score == blockArray.Length)
            {
                GameOver("You Win!!");
            }

            if (ball.Top > GAME_HEIGHT)
            {
                GameOver("You Lose!!");
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (!_isInputActive) return;

            if (e.KeyCode == Keys.Left) goLeft = true;
            if (e.KeyCode == Keys.Right) goRight = true;

            if (e.KeyCode == Keys.Space && waitForSpace && !isGameOver)
            {
                waitForSpace = false;
                txtMessage.Visible = false;
                gameTimer.Start();
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (!_isInputActive) return;

            if (e.KeyCode == Keys.Left) goLeft = false;
            if (e.KeyCode == Keys.Right) goRight = false;

            if (e.KeyCode == Keys.Enter && isGameOver)
            {
                PlaceBlocks();
                SetupGame();
                waitForSpace = true;
                gameTimer.Stop();
                UpdateMessage("Press Space to Start");
            }
        }
    }
}