using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace windows_xp_like
{
    public class MinesweeperGame : Form
    {
        readonly int[] gridSizes = new int[] { 10, 15, 20 }; //난이도별 사이즈를 인덱스 0,1,2에 저장
        const int CELL_SIZE = 22;

        int GRID = 20;
        int minesCount = 15;
        int currentDifficultyIndex = 0;

        Button[,] buttons;//2차원 배열의 버튼
        Panel gamePanel;
        Panel topInfoPanel;
        Label timerLabel;
        Label flagLabel;

        int[,] field;//셀의 상태값 저장, 폭탄=-1
        bool[,] opened;//열렸는지 여부
        bool[,] flagged;//깃발 설치됐는지 여부
        Timer gameTimer;
        DateTime startTime;
        bool gameRunning = false;//게임 실행 중인지 여부
        int remainingFlags;//남은 깃발 수
        Random rnd = new Random();

        public MinesweeperGame()
        {
            this.Text = "지뢰찾기";

            // 고정 크기 설정
            Size fixedSize = new Size(500, 520);

            this.MinimumSize = fixedSize;
            this.Size = fixedSize;

            this.BackColor = Color.Silver; // 혹시 몰라 지뢰찾기 배경색으로 설정
            this.DoubleBuffered = true; // 깜빡임 방지를 위해 폼의 더블 버퍼링 설정 true로

            InitializeStartScreen();
        }

        void InitializeStartScreen()//스크린 초기화
        {
            this.Controls.Clear();

            // 창 크기를 바꿀 때 절대 좌표는 위치가 틀어지므로 자동 정렬이 필요
            // 표처럼 행과 열 격자를 만들어 UI 요소를 정렬하는 테이블 레이아웃 패널 활용

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 6;

            // 화면을 상하 대칭으로 분할하기 위해 유동적인 공간인 퍼센트 할당
            // 위와 아래 사이 중앙 지역에는 고정된 높이의 메뉴 버튼이 필요하므로 고정 설정 
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            Label title = new Label//시작화면 제목 레이블 생성
            {
                Text = "지뢰찾기",
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            // 버튼 생성할 때 좌표 지정 없이 Anchor로 중앙 정렬
            Button CreateMenuBtn(string text, int diff, int mines)//메뉴 버튼 만들기 함수. 텍스트, 난이도, 폭탄을 저장
            {
                Button btn = new Button { Text = text, Size = new Size(160, 35) };
                btn.Anchor = AnchorStyles.None;
                btn.Click += (s, e) => StartGameWithDifficulty(diff, mines);//버튼 클릭시 난이도 시작 함수 호출
                btn.Cursor = Cursors.Hand;
                return btn;
            }

            // 테이블에 요소들을 (요소, 열, 행) 순서로 매개변수에 담아 추가
            // 요소들은 위에서부터 하나씩 위에서 설정한 고정 크기 행 차지
            layout.Controls.Add(title, 0, 1);
            layout.Controls.Add(CreateMenuBtn("쉬움 (10x10)", 0, 10), 0, 2);//요소: 텍스트, 난이도 인덱스, 폭탄수
            layout.Controls.Add(CreateMenuBtn("보통 (15x15)", 1, 40), 0, 3);
            layout.Controls.Add(CreateMenuBtn("어려움 (20x20)", 2, 80), 0, 4);

            this.Controls.Add(layout);
        }

        void StartGameWithDifficulty(int difficultyIndex, int mines)//난이도(버튼 클릭시) 시작 함수, 난이도 인덱스와 폭탄 저장
        {
            currentDifficultyIndex = difficultyIndex;
            GRID = gridSizes[difficultyIndex];
            minesCount = mines;

            InitializeGameBoard();//초기화(첫 시작도 마찬가지)
            InitializeGameUI();
            StartTimer();
            gameRunning = true;//게임 실행 상태
        }

        void InitializeGameUI()//UI 초기화
        {
            this.SuspendLayout(); // 깜빡임 방지를 위한 그리기 일시 정지

            this.Controls.Clear();

            // 상단 정보 표시용 패널 추가, DockStyle은 Top으로 해서 화면 상단바 기준 일정한 위치 유지
            topInfoPanel = new Panel//게임 상단의 깃발과 타이머가 들어갈 부분
            {
                Dock = DockStyle.Top,
                Height = 36, 
                Padding = new Padding(15, 0, 15, 0),
                BackColor = Color.Silver
            };

            remainingFlags = minesCount;//깃발 개수 = 폭탄 수
            flagLabel = new Label//깃발을 표시할 레이블
            {
                Text = $"🚩 {remainingFlags}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 100,
                Dock = DockStyle.Left
            };

            timerLabel = new Label//타이머
            {
                Text = "00:00",
                Font = new Font("Consolas", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Width = 100,
                Dock = DockStyle.Right
            };

            // 정보 표시 요소들을 상단 정보 표시용 패널에 담아서 창 크기가 바뀌어도 상대적으로 위치 유지
            topInfoPanel.Controls.Add(flagLabel);
            topInfoPanel.Controls.Add(timerLabel);
            this.Controls.Add(topInfoPanel);

            
            int boardSize = GRID * CELL_SIZE;
            // 원래는 난이도에 따라 패널 여백을 계산했으나 창 크기 변경했을 때 한쪽으로 쏠리는 현상 발생
            gamePanel = new Panel
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.Gray
            };

            gamePanel.ClientSize = new Size(boardSize, boardSize);

            // 게임 패널의 위치는 중앙 정렬 함수를 호출해서 지정
            CenterGamePanel(); 

            buttons = new Button[GRID, GRID];//2차원 배열로 버튼을 생성
            
            for (int y = 0; y < GRID; y++)//그리드 수에 맞춰 반복 생성
                for (int x = 0; x < GRID; x++)
                {
                    Button b = new Button
                    {
                        Size = new Size(CELL_SIZE, CELL_SIZE),
                        Location = new Point(x * CELL_SIZE, y * CELL_SIZE),
                        Tag = new Point(x, y),//좌표를 태그에 저장
                        BackColor = Color.LightGray,
                        Font = new Font("Verdana", 8, FontStyle.Bold), 
                        UseVisualStyleBackColor = false,
                        FlatStyle = FlatStyle.Popup,
                        Margin = Padding.Empty
                    };
                    b.MouseUp += Cell_MouseUp;
                    buttons[y, x] = b;
                    gamePanel.Controls.Add(b);
                }

            this.Controls.Add(gamePanel);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterGamePanel(); // 창 크기가 변할 때마다 게임 패널이 중앙 정렬되도록 호출
        }

        private void CenterGamePanel()//게임 화면
        {
            if (gamePanel != null && !gamePanel.IsDisposed) // 게임 패널 변수에 값이 들어 있고, 삭제되지 않았다면
            {
                // (창 너비 - 게임판 너비) / 2가 곧 중앙 x 좌표
                int x = (this.ClientSize.Width - gamePanel.Width) / 2;

                int topOffset; // 상단 정보 표시용 패널의 높이 저장용
                if (topInfoPanel != null)
                {
                    topOffset = topInfoPanel.Height;
                }
                else
                {
                    topOffset = 0;
                }

                int availableHeight = this.ClientSize.Height - topOffset;

                // 식을 풀어 쓰면 (화면 높이)/2 - (정보 표시 패널 높이)/2 - (게임 패널 높이)/2 + (정보 표시 패널 높이)
                // == (화면 높이)/2 - (게임 패널 높이)/2 + (정보 표시 패널 높이)/2
                // 즉, x와 같은 원리로 중앙을 구하되 정보 표시 패널로 인해 잃은 상단 공간만큼 판을 밑으로 내리는 것
                int y = (availableHeight - gamePanel.Height) / 2 + topOffset;

                // 만약 y가 너무 작으면 상단 정보 표시 패널과 겹칠 수 있으므로 최솟값 보정 
                if (y < topOffset + 5) y = topOffset + 5;

                gamePanel.Location = new Point(x, y);
            }
        }

        void InitializeGameBoard()//게임보드 초기화
        {
            field = new int[GRID, GRID];
            opened = new bool[GRID, GRID];
            flagged = new bool[GRID, GRID];

            int placed = 0;//설치된 폭탄 수
            while (placed < minesCount)//지정된 폭탄 수까지 설치 반복
            {
                int x = rnd.Next(GRID);
                int y = rnd.Next(GRID);
                if (field[y, x] != -1)//이미 폭탄이 설치된 곳이 아니라면
                {
                    field[y, x] = -1;//폭탄으로 저장
                    placed++;
                }
            }
            //폭탄의 개수를 나타내는 숫자
            for (int y = 0; y < GRID; y++)//모든 셀을 반복
                for (int x = 0; x < GRID; x++)
                {
                    if (field[y, x] == -1) continue;
                    int cnt = 0;//하나의 셀 주변의 폭탄 수
                    for (int dy = -1; dy <= 1; dy++)//하나의 셀을 중심으로 8개의 좌표를 순회
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx >= 0 && ny >= 0 && nx < GRID && ny < GRID && field[ny, nx] == -1)//그리드 밖을 넘어가지 않는지 검사
                                cnt++;//폭탄이라면 카운트+1
                        }
                    field[y, x] = cnt;//폭탄 검사가 끝난 카운트를 셀의 필드에 저장
                }
        }

        void StartTimer()
        {
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Dispose();
            }
            startTime = DateTime.Now;
            gameTimer = new Timer { Interval = 500 };
            gameTimer.Tick += (s, e) =>
            {
                timerLabel.Text = FormatTime(DateTime.Now - startTime);
            };
            gameTimer.Start();
        }

        string FormatTime(TimeSpan t)
        {
            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private void Cell_MouseUp(object sender, MouseEventArgs e)
        {
            if (!gameRunning) return;

            Button btn = (Button)sender;
            Point p = (Point)btn.Tag;
            int x = p.X, y = p.Y;

            if (e.Button == MouseButtons.Right)//우클릭 이벤트
            {
                if (opened[y, x]) return;//이미 오픈되어있지 않아야 함
                if (!flagged[y, x] && remainingFlags == 0) return;//깃발이 이미 설치되어있지 않고 남은 깃발 수가 1이 아니라면

                flagged[y, x] = !flagged[y, x];
                btn.Text = flagged[y, x] ? "⚑" : "";//깃발 설치(true) 또는 제거된 상태(false)
                remainingFlags += flagged[y, x] ? -1 : 1;//남은 깃발 수에 -1 또는 +1
                flagLabel.Text = $"🚩 {remainingFlags}";
                btn.ForeColor = Color.Red;
                return;
            }
            else if (e.Button == MouseButtons.Left)//좌클릭 이벤트
            {
                if (opened[y, x] || flagged[y, x]) return;//이미 오픈되어 있거나 깃발이 설치되어 있지 않다면

                if (field[y, x] == -1) TriggerExplosion(x, y);//폭탄이라면 폭발 함수 호출(좌표 저장)
                else { OpenCell(x, y); CheckWinCondition(); }//아니라면 셀 오픈, 성공 검사
            }
        }

        void OpenCell(int x, int y)//셀 오픈 함수
        {
            if (x < 0 || y < 0 || x >= GRID || y >= GRID) return;//셀을 벗어나지 않았는지 확인
            if (opened[y, x]) return;//이미 오픈된 셀은 무시

            opened[y, x] = true;//오픈 상태를 true로 변경
            Button b = buttons[y, x];//저장해둔 버튼을 불러옴
            b.Enabled = false;
            int v = field[y, x];
            b.Text = v == 0 ? "" : v.ToString();//필드 값이 0이면 빈 셀, 아니면 필드값의 숫자를 표시
            b.BackColor = Color.White;

           
        }

        async void TriggerExplosion(int hitX, int hitY)//폭발 함수(좌표)
        {
            gameRunning = false;//게임 실행 종료 상태
            gameTimer?.Stop();

            Button hitBtn = buttons[hitY, hitX];//폭탄인 셀을 폭발 이미지로 변경
            hitBtn.Text = "💥";
            hitBtn.BackColor = Color.Red;

            await Task.Delay(400);//폭발에서 폭탄 이미지로 변경되기 전 기다림
            for (int y = 0; y < GRID; y++)//모든 셀 중에서
                for (int x = 0; x < GRID; x++)
                    if (field[y, x] == -1)//폭탄이라면
                    {
                        Button b = buttons[y, x];//해당 셀 이미지를 폭탄으로 변경
                        b.Text = "💣";
                        b.BackColor = Color.LightCoral;
                        b.Enabled = false;
                    }

            ShowEndPanel(false, DateTime.Now - startTime);//종료 함수 호출
        }

        void CheckWinCondition()//우승 검사
        {
            int openedCount = 0;//오픈된 셀의 수
            for (int y = 0; y < GRID; y++)
                for (int x = 0; x < GRID; x++) if (opened[y, x]) openedCount++;

            if (openedCount == GRID * GRID - minesCount)//우승 조건: 오픈된 셀의 수 = 전체 셀의 수 - 폭탄 수
            {
                gameRunning = false;//게임 종료
                gameTimer?.Stop();
                ShowEndPanel(true, DateTime.Now - startTime);//종료 함수 호출
            }
        }

        void ShowEndPanel(bool win, TimeSpan elapsed)//게임 종료 함수
        {
            Panel overlay = new Panel//게임 종료 패널
            {
                Size = new Size(300, 140),
                BackColor = Color.FromArgb(230, Color.LightGray),
                BorderStyle = BorderStyle.FixedSingle
            };

            // 오버레이도 중앙 정렬
            overlay.Location = new Point(
                (this.ClientSize.Width - overlay.Width) / 2,
                (this.ClientSize.Height - overlay.Height) / 2
            );

            Label msg = new Label//승리 또는 실패 레이블
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };
            msg.Text = win ? $"승리! {FormatTime(elapsed)}" : $"실패.. {FormatTime(elapsed)}";
            overlay.Controls.Add(msg);

            Button homeBtn = new Button//시작화면으로 돌아가는 버튼 생성
            {
                Text = "메뉴로",
                Size = new Size(100, 35),
                Location = new Point((overlay.Width - 100) / 2, 70),
                Cursor = Cursors.Hand
            };
            homeBtn.Click += (s, e) =>
            {
                gameTimer?.Stop();
                InitializeStartScreen();//클릭시 초기화
            };
            overlay.Controls.Add(homeBtn);

            this.Controls.Add(overlay);
            overlay.BringToFront();
        }
    }
}