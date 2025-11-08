using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace windows_xp_like
{
    public partial class AppForm : Form
    {
        // ===== Windows 메시지 상수 (크기 조절 + 이동) =====
        private const int WM_NCHITTEST = 0x84;
        private const int GRIP_SIZE = 8; // 폼 가장자리의 크기 조절 영역 두께
        private const int TITLE_BAR_HEIGHT = 32; // 타이틀 바 높이

        // 히트 테스트 결과 상수
        private const int HTCLIENT = 1;      // 클라이언트 영역
        private const int HTCAPTION = 2;     // 타이틀 바 (이동)
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private const int WM_SIZING = 0x214;
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        public Control TaskbarPanel { get; set; }

        // [NEW] 창 상태 관리
        private bool _isMaximized = false;
        private Rectangle _normalBounds; // 최대화 이전 Bounds 저장

        // ===== Clamp 가드 =====
        private bool _clampingSize;

        // ===== 폼 디자인 =====
        private Color _titleBarColor = Color.FromArgb(0, 84, 227); // 예시: Windows 10/11 블루
        private Color _titleTextColor = Color.White;

        private Control _innerForm = null;
        private double _innerFormRatio = 0.0;
        private Size _innerFormMinSize = Size.Empty;
        private Size _innerFormMaxSize = Size.Empty;

        private bool _isRatioLocked = true;

        public AppForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimumSize = new Size(320, 200);
            this.DoubleBuffered = true;

            // [NEW] 최소화 버튼 생성 및 추가
            minimizeButton.Size = new System.Drawing.Size(28, 24);
            minimizeButton.UseVisualStyleBackColor = true;
            minimizeButton.Click += new System.EventHandler(this.minimizeButton_Click);
            this.Controls.Add(this.minimizeButton);

            // [NEW] 최대화 버튼 생성 및 추가
            maximizeButton.Size = new System.Drawing.Size(28, 24);
            maximizeButton.UseVisualStyleBackColor = true;
            maximizeButton.Click += new System.EventHandler(this.maximizeButton_Click);
            this.Controls.Add(this.maximizeButton);

            // [중요] 폼이 리사이즈될 때마다 OnPaint를 다시 호출
            this.ResizeRedraw = true;

            this.Padding = new Padding(
                GRIP_SIZE,              // Left
                TITLE_BAR_HEIGHT,       // Top
                GRIP_SIZE,              // Right
                GRIP_SIZE               // Bottom
            );
        }

        // ===== Lifecycle =====
        private void AppForm_Load(object sender, EventArgs e)
        {
            // [수정] 닫기 버튼은 폼에 직접 배치
            closeButton.Size = new Size(28, 24);
            closeButton.Location = new Point(
                this.ClientSize.Width - closeButton.Width - 6,
                (TITLE_BAR_HEIGHT - closeButton.Height) / 2 // Padding 영역(타이틀바)에 수직 중앙
            );
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.BringToFront();

            // [NEW] 최대화 버튼 위치 설정
            maximizeButton.Size = new Size(28, 24);
            maximizeButton.Location = new Point(
                closeButton.Location.X - maximizeButton.Width - 2, // 닫기 버튼 왼쪽에 배치
                closeButton.Location.Y
            );
            maximizeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            maximizeButton.BringToFront();

            // [NEW] 최소화 버튼 위치 설정
            minimizeButton.Size = new Size(28, 24);
            minimizeButton.Location = new Point(
                maximizeButton.Location.X - minimizeButton.Width - 2, // 최대화 버튼 왼쪽에 배치
                closeButton.Location.Y
            );
            minimizeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            minimizeButton.BringToFront();

            // [수정] '맨 앞으로' 훅킹 (패널 제거)
            HookBringToFront(this);
            HookBringToFront(closeButton);
            HookBringToFront(maximizeButton); // [NEW] 추가
            HookBringToFront(minimizeButton); // [NEW] 추가

            UpdateFormRegion();
        }

        // ===== [핵심] WndProc (이동과 크기 조절 모두 담당) =====
        protected override void WndProc(ref Message m)
        {
            // (기존 'this.WindowState == FormWindowState.Maximized' 검사는
            //  TopLevel=false 폼에서는 의미가 없으므로 무시합니다.)

            if (m.Msg == WM_NCHITTEST)
            {
                // 1. 마우스 좌표를 폼 클라이언트 기준으로 변환
                Point pos = this.PointToClient(new Point(m.LParam.ToInt32()));

                // 2. [수정] 닫기, 최대화, 최소화 버튼 영역은 항상 '클라이언트'로 처리
                if (closeButton.Bounds.Contains(pos) ||
                    maximizeButton.Bounds.Contains(pos) ||
                    minimizeButton.Bounds.Contains(pos))
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                // 3. [NEW] 최대화 상태일 때
                if (_isMaximized)
                {
                    // 3a. 타이틀바 영역 (버튼 제외)은 클라이언트로 처리 (이동 방지)
                    if (pos.Y < this.Padding.Top)
                    {
                        m.Result = (IntPtr)HTCLIENT; // HTNOWHERE도 가능
                        return;
                    }
                    // 3b. 나머지 영역(가장자리 포함)도 클라이언트로 처리 (크기 조절 방지)
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                // --- (이하는 _isMaximized가 false일 때, 즉 평상시 로직) ---

                // 4. [기존] 폼 가장자리 (크기 조절) - *가장 먼저* 확인
                bool onTopEdge = pos.Y < GRIP_SIZE;
                bool onLeftEdge = pos.X < GRIP_SIZE;
                bool onRightEdge = pos.X >= ClientSize.Width - GRIP_SIZE;
                bool onBottomEdge = pos.Y >= ClientSize.Height - GRIP_SIZE;

                // (기존 코너/변 로직 동일)
                if (onTopEdge && onLeftEdge) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (onTopEdge && onRightEdge) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (onBottomEdge && onLeftEdge) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (onBottomEdge && onRightEdge) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                if (onTopEdge) { m.Result = (IntPtr)HTTOP; return; }
                if (onLeftEdge) { m.Result = (IntPtr)HTLEFT; return; }
                if (onRightEdge) { m.Result = (IntPtr)HTRIGHT; return; }
                if (onBottomEdge) { m.Result = (IntPtr)HTBOTTOM; return; }

                // 5. [기존] 타이틀바 (이동)
                if (pos.Y < this.Padding.Top)
                {
                    m.Result = (IntPtr)HTCAPTION;
                    return;
                }

                // 6. [기존] 나머지(콘텐츠 영역)
                m.Result = (IntPtr)HTCLIENT;
                return;
            }

            // [수정] WM_SIZING (비율 강제)
            // 최대화 상태가 아닐 때만 비율 고정 로직을 실행
            if (m.Msg == WM_SIZING && !_isMaximized && _isRatioLocked && _innerFormRatio > 0.0)
            {
                // ... (기존 WM_SIZING 내부 로직은 그대로 둠) ...
                RECT rect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                int edge = m.WParam.ToInt32();

                int padH = this.Padding.Horizontal;
                int padV = this.Padding.Vertical;

                int newWidth = rect.Right - rect.Left;
                int newHeight = rect.Bottom - rect.Top;

                int contentWidth = newWidth - padH;
                int contentHeight = newHeight - padV;

                switch (edge)
                {
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        contentHeight = (int)(contentWidth / _innerFormRatio);
                        newHeight = contentHeight + padV;
                        rect.Bottom = rect.Top + newHeight;
                        break;
                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        contentWidth = (int)(contentHeight * _innerFormRatio);
                        newWidth = contentWidth + padH;
                        rect.Right = rect.Left + newWidth;
                        break;
                    case WMSZ_TOPLEFT:
                    case WMSZ_TOPRIGHT:
                        contentHeight = (int)(contentWidth / _innerFormRatio);
                        newHeight = contentHeight + padV;
                        rect.Top = rect.Bottom - newHeight;
                        break;
                    case WMSZ_BOTTOMLEFT:
                    case WMSZ_BOTTOMRIGHT:
                        contentHeight = (int)(contentWidth / _innerFormRatio);
                        newHeight = contentHeight + padV;
                        rect.Bottom = rect.Top + newHeight;
                        break;
                }

                Marshal.StructureToPtr(rect, m.LParam, true);
            }

            base.WndProc(ref m);
        }

        // ===== [NEW] 타이틀 바 그리기 =====
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // [수정] 'if (_isMaximized) { return; }' 라인을 삭제합니다.

            // 폼의 클라이언트 영역 크기
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            using (Brush titleBrush = new SolidBrush(_titleBarColor))
            {
                // 1. 타이틀바 배경 그리기 (항상)
                Rectangle titleRect = new Rectangle(0, 0, width, this.Padding.Top);
                e.Graphics.FillRectangle(titleBrush, titleRect);

                // 2. [NEW] 최대화가 아닐 때만 테두리 그리기
                if (!_isMaximized)
                {
                    // [기존] 왼쪽 테두리 그리기
                    Rectangle leftRect = new Rectangle(
                        0,
                        this.Padding.Top,
                        this.Padding.Left,
                        height - this.Padding.Top - this.Padding.Bottom);
                    e.Graphics.FillRectangle(titleBrush, leftRect);

                    // [기존] 오른쪽 테두리 그리기
                    Rectangle rightRect = new Rectangle(
                        width - this.Padding.Right,
                        this.Padding.Top,
                        this.Padding.Right,
                        height - this.Padding.Top - this.Padding.Bottom);
                    e.Graphics.FillRectangle(titleBrush, rightRect);

                    // [기존] 하단 테두리 그리기
                    Rectangle bottomRect = new Rectangle(
                        0,
                        height - this.Padding.Bottom,
                        width,
                        this.Padding.Bottom);
                    e.Graphics.FillRectangle(titleBrush, bottomRect);
                }
            }

            // 5. 타이틀바 텍스트 (항상)
            // [수정] 텍스트 영역 계산
            Rectangle textRect = new Rectangle(
                // 최대화 시 X 시작 위치는 0, 아닐 때는 패딩(Grip) 이후
                _isMaximized ? 0 : this.Padding.Left,
                0,
                // 텍스트 너비 = 전체폭 - (좌/우 패딩) - (버튼 3개 너비) - (버튼 간 간격 + 여유)
                width - (_isMaximized ? 0 : this.Padding.Left) - (_isMaximized ? 0 : this.Padding.Right)
                    - closeButton.Width - maximizeButton.Width - minimizeButton.Width - 18, // (6+2+2+8)
                this.Padding.Top // 타이틀바 높이
            );

            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, _titleTextColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        // ===== [NEW] 폼 텍스트 변경 시 다시 그리기 =====
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate(new Rectangle(0, 0, this.ClientSize.Width, this.Padding.Top)); // 타이틀바 영역만
        }


        // ===== 부모 변경 및 크기/위치 제한 (Clamp) =====
        // (수정 없음)
        private void AppForm_ParentChanged(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                this.Parent.SizeChanged -= Parent_SizeChanged;
                this.Parent.SizeChanged += Parent_SizeChanged;
            }
            ClampToParentAndSize();
        }
        private void Parent_SizeChanged(object sender, EventArgs e) { ClampToParentAndSize(); }
        private void AppForm_Move(object sender, EventArgs e) { ClampToParent(); }
        private void AppForm_SizeChanged(object sender, EventArgs e)
        {
            ClampToParentAndSize();
            UpdateFormRegion(); // (이전 둥근 모서리 코드)

            // [추가] 폼 크기가 바뀌었으므로 내부 폼도 다시 중앙 정렬
            UpdateInnerFormLayout();
        }


        // ===== [추가] 폼의 Region을 둥글게 설정하는 메서드 =====
        private void UpdateFormRegion()
        {
            if (_isMaximized)
            {
                this.Region = null;
                return;
            }

            // 둥근 모서리의 반지름
            int R = 12; // (원하는 만큼 조절)
            int D = R * 2;

            // 폼의 현재 크기
            Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);

            // [핵심] 상단만 둥근 모서리를 가진 경로(Path) 생성
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            path.StartFigure();
            // 1. Top-Left Arc
            path.AddArc(bounds.Left, bounds.Top, D, D, 180, 90);
            // 2. Top-Right Arc
            path.AddArc(bounds.Right - D, bounds.Top, D, D, 270, 90);
            // 3. Bottom-Right Corner (Square)
            path.AddLine(bounds.Right, bounds.Top + R, bounds.Right, bounds.Bottom);
            // 4. Bottom-Left Corner (Square)
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            // 5. Left Edge
            path.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Top + R);
            path.CloseFigure();

            // [결정] 생성된 경로를 폼의 새 Region으로 설정
            this.Region = new System.Drawing.Region(path);
        }

        private void ClampToParent() { ClampToParent(this.Location); }
        private void ClampToParent(Point desired)
        {
            if (this.Parent == null) { this.Location = desired; return; }
            Rectangle r = this.Parent.ClientRectangle;
            int x = Math.Max(r.Left, Math.Min(desired.X, r.Right - this.Width));
            int y = Math.Max(r.Top, Math.Min(desired.Y, r.Bottom - this.Height));
            this.Location = new Point(x, y);
        }
        private void ClampToParentAndSize()
        {
            if (_clampingSize || this.Parent == null) return;
            try
            {
                _clampingSize = true;
                Rectangle r = this.Parent.ClientRectangle;
                int w = Math.Max(this.MinimumSize.Width, Math.Min(this.Width, r.Width));
                int h = Math.Max(this.MinimumSize.Height, Math.Min(this.Height, r.Height));
                int x = Math.Max(r.Left, this.Left);
                int y = Math.Max(r.Top, this.Top);
                if (x + w > r.Right) x = Math.Max(r.Right - w, r.Left);
                if (y + h > r.Bottom) y = Math.Max(r.Bottom - h, r.Top);
                this.Bounds = new Rectangle(x, y, w, h);
            }
            finally { _clampingSize = false; }
        }

        // ===== Public: embed any inner form =====
        public void LoadInnerForm(Control inner, bool isRatioLocked = true)
        {
            if (inner == null) return;

            _isRatioLocked = isRatioLocked;

            // [수정] 기존 폼들 제거 (이전과 동일)
            foreach (Control c in this.Controls)
            {
                // 2. (c is Form) 조건을 (c != inner && c != closeButton ...)로 변경
                //    c is Form -> c is Control로 바꾸면 버튼까지 다 지워지므로
                //    내부 콘텐츠(inner)만 골라서 지우도록 조건을 수정합니다.
                if (c != inner &&
                    c != closeButton &&
                    c != maximizeButton &&
                    c != minimizeButton)
                {
                    this.Controls.Remove(c);
                    c.Dispose();
                }
            }
            _innerForm = null; // [추가] 기존 폼 참조 초기화

            _innerForm = inner;

            if (inner is Form)
            {
                // Form 타입일 때만 TopLevel과 FormBorderStyle을 설정합니다.
                Form innerAsForm = inner as Form;
                innerAsForm.TopLevel = false;
                innerAsForm.FormBorderStyle = FormBorderStyle.None;
            }

            inner.Dock = DockStyle.None;
            inner.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // [핵심 1] GameForm의 제약사항 읽기
            // GameForm의 원본 크기를 비율 계산에 사용
            _innerFormRatio = (double)inner.Width / inner.Height;
            _innerFormMinSize = inner.MinimumSize;
            _innerFormMaxSize = inner.MaximumSize; // (0, 0일 수 있음)

            // [핵심 2] AppForm의 최소/최대 크기를 GameForm에 맞게 설정
            // (Padding 크기를 더해줌)
            int padH = this.Padding.Horizontal; // (Left + Right)
            int padV = this.Padding.Vertical;   // (Top + Bottom)

            if (_innerFormMinSize.Width > 0 && _innerFormMinSize.Height > 0)
            {
                this.MinimumSize = new Size(
                    _innerFormMinSize.Width + padH,
                    _innerFormMinSize.Height + padV
                );
            }

            if (_innerFormMaxSize.Width > 0 && _innerFormMaxSize.Height > 0)
            {
                this.MaximumSize = new Size(
                    _innerFormMaxSize.Width + padH,
                    _innerFormMaxSize.Height + padV
                );
            }

            this.Controls.Add(inner);
            inner.Show();
            HookBringToFront(inner);
            inner.SendToBack();

            // [핵심 3] 레이아웃 업데이트 호출
            UpdateInnerFormLayout();
        }

        // CenterInnerForm 메서드를 아래와 같이 수정 (이름 변경 및 로직 단순화)
        private void UpdateInnerFormLayout()
        {
            if (_innerForm == null || _innerForm.IsDisposed) return;

            Rectangle contentRect = this.DisplayRectangle;
            if (contentRect.Width <= 0 || contentRect.Height <= 0) return;

            // [NEW] 스위치 확인
            if (_isRatioLocked)
            {
                // 1. [비율 고정 모드]
                // WM_SIZING이 비율을 보장하므로, 컨텐츠 영역을 꽉 채움 (스트레칭)
                _innerForm.Location = contentRect.Location;
                _innerForm.Size = contentRect.Size;
            }
            else
            {
                // 2. [자유 비율 모드] (비율 고정 해제)
                // GameForm이 찌그러지지 않도록 원본 비율을 유지하며 중앙에 배치 (레터박스)

                // 원본 폼의 가로세로 비율
                // (_innerFormRatio는 LoadInnerForm에서 이미 계산됨)
                double originalRatio = _innerFormRatio;

                // 현재 컨텐츠 영역의 가로세로 비율
                double contentRatio = (double)contentRect.Width / contentRect.Height;

                int newWidth;
                int newHeight;

                if (contentRatio > originalRatio)
                {
                    // 컨텐츠 영역이 더 넓적한 경우 -> 높이에 맞춤
                    newHeight = contentRect.Height;
                    newWidth = (int)(newHeight * originalRatio);
                }
                else
                {
                    // 컨텐츠 영역이 더 길쭉한 경우 -> 너비에 맞춤
                    newWidth = contentRect.Width;
                    newHeight = (int)(newWidth / originalRatio);
                }

                // 새 크기 적용
                _innerForm.Size = new Size(newWidth, newHeight);

                // 중앙 좌표 계산
                int x = contentRect.Left + (contentRect.Width - newWidth) / 2;
                int y = contentRect.Top + (contentRect.Height - newHeight) / 2;

                _innerForm.Location = new Point(x, y);
            }
        }

        // ===== Close =====
        // (수정 없음)
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            // 폼 숨기기 (최소화)
            this.Hide();
        }

        private void maximizeButton_Click(object sender, EventArgs e)
        {
            // [수정] 부모(desktopHost)와 주입받은 TaskbarPanel을 직접 사용합니다.
            Control desktopHost = this.Parent;
            Control taskbar = this.TaskbarPanel; // 주입받은 컨트롤 사용

            // [수정] 안전 장치
            if (desktopHost == null || taskbar == null)
            {
                // 부모나 작업표시줄 참조가 없으면 최대화/복원 로직을 실행할 수 없음
                return;
            }

            // [삭제] 기존의 폼 및 컨트롤 검색 로직을 모두 제거합니다.
            // Form desktopForm = this.FindForm(); ...
            // Control[] matches = desktopForm.Controls.Find(...); ...
            // if (taskbar == null) return; ...


            if (_isMaximized)
            {
                // Restore (복원)
                _isMaximized = false;
                // [기존] 테두리/타이틀바 Padding 복원
                this.Padding = new Padding(GRIP_SIZE, TITLE_BAR_HEIGHT, GRIP_SIZE, GRIP_SIZE);
                this.Bounds = _normalBounds; // 크기/위치 복원
                // maximizeButton.Text = "[]"; // 복원 시 아이콘
            }
            else
            {
                // Maximize (최대화)
                _isMaximized = true;
                _normalBounds = this.Bounds; // 현재 상태 저장

                // [기존] 상단 타이틀바 영역만 남김
                this.Padding = new Padding(0, TITLE_BAR_HEIGHT, 0, 0);

                // [핵심 수정]
                // Bounds를 desktopHost.ClientRectangle (전체 화면)로 설정하는 대신,
                // (desktopHost.Width, desktopHost.Height - taskbar.Height)로 설정합니다.
                Rectangle workArea = new Rectangle(
                    0, // desktopHost 기준 X
                    0, // desktopHost 기준 Y
                    desktopHost.ClientRectangle.Width,
                    desktopHost.ClientRectangle.Height - taskbar.Height // [중요] 작업 표시줄 높이 제외
                );
                this.Bounds = workArea;

                // maximizeButton.Text = "O"; // 최대화 시 아이콘
            }

            // [Z-Order 보정] (Issue 2 해결)
            // 작업 표시줄이 AppForm(이 속한 desktopHost)보다 항상 위에 있도록 Z-순서를 다시 설정합니다.
            desktopHost.SendToBack();
            taskbar.BringToFront();
        }

        // ===== BringToFront hooks =====
        // (수정 없음)
        private void HookBringToFront(Control root)
        {
            if (root == null) return;
            root.MouseDown -= Any_MouseDown_BringToFront;
            root.MouseDown += Any_MouseDown_BringToFront;
            root.ControlAdded -= Any_ControlAdded_HookBringToFront;
            root.ControlAdded += Any_ControlAdded_HookBringToFront;
            foreach (Control c in root.Controls) HookBringToFront(c);
        }

        private void Any_ControlAdded_HookBringToFront(object sender, ControlEventArgs e)
        {
            HookBringToFront(e.Control);
        }

        private void Any_MouseDown_BringToFront(object sender, MouseEventArgs e)
        {
            // 'sender'는 클릭된 컨트롤 (Button, Panel, GameForm 등)입니다.
            // 'this'는 *항상* 우리가 맨 앞으로 가져오려는 AppForm 인스턴스입니다.

            // 1. (수정) 'this' (AppForm)를 부모(desktopHost) 안에서 맨 앞으로 가져옵니다.
            this.BringToFront();

            // 2. [수정] Z-Order 보정 (maximizeButton_Click과 동일한 로직)
            //    'f.Parent' 대신 'this.Parent'를 사용하고,
            //    'Controls.Find' 대신 주입받은 'this.TaskbarPanel'을 사용합니다.

            Control desktopHost = this.Parent;
            Control taskbar = this.TaskbarPanel; // 주입받은 참조 사용

            if (desktopHost != null && taskbar != null)
            {
                // 1. desktopHost를 맨 뒤로 보냅니다.
                desktopHost.SendToBack();
                // 2. taskbarPanel을 맨 앞으로 보냅니다.
                taskbar.BringToFront();
            }
        }
    }
}