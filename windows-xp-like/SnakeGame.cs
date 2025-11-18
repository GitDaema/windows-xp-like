using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace windows_xp_like
{
    public class SnakeGame : Form, IAppFormChild
    {
        private const int GAME_WIDTH = 480;
        private const int GAME_HEIGHT = 500;
        private const int GRID_SIZE = 20;
        private int SCORE_AREA_HEIGHT = 40;

        private List<Point> snake = new List<Point>();
        private Point food;
        private int direction = 0;
        private Timer gameTimer = new Timer();
        private bool gameOver = false;
        private Random rand = new Random();

        private int score = 0;
        private Font scoreFont = new Font("Consolas", 14, FontStyle.Bold);
        private int finalScore = 0;

        private DateTime startTime;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private Font timerFont = new Font("Consolas", 14, FontStyle.Bold);

        private int initialInterval = 100;
        private int speedIncrease = 10;
        private int speedLevel = 0;

        private Point buffFood;
        private bool isBuffFoodActive = false;
        private Timer buffTimer = new Timer();
        private Timer buffSpawnTimer = new Timer();

        private bool isSnakePaused = false;
        private Timer pauseTimer = new Timer();
        private Timer slowTimer = new Timer();
        private int originalInterval;
        private bool isSnakeBlinking = false;
        private bool isSlowed = false;

        private Panel gamePanel;
        private bool _isInputActive = true;

        public SnakeGame()
        {
            this.Text = "Snake Game";
            this.BackColor = Color.Black; 

            this.DoubleBuffered = true;

            Size fixedSize = new Size(GAME_WIDTH + 40, GAME_HEIGHT + 40);
            this.MinimumSize = fixedSize;
            this.Size = fixedSize;

            this.KeyPreview = true;
            this.DoubleBuffered = true;

            InitializeGameUI();
            ResetGame();

            gameOver = true;

            this.KeyDown += new KeyEventHandler(OnKeyDown);
        }

        public void SetFocusState(bool isActive)
        {
            _isInputActive = isActive;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (!_isInputActive) return true;
            if (keyData == Keys.Right || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Up)
            {
                return false;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void InitializeGameUI()
        {
            this.SuspendLayout();

            // 더블 버퍼링으로 화면 깜빡임을 완화하기 위해 만든 커스텀 패널 클래스로 인스턴스화
            gamePanel = new DoubleBufferedPanel
            {
                Size = new Size(GAME_WIDTH, GAME_HEIGHT),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.Fixed3D // 뱀이 닿으면 게임오버되는 경계선을 뚜렷하게 표시하기 위한 테두리 스타일
            };

            gamePanel.Paint += new PaintEventHandler(OnPaint);
            gamePanel.Click += (s, e) => this.Focus();

            this.Controls.Add(gamePanel);
            CenterGamePanel();

            gameTimer.Interval = initialInterval;
            gameTimer.Tick += GameLoop;

            buffTimer.Interval = 3000;
            buffTimer.Tick += BuffTimer_Tick;
            buffSpawnTimer.Interval = 40000;
            buffSpawnTimer.Tick += BuffSpawnTimer_Tick;

            pauseTimer.Interval = 150;
            pauseTimer.Tick += PauseTimer_Tick;
            slowTimer.Interval = 10000;
            slowTimer.Tick += SlowTimer_Tick;

            this.Load += (s, e) => this.Focus();
            this.Deactivate += (s, e) => SetFocusState(false);

            this.ResumeLayout(false);
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

        private void BuffTimer_Tick(object sender, EventArgs e)
        {
            buffTimer.Stop();
            isBuffFoodActive = false;
            gamePanel.Invalidate();
        }

        private void BuffSpawnTimer_Tick(object sender, EventArgs e)
        {
            if (!isBuffFoodActive && !gameOver)
            {
                SpawnBuffFood();
            }
        }

        private void PauseTimer_Tick(object sender, EventArgs e)
        {
            isSnakeBlinking = !isSnakeBlinking;

            if (pauseTimer.Tag != null && (int)pauseTimer.Tag == 5)
            {
                pauseTimer.Stop();
                isSnakePaused = false;
                isSnakeBlinking = false;
                gameTimer.Start();
                pauseTimer.Tag = 0;
            }
            else
            {
                pauseTimer.Tag = (pauseTimer.Tag == null) ? 1 : (int)pauseTimer.Tag + 1;
            }
            gamePanel.Invalidate();
        }

        private void SlowTimer_Tick(object sender, EventArgs e)
        {
            slowTimer.Stop();
            isSlowed = false;

            gameTimer.Interval = originalInterval;
        }

        private void SpawnBuffFood()
        {
            int maxGridX = GAME_WIDTH / GRID_SIZE;
            int maxGridY = GAME_HEIGHT / GRID_SIZE;
            int minFoodGridY = SCORE_AREA_HEIGHT / GRID_SIZE + 1;

            buffFood = new Point(rand.Next(0, maxGridX),
                                rand.Next(minFoodGridY, maxGridY));

            while (snake.Contains(buffFood) || food == buffFood)
            {
                buffFood = new Point(rand.Next(0, maxGridX),
                                    rand.Next(minFoodGridY, maxGridY));
            }

            isBuffFoodActive = true;
            buffTimer.Start();
            gamePanel.Invalidate();
        }

        private void ResetGame()
        {
            snake.Clear();
            snake.Add(new Point(5, SCORE_AREA_HEIGHT / GRID_SIZE + 1));
            direction = 0;
            score = 0;
            finalScore = 0;
            SpawnFood();
            gamePanel.Invalidate();

            gameTimer.Interval = initialInterval;
            speedLevel = 0;
            elapsedTime = TimeSpan.Zero;

            isBuffFoodActive = false;
            buffTimer.Stop();
            buffSpawnTimer.Stop();

            isSnakePaused = false;
            isSnakeBlinking = false;
            isSlowed = false;
            pauseTimer.Stop();
            slowTimer.Stop();
        }

        private void SpawnFood()
        {
            int maxGridX = GAME_WIDTH / GRID_SIZE;
            int maxGridY = GAME_HEIGHT / GRID_SIZE;
            int minFoodGridY = SCORE_AREA_HEIGHT / GRID_SIZE + 1;

            food = new Point(rand.Next(0, maxGridX),
                            rand.Next(minFoodGridY, maxGridY));

            while (snake.Contains(food) || food == buffFood)
            {
                food = new Point(rand.Next(0, maxGridX),
                                rand.Next(minFoodGridY, maxGridY));
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameOver || isSnakePaused)
                return;

            elapsedTime = DateTime.Now - startTime;
            int newSpeedLevel = (int)(elapsedTime.TotalSeconds / 10);

            if (newSpeedLevel > speedLevel && !isSlowed)
            {
                speedLevel = newSpeedLevel;
                int newInterval = Math.Max(20, initialInterval - (speedIncrease * speedLevel));
                if (gameTimer.Interval != newInterval)
                {
                    gameTimer.Interval = newInterval;
                }
            }

            Point head = snake[0];
            Point newHead = head;

            switch (direction)
            {
                case 0: newHead.X += 1; break;
                case 1: newHead.Y += 1; break;
                case 2: newHead.X -= 1; break;
                case 3: newHead.Y -= 1; break;
            }

            if (newHead.X < 0 || newHead.Y < 0 ||
                newHead.X >= GAME_WIDTH / GRID_SIZE ||
                newHead.Y >= GAME_HEIGHT / GRID_SIZE ||
                snake.Contains(newHead) ||
                (newHead.Y * GRID_SIZE < SCORE_AREA_HEIGHT))
            {
                gameOver = true;
                gameTimer.Stop();
                finalScore = score;
                gamePanel.Invalidate();
                return;
            }

            snake.Insert(0, newHead);

            if (newHead == food)
            {
                score += 10;
                SpawnFood();
            }
            else if (isBuffFoodActive && newHead == buffFood)
            {
                score += 30;
                isBuffFoodActive = false;
                buffTimer.Stop();

                isSnakePaused = true;
                pauseTimer.Tag = 0;
                gameTimer.Stop();
                pauseTimer.Start();

                if (!isSlowed)
                {
                    originalInterval = gameTimer.Interval;
                    gameTimer.Interval = Math.Min(initialInterval, originalInterval + 50);
                    isSlowed = true;
                    slowTimer.Start();
                }
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            gamePanel.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRectangle(Brushes.DarkSlateGray, 0, 0, GAME_WIDTH, SCORE_AREA_HEIGHT);

            string scoreMsg = $"SCORE: {score}";
            g.DrawString(scoreMsg, scoreFont, Brushes.Yellow, 10, 10);

            string timerMsg = $"TIME: {elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}";
            SizeF timerSize = g.MeasureString(timerMsg, timerFont);
            float timerX = GAME_WIDTH - timerSize.Width - 10;
            float timerY = 10;
            g.DrawString(timerMsg, timerFont, Brushes.Yellow, timerX, timerY);

            if (gameOver)
            {
                string msg;
                Brush colorBrush;

                if (finalScore == 0 && score == 0)
                {
                    msg = "Press R to Start";
                    colorBrush = Brushes.LimeGreen;
                }
                else
                {
                    msg = $"GAME OVER!\nYour Score: {finalScore}\nPress R to Restart";
                    colorBrush = Brushes.Red;
                }

                Font overlayFont = new Font("Arial", 24, FontStyle.Bold);

                SizeF msgSize = g.MeasureString(msg, overlayFont);
                g.DrawString(msg, overlayFont, colorBrush,
                    (GAME_WIDTH - msgSize.Width) / 2,
                    (GAME_HEIGHT - msgSize.Height) / 2);

                overlayFont.Dispose();
                return;
            }

            if (isBuffFoodActive)
            {
                g.FillEllipse(Brushes.Gold, buffFood.X * GRID_SIZE, buffFood.Y * GRID_SIZE, GRID_SIZE, GRID_SIZE);
            }

            if (!isSnakeBlinking)
            {
                foreach (Point p in snake)
                    g.FillEllipse(Brushes.LimeGreen, p.X * GRID_SIZE, p.Y * GRID_SIZE, GRID_SIZE, GRID_SIZE);
            }

            g.FillEllipse(Brushes.Red, food.X * GRID_SIZE, food.Y * GRID_SIZE, GRID_SIZE, GRID_SIZE);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isInputActive) return;

            switch (e.KeyCode)
            {
                case Keys.Right:
                    if (direction != 2) direction = 0;
                    break;
                case Keys.Down:
                    if (direction != 3) direction = 1;
                    break;
                case Keys.Left:
                    if (direction != 0) direction = 2;
                    break;
                case Keys.Up:
                    if (direction != 1) direction = 3;
                    break;
                case Keys.R:
                    if (gameOver)
                    {
                        ResetGame();
                        gameOver = false;
                        startTime = DateTime.Now;
                        gameTimer.Start();
                        buffSpawnTimer.Start();
                    }
                    break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            gameTimer.Dispose();
            buffTimer.Dispose();
            buffSpawnTimer.Dispose();
            pauseTimer.Dispose();
            slowTimer.Dispose();

            base.OnFormClosing(e);
        }
    }

    /// <summary>
    /// 생성한 패널로는 protected 속성인 DoubleBuffered에 접근을 못하니 대체용으로 만든 깜빡임 제거용 패널 클래스
    /// </summary>
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            // 더블 버퍼링이 들어간 스타일로 강제 적용
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        // 어차피 OnPaint에서 전체 화면을 검은색으로 덮어 쓰니 배경 지우기 단계를 패스해서 깜빡임 더 완화
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
    }
}