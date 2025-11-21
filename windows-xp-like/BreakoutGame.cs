using System;
using System.Drawing;
using System.Windows.Forms;

namespace windows_xp_like
{
    public class BreakoutGame : Form, IAppFormChild // 키보드 포커스 문제를 해결하기 위해 새로 만든 인터페이스 상속
    {
        // 실제로 공이 돌아다닐 수 있는 게임 판 크기
        private const int GAME_WIDTH = 800;
        private const int GAME_HEIGHT = 450;

        // 게임 판보다 좀 더 여유롭게 큰 창 최소 크기
        private const int MIN_WIN_WIDTH = 840;
        private const int MIN_WIN_HEIGHT = 500;

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

            // 벽돌깨기는 위치에 민감하므로 최소 크기 설정
            Size fixedSize = new Size(MIN_WIN_WIDTH, MIN_WIN_HEIGHT);
            this.MinimumSize = fixedSize;
            this.Size = fixedSize;

            this.KeyPreview = true; // 포커스가 있는 컨트롤이 아닌 폼이 먼저 키를 가로챌 수 있도록 설정
            this.DoubleBuffered = true; // 화면 깜빡임 방지용

            InitializeGameUI();
            PlaceBlocks();

            waitForSpace = true;
            gameTimer.Stop();
            UpdateMessage("Press Space to Start");
        }

        public void SetFocusState(bool isActive)
        {
            // 키보드 포커싱 제어 인터페이스에서 넘어와 구현된 메서드
            // 벽돌깨기의 포커싱 제어는 창이 비활성화 상태일 때 방향키를 누르지 않은 상태로 초기화하는 것이 목표

            _isInputActive = isActive;

            if (!isActive)
            {
                // 비활성 상태면 방향키 둘 중 아무것도 누르지 않은 것처럼 false 넣기
                goLeft = false;
                goRight = false;
            }
        }


        /// <summary>
        /// 윈도우 폼에서 방향키는 기본적으로 버튼 간 이동용이라 포커싱 이동 문제가 발생해 이를 게임 조작용으로만 쓰게끔 덮어쓴 메서드
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            // 창이 선택되지 않았을 때, 즉 비활성 상태라면 true 반환해서 완전 차단
            // 이래야지만 포커스 이동과 게임 조작 둘 다 방지 가능
            if (!_isInputActive) return true;

            // 만약 방향키 입력이면 포커스 기능을 끄고 게임 키로 인식시키도록 return false
            // 이러면 윈도우가 이걸 일반 키 입력으로 취급해서 KeyDown 이벤트 발생
            if (keyData == Keys.Left || keyData == Keys.Right)
            {
                return false;
            }

            // 이 밖에는 그냥 기본 동작
            return base.ProcessDialogKey(keyData);
        }

        private void InitializeGameUI()
        {
            this.SuspendLayout(); // 깜빡임 방지를 위해 배치하는 동안만 화면 그리기 중지

            // 람다식 형태 이벤트 추가
            // s, e는 흔히 이벤트에서 받는 매개변수인 sender와 EventArgs, => 기호는 이 함수를 실행하면 오른쪽 코드가 실행된다는 뜻
            // 쉽게 말해 컨트롤이 추가될 때마다 자동으로 클릭 시 포커스 가져오도록 이벤트 추가
            this.ControlAdded += (s, e) =>
            {
                e.Control.Click += (sender, args) => this.Focus();

                // 만약 패널 같은 컨테이너가 추가되면, 그 내부에도 똑같이 감지하도록 설정
                e.Control.ControlAdded += (innerS, innerE) =>
                {
                    innerE.Control.Click += (sender, args) => this.Focus();
                };
            };

            // 당연히 폼 자체를 클릭했을 때도 포커스
            this.Click += (s, e) => this.Focus();

            // 창 기준 절대 좌표로 계산해 블록을 추가했을 때 창 크기 변경 시 정렬 문제 발생
            // 중앙 정렬용 패널을 만들어 거기에 담는 방식으로 변경
            gamePanel = new Panel
            {
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle // 창의 크기거 커져도 외곽선을 확실히 알 수 있도록 설정
            };

            gamePanel.ClientSize = new Size(GAME_WIDTH, GAME_HEIGHT);

            CenterGamePanel(); // 폼에 패널을 추가하기 전에 정렬 위치부터 계산
            this.Controls.Add(gamePanel);

            // 점수판 레이블, 게임 상태 표시 메시지용 레이블, 플레이어 픽쳐 박스, 공 픽쳐 박스 생성 코드
            // 이 요소들 전부 창 크기와 관계 없이 중앙 정렬하기 위해 게임 패널에 담기
            txtScore = new Label
            {
                Text = "Score: 0",
                Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10)
            };
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

            this.ResumeLayout(false);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterGamePanel();
        }

        /// <summary>
        /// 게임 패널을 중앙 정렬하기 위한 메서드
        /// </summary>
        private void CenterGamePanel()
        {
            if (gamePanel != null)
            {
                // x와 y 모두 실제 화면 크기에서 게임 패널 화면 크기를 뺀 것을 2로 나눈 것이 중앙값
                int x = (this.ClientSize.Width - gamePanel.Width) / 2;
                int y = (this.ClientSize.Height - gamePanel.Height) / 2;

                // 만약 x 또는 y 값이 음수라면 0으로 맞춰 줘야 화면이 왼쪽 위로 넘어가는 것을 방지
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
            if (isGameOver) return;

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

            // 원래는 테두리에 공이 닿으면 튕겼지만, 지금은 튕기는 
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