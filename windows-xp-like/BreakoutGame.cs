using System;
using System.Drawing;
using System.Windows.Forms;

namespace windows_xp_like
{
    public class BreakoutGame : Form, IAppFormChild // 키보드 포커스 처리를 위해 만든 인터페이스 상속
    {
        // 실제 게임이 진행되는 영역 크기 (패널 기준)
        private const int GAME_WIDTH = 800;
        private const int GAME_HEIGHT = 450;

        // 게임 영역보다 약간 큰 최소 창 크기
        private const int MIN_WIN_WIDTH = 840;
        private const int MIN_WIN_HEIGHT = 500;

        // 왼쪽, 오른쪽 방향키 입력 상태
        bool goLeft;
        bool goRight;

        // 게임 종료 여부
        bool isGameOver;

        // 게임 시작 전 스페이스 입력을 기다리는지 여부
        bool waitForSpace;

        // 점수 (깨진 블록 개수)
        int score;

        // 공 속도 (x, y 방향)
        int ballx;
        int bally;

        // 패들(플레이어 막대) 이동 속도
        int playerSpeed;

        // 블록 색상을 랜덤으로 주기 위한 난수 객체
        Random rnd = new Random();

        // 실제 게임 화면이 올라가는 패널
        Panel gamePanel;

        // 점수를 표시하는 레이블
        Label txtScore;

        // "Press Space", "You Win" 등 메시지를 표시하는 레이블
        Label txtMessage;

        // 플레이어 패들
        PictureBox player;

        // 공
        PictureBox ball;

        // 게임 루프용 타이머 (주기적으로 게임 상태를 갱신)
        Timer gameTimer;

        // 블록들을 모아두는 배열
        PictureBox[] blockArray;

        // 현재 이 폼이 키보드 입력을 받을 수 있는 상태인지 여부 (탭 전환 등 고려)
        private bool _isInputActive = true;

        public BreakoutGame()
        {
            this.Text = "벽돌 깨기";
            this.BackColor = Color.Black;

            // 창 크기가 너무 작아져서 레이아웃이 깨지는 것을 방지하기 위해 최소 크기 지정
            Size fixedSize = new Size(MIN_WIN_WIDTH, MIN_WIN_HEIGHT);
            this.MinimumSize = fixedSize;
            this.Size = fixedSize;

            this.KeyPreview = true;    // 컨트롤보다 폼이 먼저 키 입력을 받도록 설정
            this.DoubleBuffered = true; // 깜빡임을 줄이기 위한 더블 버퍼링 설정

            InitializeGameUI(); // UI 초기 구성
            PlaceBlocks();      // 블록 배치

            // 처음에는 스페이스바 입력 전까지 대기 상태
            waitForSpace = true;
            gameTimer.Stop();
            UpdateMessage("Press Space to Start");
        }

        public void SetFocusState(bool isActive)
        {
            // 탭 변경이나 다른 창 전환 시 호출되는 포커스 상태 설정 메서드
            // 창이 비활성화되었을 때 방향키 입력이 남아 있는 문제를 방지하기 위함
            _isInputActive = isActive;

            if (!isActive)
            {
                // 입력 비활성 상태에서는 방향키 입력 상태를 초기화
                goLeft = false;
                goRight = false;
            }
        }

        // 방향키는 기본적으로 포커스 이동에 사용되므로, 여기서는 게임 조작용으로 쓰기 위해 재정의
        protected override bool ProcessDialogKey(Keys keyData)
        {
            // 창이 비활성 상태이면 키 입력을 처리하지 않게 함
            if (!_isInputActive) return true;

            // 방향키인 경우, 포커스 이동 동작을 막고 KeyDown 이벤트로 넘기기 위해 false 반환
            if (keyData == Keys.Left || keyData == Keys.Right)
            {
                return false;
            }

            // 이 외의 키는 기본 동작 사용
            return base.ProcessDialogKey(keyData);
        }

        // 게임에 필요한 UI 요소들을 한 번에 만드는 함수
        private void InitializeGameUI()
        {
            // 컨트롤들을 배치하는 동안 레이아웃 갱신을 잠시 멈춰 깜빡임을 줄임
            this.SuspendLayout();

            // 폼에 컨트롤이 추가될 때마다, 해당 컨트롤을 클릭하면 폼이 포커스를 가지도록 설정
            this.ControlAdded += (s, e) =>
            {
                // 추가된 컨트롤 클릭 시 포커스를 폼으로 이동
                e.Control.Click += (sender, args) => this.Focus();

                // 패널처럼 내부에 또 다른 컨트롤을 가지는 경우를 위해 중첩 처리
                e.Control.ControlAdded += (innerS, innerE) =>
                {
                    innerE.Control.Click += (sender, args) => this.Focus();
                };
            };

            // 폼 빈 공간 클릭 시에도 포커스를 폼으로 이동
            this.Click += (s, e) => this.Focus();

            // 창 크기 변화에 따라 좌표를 계속 다시 계산하지 않기 위해
            // gamePanel 하나를 만들고 이 패널을 중앙 정렬하는 방식으로 구성
            gamePanel = new Panel
            {
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle // 게임 영역 경계가 보이도록 설정
            };

            // 게임판 크기와 동일하게 클라이언트 크기 설정
            gamePanel.ClientSize = new Size(GAME_WIDTH, GAME_HEIGHT);

            // 게임 패널을 폼 중앙으로 정렬
            CenterGamePanel();
            this.Controls.Add(gamePanel);

            // 점수 표시 레이블 생성
            txtScore = new Label
            {
                Text = "Score: 0",
                Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10) // 게임 패널 기준 왼쪽 상단
            };
            gamePanel.Controls.Add(txtScore);

            // 중앙에 출력되는 메시지 레이블 (시작/승리/패배 안내용)
            txtMessage = new Label
            {
                Text = "Press Space",
                Font = new Font("Verdana", 16F, FontStyle.Bold),
                ForeColor = Color.Yellow,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 50),
                Location = new Point((GAME_WIDTH - 400) / 2, (GAME_HEIGHT - 50) / 2),
                Cursor = Cursors.Hand // 마우스 오버 시 손 모양 커서 표시
            };
            gamePanel.Controls.Add(txtMessage);

            // 플레이어 패들 (하단에 위치한 흰색 막대)
            player = new PictureBox
            {
                BackColor = Color.White,
                Size = new Size(130, 25),
                Location = new Point((GAME_WIDTH - 130) / 2, GAME_HEIGHT - 50),
                Tag = "player" // 나중에 타입 구분용으로 사용
            };
            gamePanel.Controls.Add(player);

            // 공 (노란색 사각형)
            ball = new PictureBox
            {
                BackColor = Color.Yellow,
                Size = new Size(20, 20),
                Location = new Point((GAME_WIDTH - 20) / 2, GAME_HEIGHT - 80),
                Tag = "ball"
            };
            gamePanel.Controls.Add(ball);

            // 게임 타이머 (20ms마다 한 번씩 호출되어 게임 상태 갱신)
            gameTimer = new Timer { Interval = 20 };
            gameTimer.Tick += MainGameTimerEvent;

            // 키 입력 이벤트 연결
            this.KeyDown += KeyIsDown;
            this.KeyUp += KeyIsUp;

            // 폼이 로드될 때 포커스를 폼으로 한 번 맞춰서 바로 키 입력을 받을 수 있게 함
            this.Load += (s, e) => this.Focus();

            this.ResumeLayout(false);
        }

        // 창 크기가 변경될 때마다 호출되는 메서드
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // 창 크기가 바뀌더라도 gamePanel이 항상 중앙에 오도록 조정
            CenterGamePanel();
        }

        // 게임 패널을 폼 중앙으로 배치하는 함수
        private void CenterGamePanel()
        {
            if (gamePanel != null)
            {
                // 중앙 좌표 = (전체 클라이언트 크기 - 패널 크기) / 2
                int x = (this.ClientSize.Width - gamePanel.Width) / 2;
                int y = (this.ClientSize.Height - gamePanel.Height) / 2;

                // 음수가 되면 화면 밖으로 나가므로 0으로 보정
                if (x < 0) x = 0;
                if (y < 0) y = 0;

                gamePanel.Location = new Point(x, y);
            }
        }

        // 게임 한 판을 시작하거나 다시 시작할 때 기본 상태를 초기화하는 함수
        private void SetupGame()
        {
            isGameOver = false;
            score = 0;

            // 공의 초기 속도 설정
            ballx = 5;
            bally = 5;

            // 패들 이동 속도 설정
            playerSpeed = 12;

            txtScore.Text = "Score: " + score;
            txtMessage.Visible = false; // 안내 메시지 숨김

            // 패들을 가로 중앙에 배치
            player.Left = (GAME_WIDTH - player.Width) / 2;

            // 공을 패들 위쪽에 위치시키기
            ball.Left = (GAME_WIDTH - ball.Width) / 2;
            ball.Top = player.Top - ball.Height - 10;

            // 이미 생성된 블록들의 색을 랜덤으로 변경
            foreach (Control x in gamePanel.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "blocks")
                {
                    x.BackColor = Color.FromArgb(
                        rnd.Next(256),
                        rnd.Next(256),
                        rnd.Next(256));
                }
            }
        }

        // 게임 종료 시 공통 처리 (메시지 출력 및 타이머 정지)
        private void GameOver(string message)
        {
            isGameOver = true;
            gameTimer.Stop();
            UpdateMessage(message + "\nPress Enter to Restart");
        }

        // 중앙 메시지 텍스트를 변경하고 표시하는 함수
        private void UpdateMessage(string msg)
        {
            txtMessage.Text = msg;
            txtMessage.Visible = true;
            txtMessage.BringToFront(); // 다른 컨트롤 위로 올려서 보이도록 함
        }

        // 블록들을 새로 생성하여 화면에 배치하는 함수
        private void PlaceBlocks()
        {
            // 이전에 생성된 블록이 있으면 먼저 제거
            RemoveBlocks();

            // 블록 총 18개 (가로 6개 × 세로 3줄)
            blockArray = new PictureBox[18];

            int a = 0;              // 현재 줄에 몇 개의 블록이 배치되었는지 카운트
            int top = 50;           // 첫 줄의 y 위치
            int leftStart = 40;     // 각 줄의 시작 x 위치
            int left = leftStart;   // 현재 블록의 x 위치
            int blockW = 110;       // 블록 가로 길이
            int blockH = 30;        // 블록 세로 길이
            int gap = 10;           // 블록 사이 간격

            for (int i = 0; i < blockArray.Length; i++)
            {
                blockArray[i] = new PictureBox();
                blockArray[i].Height = blockH;
                blockArray[i].Width = blockW;
                blockArray[i].Tag = "blocks";
                blockArray[i].BackColor = Color.White;

                // 한 줄에 6개 배치 후 다음 줄로 이동
                if (a == 6)
                {
                    top = top + blockH + gap; // 다음 줄로 y 좌표 이동
                    left = leftStart;         // x 좌표를 다시 시작 위치로 초기화
                    a = 0;
                }

                if (a < 6)
                {
                    a++;
                    blockArray[i].Left = left;
                    blockArray[i].Top = top;
                    gamePanel.Controls.Add(blockArray[i]);

                    // 다음 블록 위치 계산
                    left = left + blockW + gap;
                }
            }

            // 블록 배치 후 게임 상태 초기화
            SetupGame();
        }

        // 기존 블록들을 gamePanel에서 제거하는 함수
        private void RemoveBlocks()
        {
            if (blockArray == null) return;

            foreach (PictureBox x in blockArray)
            {
                if (x != null) gamePanel.Controls.Remove(x);
            }
        }

        // 게임 타이머가 틱마다 호출하는 메서드 (실질적인 게임 루프 역할)
        private void MainGameTimerEvent(object sender, EventArgs e)
        {
            // 게임이 이미 끝난 상태라면 더 이상 처리하지 않음
            if (isGameOver) return;

            // 입력 비활성 상태에서는 방향키 입력을 초기화
            if (!_isInputActive)
            {
                goLeft = false;
                goRight = false;
            }

            // 점수 UI 갱신
            txtScore.Text = "Score: " + score;

            // 왼쪽 이동 (화면 왼쪽 바깥으로 나가지 않도록 제한)
            if (goLeft && player.Left > 0)
            {
                player.Left -= playerSpeed;
            }

            // 오른쪽 이동 (화면 오른쪽 바깥으로 나가지 않도록 제한)
            if (goRight && player.Left < (GAME_WIDTH - player.Width))
            {
                player.Left += playerSpeed;
            }

            // 공 이동 (현재 속도만큼 위치 변경)
            ball.Left += ballx;
            ball.Top += bally;

            // 좌우 벽과 충돌 시 x 방향 반전
            if (ball.Left < 0 || ball.Left > (GAME_WIDTH - ball.Width))
            {
                ballx = -ballx;
            }

            // 위쪽 벽과 충돌 시 y 방향 반전
            if (ball.Top < 0)
            {
                bally = -bally;
            }

            // 공과 패들 간 충돌 체크
            if (ball.Bounds.IntersectsWith(player.Bounds))
            {
                // 공이 아래에서 위로 튕겨 나가야 하므로, 아래 방향으로 이동 중일 때만 처리
                if (bally > 0)
                {
                    // 공을 패들 위에 올려놓고
                    ball.Top = player.Top - ball.Height;

                    // 위쪽(-) 방향으로 y 속도를 랜덤 범위로 설정
                    bally = rnd.Next(5, 10) * -1;

                    // x 속도도 약간 랜덤하게 조정 (현재 방향은 유지)
                    if (ballx < 0) ballx = rnd.Next(5, 12) * -1;
                    else ballx = rnd.Next(5, 12);
                }
            }

            // 블록과의 충돌 검사
            // 뒤에서부터 탐색하는 이유: 컨트롤 제거 시 인덱스 문제가 생기지 않도록 하기 위함
            for (int i = gamePanel.Controls.Count - 1; i >= 0; i--)
            {
                Control x = gamePanel.Controls[i];

                if (x is PictureBox && (string)x.Tag == "blocks")
                {
                    if (ball.Bounds.IntersectsWith(x.Bounds))
                    {
                        score += 1;       // 점수 1점 증가
                        bally = -bally;   // y 방향 반전
                        gamePanel.Controls.Remove(x); // 해당 블록 제거
                    }
                }
            }

            // 모든 블록을 제거했다면 승리
            if (score == blockArray.Length)
            {
                GameOver("You Win!!");
            }

            // 공이 게임 화면 아래쪽으로 완전히 떨어지면 패배
            if (ball.Top > GAME_HEIGHT)
            {
                GameOver("You Lose!!");
            }
        }

        // 키를 눌렀을 때 호출되는 메서드
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (!_isInputActive) return;

            // 방향키 입력 상태 갱신
            if (e.KeyCode == Keys.Left) goLeft = true;
            if (e.KeyCode == Keys.Right) goRight = true;

            // 스페이스바로 게임 시작
            if (e.KeyCode == Keys.Space && waitForSpace && !isGameOver)
            {
                waitForSpace = false;
                txtMessage.Visible = false;
                gameTimer.Start(); // 타이머를 시작하여 게임 진행
            }
        }

        // 키에서 손을 뗐을 때 호출되는 메서드
        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (!_isInputActive) return;

            // 방향키에서 손을 떼면 이동 중지
            if (e.KeyCode == Keys.Left) goLeft = false;
            if (e.KeyCode == Keys.Right) goRight = false;

            // 게임 종료 상태에서 Enter를 누르면 재시작
            if (e.KeyCode == Keys.Enter && isGameOver)
            {
                PlaceBlocks();          // 블록 재생성
                SetupGame();            // 게임 상태 초기화
                waitForSpace = true;    // 다시 스페이스 입력 대기 상태로 전환
                gameTimer.Stop();
                UpdateMessage("Press Space to Start");
            }
        }
    }
}
