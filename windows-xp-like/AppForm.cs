using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace windows_xp_like
{
    public partial class AppForm : Form
    {
        // 마우스가 창의 어느 구역을 눌렀는지 물어볼 때 보내는 메시지
        // 어느 구역인지를 답으로 돌려줌으로써 이동이나 크기 조절 가능
        private const int WM_NCHITTEST = 0x84;

        private const int GRIP_SIZE = 8; // 폼 가장자리의 크기 조절 영역 두께
        private const int TITLE_BAR_HEIGHT = 32; // 타이틀 바 높이

        // 위의 NCHHITTEST, 즉 히트 테스트 결과값
        private const int HTCLIENT = 1; // 일반 클라이언트 영역, 드래그도 그냥 클릭 
        private const int HTCAPTION = 2; // 드래그가 곧 이동인 타이틀 바

        // 어느 면에서 크기 조절 중인지에 대한 히트 테스트 결과
        // 즉, 위치 판정 결과이니 OS가 어떤 커서로 바꿀지를 결정하도록 돕는 코드
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        // 크기 조절 중일 때 계속 보내는 메시지
        // 크기나 비율 보정은 사실 이 메시지를 이용해서 강제하는 것
        private const int WM_SIZING = 0x214;

        // 어느 면에서 크기 조절 중인지에 대한 WM_SIZING의 메시지
        // 즉, 현재 동작 상황을 나타내는 코드
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;

        // Win32에서 사용하는 좌표용 사각형 구조체 
        // 지금 쓰는 C#과 Win32 간에 값을 읽고 쓰기 위해 설정 

        // StructLayout은 C# 구조체가 메모리에 어떻게 저장될지 정하는 설정
        // LayoutKind가 Sequential이면 작성 순서대로 저장, Auto는 C#의 자동 배치
        // 이게 필요한 이유는 윈도우와 데이터를 주고받을 때 메모리 모양이 같아야 하기 때문
        // 구조체가 순서에 맞게 4바이트씩 저장되니 윈도우 API에서 그대로 사용 가능 
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        public Control TaskbarPanel { get; set; }

        // [NEW] 창 상태 관리
        private bool _isMaximized = false;
        private Rectangle _normalBounds; // 최대화 이전 Bounds 저장

        // ===== Clamp 가드 =====
        private bool _clamping;

        // ===== 폼 디자인 =====
        // 윈도우 XP 창의 시작 색(밝은 파랑)
        private Color _titleBarBrightColor = Color.FromArgb(56, 137, 255);
        private Color _titleBarColor = Color.FromArgb(0, 88, 238);

        private Color _titleTextColor = Color.White;

        private Control _innerForm = null;
        private double _innerFormRatio = 0.0;
        private Size _innerFormMinSize = Size.Empty;
        private Size _innerFormMaxSize = Size.Empty;

        private bool _isRatioLocked = true;

        private BringToFrontFilter _bringFilter;

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

            UpdateFormRegion();
        }

        /// <summary>
        /// 메시지를 가로채 규칙에 맞게 보정하도록 오버라이드한 메서드
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // 메시지가 마우스 히트 테스트면
            if (m.Msg == WM_NCHITTEST)
            {
                // 마우스 좌표를 폼 클라이언트 기준으로 변환해 저장
                // 메시지에 저장된 정수를 우선 32비트 부호 있는 정수로 변환해야 사용 가능 
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));

                // 창 기능 버튼 위 혹은 최대화 상태인지 검사
                // 만약 맞다면 일반 영역으로 취급하고 넘어가야만 충돌하지 않음
                // 어차피 이 상태라면 아래에 있는 폼 가장자리 조절 등이 의미 없으니 패스
                if (closeButton.Bounds.Contains(pos) ||
                    maximizeButton.Bounds.Contains(pos) ||
                    minimizeButton.Bounds.Contains(pos) ||
                    _isMaximized)
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                // 크기 조절을 위한 폼 가장자리 판정
                // 크기 조절 영역이 너무 두꺼우면 true가 되는 판정 영역도 넓어지니 주의
                bool onTopEdge = pos.Y < GRIP_SIZE;
                bool onLeftEdge = pos.X < GRIP_SIZE;
                bool onRightEdge = pos.X >= ClientSize.Width - GRIP_SIZE;
                bool onBottomEdge = pos.Y >= ClientSize.Height - GRIP_SIZE;

                // 일단 대각선 먼저 체크하고, 아닐 경우 변으로 이동
                // 결과로 돌려주는 값에 따라 OS가 자동으로 커서 모양과 크기 조절 동작
                if (onTopEdge && onLeftEdge) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (onTopEdge && onRightEdge) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (onBottomEdge && onLeftEdge) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (onBottomEdge && onRightEdge) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                if (onTopEdge) { m.Result = (IntPtr)HTTOP; return; }
                if (onLeftEdge) { m.Result = (IntPtr)HTLEFT; return; }
                if (onRightEdge) { m.Result = (IntPtr)HTRIGHT; return; }
                if (onBottomEdge) { m.Result = (IntPtr)HTBOTTOM; return; }

                // 이 시점에서 타이틀 바 높이 안에 y 좌표가 들어와 있다는 뜻은 이동 판정
                // 닫기, 최소화, 최대화 버튼 위가 아님도 미리 검사했으므로 무관
                if (pos.Y < Padding.Top)
                {
                    m.Result = (IntPtr)HTCAPTION;
                    return;
                }

                // 그 외에는 전부 일반 클라이언트로
                m.Result = (IntPtr)HTCLIENT;
                return;
            }

            // 만약 창 크기 조절을 위해 드래그 중이라면, 이 메시지 조건은 프레임마다 계속 true
            // 최대화 상태가 아니고, 비율 고정 모드이며, 내부 컨트롤의 비율이 정상값(양수)일 경우
            // 크기 조절 중 비율 고정을 위한 강제 조정 작업 수행
            if (m.Msg == WM_SIZING && !_isMaximized && _isRatioLocked && _innerFormRatio > 0.0)
            {
                // WM_SIZING 메시지는 윈도우의 새 크기 후보인 Rect를 포인터로 전달
                // 이 값은 RECT 형태이니 포인터를 구조체로 바꿔줘야 사용 가능
                // 즉, 창이 어떻게 바뀔지에 대한 좌표값을 얻어오는 것이 목표 
                RECT rect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                // 어느 방향으로 크기 조절 중인지에 대한 정수 코드를 32비트 부호 있는 정수로 받아오기
                int edge = m.WParam.ToInt32();

                int padH = Padding.Horizontal;
                int padV = Padding.Vertical;

                int newWidth = rect.Right - rect.Left;
                int newHeight = rect.Bottom - rect.Top;

                // 여백, 즉 테두리 두께를 제외한 실제 콘텐츠 크기 저장
                int contentWidth = newWidth - padH;
                int contentHeight = newHeight - padV;

                // 크기 조절 방향에 따라 비율을 맞추는 기준이 달라지므로 switch문
                switch (edge)
                {
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        // 왼쪽이나 오른쪽이면 가로가 바뀌는 거니 세로를 비율에 맞게 계산
                        // 세로는 곧 가로를 비율로 나누어 상쇄한 것
                        // 그리고 창의 하단이 새 높이에 맞게 조정되도록 강제
                        contentHeight = (int)(contentWidth / _innerFormRatio);
                        newHeight = contentHeight + padV;
                        rect.Bottom = rect.Top + newHeight;
                        break;
                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        // 위나 아래면 세로가 바뀌는 거니 가로를 비율에 맞게 계산
                        // 가로는 곧 세로를 비율로 곱해 상쇄한 것
                        // 그리고 창의 오른쪽이 새 폭에 맞게 조정되도록 강제
                        contentWidth = (int)(contentHeight * _innerFormRatio);
                        newWidth = contentWidth + padH;
                        rect.Right = rect.Left + newWidth;
                        break;

                    // 나머지 경우들은 가로와 세로가 동시에 변하는 대각선
                    // 비율 계산 방법은 세로, 조정될 기준은 가로 방향에 따라 결정
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

                // 수정한 RECT를 OS에 돌려줘서 원래 크기 말고 이걸로 반영하도록 강제 명령
                Marshal.StructureToPtr(rect, m.LParam, true);
            }

            // 나머지 메시지들은 그대로 수행
            base.WndProc(ref m);
        }

        /// <summary>
        /// 타이틀 바를 그리기 위해 기본 그리기 메서드를 상속 받아 새로운 동작 추가
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int width = ClientSize.Width;
            int height = ClientSize.Height;

            // 타이틀 바 배경을 그리되, 색상은 아래 브러시에서 설정
            Rectangle titleRect = new Rectangle(0, 0, width, Padding.Top);

            // 브러시 활용은 using을 사용해주는 것이 좋음
            // 사용 후 자동으로 Dispose() 호출해 메모리 정리
            // 그라데이션 브러시를 생성하려면 시작 색, 끝 색, 방향을 정해줘야 함
            // 여기에서는 시작 색이 위쪽, 끝 색이 아래쪽
            using (LinearGradientBrush titleBrush = new LinearGradientBrush(
                titleRect,
                _titleBarBrightColor, // 시작 색은 밝은 파랑
                _titleBarColor,   // 끝 색은 기본 파랑
                LinearGradientMode.Vertical  // 방향은 수직
            ))
            {
                e.Graphics.FillRectangle(titleBrush, titleRect);
            } 

            // 나머지 테두리에는 그라데이션 없이 기본 색상으로 브러시 설정
            using (Brush titleBrush = new SolidBrush(_titleBarColor))
            {
                // 최대화가 아닐 때만 테두리를 그려야 하니 검사
                if (!_isMaximized)
                {
                    // 왼쪽 테두리 그리기
                    Rectangle leftRect = new Rectangle(
                        0, Padding.Top,
                        Padding.Left, height - Padding.Top - Padding.Bottom);
                    e.Graphics.FillRectangle(titleBrush, leftRect);

                    // 오른쪽 테두리 그리기
                    Rectangle rightRect = new Rectangle(
                        width - Padding.Right, Padding.Top,
                        Padding.Right, height - Padding.Top - Padding.Bottom);
                    e.Graphics.FillRectangle(titleBrush, rightRect);

                    // 하단 테두리 그리기
                    Rectangle bottomRect = new Rectangle(
                        0, height - Padding.Bottom,
                        width, Padding.Bottom);
                    e.Graphics.FillRectangle(titleBrush, bottomRect);
                }
            }

            // 타이틀 바의 텍스트도 항상 그려줘야 하니 우선 사각형 공간 확보
            // 만약 창 최대화 중이면 왼쪽과 오른쪽 여백은 없으므로 실제 여백 0으로 맞추기
            int realPaddingLeft, realPaddingRight;
            if (_isMaximized)
            {
                realPaddingLeft = 0;
                realPaddingRight = 0;
            }
            else
            {
                realPaddingLeft = Padding.Left;
                realPaddingRight = Padding.Right;
            }

            // 텍스트 너비 = 전체 폭 - (좌우 여백) - (버튼 3개 너비) - (버튼 간 간격 + 여유)
            Rectangle textRect = new Rectangle(
                realPaddingLeft, 0,
                width - realPaddingLeft - realPaddingRight
                - closeButton.Width - maximizeButton.Width - minimizeButton.Width - 18,
                Padding.Top
            );

            // 생성한 사각형 자리에 알맞게 텍스트 그리기
            // TextFormatFlags는 텍스트 정렬이나 잘림 처리 방식 제어용
            // 컨트롤의 높이가 글자보다 클 때 세로 축 중앙으로 오도록 하려면 VerticalCenter
            // 텍스트를 왼쪽 정렬하기 위해 Left
            // 텍스트가 너무 길면 '...'와 같이 줄임표로 자르기
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, _titleTextColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// 폼 텍스트가 변경되면 자동으로 호출되는 메서드를 상속 받아 다시 그리기
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // 타이틀 바 영역만 다시 그리면 되므로 Invalidate
            // 지정한 영역을 무효화 후 그 부분만 다시 그리도록 OnPaint() 호출하는 역할
            Invalidate(new Rectangle(0, 0, ClientSize.Width, Padding.Top));
        }


        /// <summary>
        /// 앱 폼의 부모가 바뀔 때 자동 크기 조절 이벤트를 연결하고 한 번 조절하는 메서드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppForm_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // 부모의 창 크기가 바뀔 때 앱 폼도 자동 조절되도록 이벤트 연결
                // 단, 같은 이벤트가 중복되지 않도록 안전하게 기존 연결 제거 후 다시 연결
                Parent.SizeChanged -= Parent_SizeChanged;
                Parent.SizeChanged += Parent_SizeChanged;
            }

            ClampToParentAndSize();
        }

        /// <summary>
        /// 부모의 창 크기가 바뀔 때 호출되는 메서드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Parent_SizeChanged(object sender, EventArgs e) 
        { 
            ClampToParentAndSize(); // 현재는 조정 메서드 호출용으로만 사용
        }

        /// <summary>
        /// 앱 폼이 움직일 때 부모의 클라이언트 영역을 벗어나지 않도록 하는 메서드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppForm_Move(object sender, EventArgs e) 
        {
            ClampToParentAndSize(); // 현재는 조정 메서드 호출용으로만 사용
        }

        /// <summary>
        /// 앱 폼의 크기가 바뀔 때 자동으로 크기, 상단바 모양, 내부 폼을 조정하는 메서드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppForm_SizeChanged(object sender, EventArgs e)
        {
            ClampToParentAndSize();
            UpdateFormRegion();

            // 폼 크기가 바뀌면 내부 폼에도 영향을 미치니 반드시 업데이트
            UpdateInnerFormLayout(); 
        }

        // 끝이 둥근 상단바 모양을 유지하도록 업데이트하는 메서드
        private void UpdateFormRegion()
        {
            if (_isMaximized)
            {
                this.Region = null;
                return;
            }

            int R = 12; // 둥근 모서리의 반지름
            int D = R * 2; // 지름

            // 폼의 현재 크기
            Rectangle bounds = new Rectangle(0, 0, Width, Height);

            // 상단만 둥근 모서리를 가진 패스 준비 
            System.Drawing.Drawing2D.GraphicsPath path 
                = new System.Drawing.Drawing2D.GraphicsPath();

            path.StartFigure(); // 경로 시작

            // 왼쪽 위에 설정한 지름대로 원을 180 ~ 270도까지만 그려서 둥근 모서리 생성
            path.AddArc(bounds.Left, bounds.Top, D, D, 180, 90);
            // 오른쪽 위에도 마찬가지로 270 ~ 0도까지만 그려서 둥근 모서리 생성
            path.AddArc(bounds.Right - D, bounds.Top, D, D, 270, 90);
            // 지금은 오른쪽 모서리 끝에 있으므로 둥근 부분 바로 아래에서 시작하려면 Top + R
            // 이후 밑으로 쭉 내려서 창 오른쪽 면 그리기
            path.AddLine(bounds.Right, bounds.Top + R, bounds.Right, bounds.Bottom);
            // 창 밑면 그리기
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            // 마찬가지로 둥근 부분 아래까지만 위로 그어 창 왼쪽 면 그리기
            path.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Top + R);

            path.CloseFigure(); // 경로 끝

            // 생성된 경로를 폼의 새 Region으로 설정해 모양 확정
            Region = new Region(path);
        }

        /// <summary>
        /// 부모 폼 영역에서 벗어나지 않도록 크기와 위치를 자동 보정하는 메서드
        /// </summary>
        private void ClampToParentAndSize()
        {
            if (_clamping || Parent == null) return;
            try
            {
                _clamping = true;
                Rectangle r = Parent.ClientRectangle;
                Bounds = ClampBounds(Bounds, r, MinimumSize);
            }
            finally { _clamping = false; }
        }

        /// <summary>
        /// 앱 폼과 부모의 크기 및 위치를 입력받아 부모를 넘지 않도록 조절된 값을 반환하는 메서드  
        /// </summary>
        /// <param name="current"></param>
        /// <param name="parentClient"></param>
        /// <param name="minSize"></param>
        /// <returns></returns>
        private Rectangle ClampBounds(Rectangle current, Rectangle parentClient, Size minSize)
        {
            // 적어도 크기는 최소 폭과 최소 높이 이상이며, 부모 폭 및 높이 이하
            // 자기 크기를 부모 크기와 비교해서 둘 중 작은 쪽을 고른 뒤 최소 크기보다는 크게 조정
            int w = Math.Max(minSize.Width, Math.Min(current.Width, parentClient.Width));
            int h = Math.Max(minSize.Height, Math.Min(current.Height, parentClient.Height));

            // 좌표가 Top Left 기준이므로, 우선 x, y는 반드시 부모의 left와 top 이상
            // 거기에 부모의 right와 bottom을 기준으로 앱 폼의 폭과 높이를 뺀 것의 이하
            // 예시로 부모 폭이 500이고 자식 폭이 200일 때, x가 300이면 딱 오른쪽에 붙은 것
            // 따라서 제약 1은 왼쪽 위, 제약 2는 오른쪽 밑을 넘어가지 못하게 막음
            int x = Math.Max(parentClient.Left, Math.Min(current.Left, parentClient.Right - w));
            int y = Math.Max(parentClient.Top, Math.Min(current.Top, parentClient.Bottom - h));

            return new Rectangle(x, y, w, h);
        }

        /// <summary>
        /// 앱 폼 창 내부에 폼 또는 유저 컨트롤을 가져와 실행하는 메서드
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="isRatioLocked"></param>
        public void LoadInnerForm(Control inner, bool isRatioLocked = true)
        {
            if (inner == null) return;

            _isRatioLocked = isRatioLocked;

            foreach (Control c in Controls) // 우선 모든 컨트롤 탐색
            {
                // 만약 그 컨트롤이 새로 넣을 컨트롤과 다르면서 창 컨트롤 버튼들이 아니라면
                if (c != inner &&
                    c != closeButton &&
                    c != maximizeButton &&
                    c != minimizeButton)
                {
                    // 내부에 이미 폼 또는 유저 컨트롤이 있었다는 뜻이므로 제거
                    Controls.Remove(c);
                    c.Dispose();
                }
            }

            _innerForm = inner; // 이후 기존 폼 초기화

            if (inner is Form) // 만약 넣을 컨트롤이 폼이라면
            {
                Form innerAsForm = inner as Form;
                innerAsForm.TopLevel = false; // 최상위 요소가 아니도록 설정
                // 테두리도 앱 폼의 테두리로 대체해야 하니 없도록 설정
                innerAsForm.FormBorderStyle = FormBorderStyle.None;
            }
            // 폼이든 유저 컨트롤이든 Dock, Anchor은 기본값으로 초기화
            inner.Dock = DockStyle.None;
            inner.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // 내부에 넣을 컨트롤의 크기 비율과 최소 최대 크기 정보 저장
            _innerFormRatio = (double)inner.Width / inner.Height;
            _innerFormMinSize = inner.MinimumSize;
            _innerFormMaxSize = inner.MaximumSize; // (0, 0일 수 있음)

            // 틀의 두께, 즉 여백의 가로와 세로 길이 저장
            int padH = Padding.Horizontal;
            int padV = Padding.Vertical;


            // 이후 내부 컨트롤의 최소 크기와 두께를 더해서 AppForm의 최소 크기 변경
            // 이러면 AppForm 창을 최대한 줄여도 내부 컨트롤의 최소 크기 유지
            if (_innerFormMinSize.Width > 0 && _innerFormMinSize.Height > 0)
            {
                MinimumSize = new Size(
                    _innerFormMinSize.Width + padH,
                    _innerFormMinSize.Height + padV
                );
            }

            // 최대 크기도 마찬가지로 계산
            if (_innerFormMaxSize.Width > 0 && _innerFormMaxSize.Height > 0)
            {
                MaximumSize = new Size(
                    _innerFormMaxSize.Width + padH,
                    _innerFormMaxSize.Height + padV
                );
            }

            Controls.Add(inner);
            inner.Show();

            // 내부 컨트롤 자체는 앱 폼보다 뒤로 오도록 맨 뒤로 넘기기
            inner.SendToBack();

            UpdateInnerFormLayout(); 
        }

        /// <summary>
        /// 내부 컨트롤이 앱 폼 안에서 배치되는 크기를 비율 등을 고려해 업데이트하는 메서드
        /// </summary>
        private void UpdateInnerFormLayout()
        {
            if (_innerForm == null || _innerForm.IsDisposed) return;

            // 앱 폼의 가상 표시 영역을 가져와 저장
            Rectangle contentRect = DisplayRectangle;

            // 그럴 일은 거의 없겠지만, 폭 또는 높이가 0 이하면 조정 불가
            if (contentRect.Width <= 0 || contentRect.Height <= 0) return;

            if (_isRatioLocked) // 만약 비율 고정 모드가 켜져 있을 경우
            {
                // WndProc에서 WM_SIZING 메시지를 가로채서 강제로 조절 중인 상태
                // WM_SIZING이 비율을 보장하므로 그냥 컨텐츠 영역을 꽉 채우면 끝
                _innerForm.Location = contentRect.Location;
                _innerForm.Size = contentRect.Size;
            }
            else // 비율 고정 모드가 꺼져 있을 경우
            {
                // 내부 컨트롤의 원본 비율을 유지하며 중앙에 배치하는 것이 목표

                // 원본 앱 폼의 가로세로 비율 저장
                double originalRatio = _innerFormRatio;

                // 현재 콘텐츠 영역의 가로 세로 비율 계산
                double contentRatio = (double)contentRect.Width / contentRect.Height;

                int newWidth;
                int newHeight;

                // 만약 컨텐츠 영역의 비율이 더 크다면, 폭이 더 길다는 뜻
                // 즉, 높이 기준으로 일단 채우고 남는 좌우 공간은 비우기
                if (contentRatio > originalRatio)
                {
                    newHeight = contentRect.Height;
                    // 새로운 높이에 맞춰 원본 앱 폼의 비율을 곱해주면 상쇄되어 폭
                    newWidth = (int)(newHeight * originalRatio);
                }
                else 
                {
                    // 반대로 비율이 작은 건 높이가 더 길다는 뜻
                    // 폭 기준 채우고 상하 공간 비우기
                    newWidth = contentRect.Width;
                    // 마찬가지로 새 폭에 맞춰 원본 앱 폼의 비율을 나눠주면 상쇄되어 높이
                    newHeight = (int)(newWidth / originalRatio);
                }

                _innerForm.Size = new Size(newWidth, newHeight);

                // 중앙 배치를 위한 좌표 계산, 왼쪽 위 기준이니 각각 Left와 Top으로 시작
                // 폭 또는 높이 중 차이가 발생한 쪽은 차이의 절반만큼 더해주어야 빈 공간 반영한 중앙
                int x = contentRect.Left + (contentRect.Width - newWidth) / 2;
                int y = contentRect.Top + (contentRect.Height - newHeight) / 2;

                _innerForm.Location = new Point(x, y);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            // 부모인 데스크톱이 가진 컨트롤에서 이 앱폼 제거
            if (Parent != null)
                Parent.Controls.Remove(this); 

            Dispose(); // 이 앱 폼 끄기
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            Hide(); // 숨기는 것이 곧 최소화
        }

        private void maximizeButton_Click(object sender, EventArgs e)
        {
            Control desktopHost = Parent;
            Control taskbar = TaskbarPanel;

            if (desktopHost == null || taskbar == null) return;

            if (_isMaximized) // 만약 이미 최대화 상태라면 해제
            {
                _isMaximized = false;
                // 기본 여백 및 최대화할 때 저장했던 크기 위치 상태로 복귀
                Padding = new Padding(GRIP_SIZE, TITLE_BAR_HEIGHT, GRIP_SIZE, GRIP_SIZE);
                Bounds = _normalBounds;
            }
            else
            {
                _isMaximized = true;
                // 나중에 최대화를 해제했을 때 돌아갈 크기 위치 상태 저장
                _normalBounds = Bounds;

                // 상단 타이틀바 영역만 남기고 나머지 다 0으로
                Padding = new Padding(0, TITLE_BAR_HEIGHT, 0, 0);

                // 최대화했을 때 커져야 하는 영역은 우선 왼쪽 위 중심이니 0, 0 시작
                // 단, 작업 표시줄을 가리지 않도록 데스크탑 높이에서 작업 표시줄 높이만큼 빼야 함
                Rectangle workArea = new Rectangle(
                    0, // desktopHost 기준 X
                    0, // desktopHost 기준 Y
                    desktopHost.ClientRectangle.Width,
                    desktopHost.ClientRectangle.Height - taskbar.Height);

                // 계산한 대로 최대화 크기 및 위치로 변경
                Bounds = workArea;
            }

            // 작업 표시줄이 앱 폼이 속한 desktophost보다 위에 있도록 다시 설정
            desktopHost.SendToBack();
            taskbar.BringToFront();
        }

        // 핸들은 운영체제가 만든 창에 접근할 수 있도록 돌려주는 번호
        // 즉, 새 창이 처음 Show()될 때, 핸들이 생기면서 자동 호출
        // 메시지 필터 등록은 핸들이 있을 때만 유효한 작업
        // 생성 시점에 한 번만 하는 초기화 코드를 생성자에 넣으면 누락 또는 중복 가능성
        // 따라서 오버라이드로 핸들이 생성/파괴될 때 메시지 필터 제어를 하도록 추가

        /// <summary>
        /// 창을 맨 앞으로 올리는 검사용 메시지 필터 클래스를 핸들 생성과 함께 등록
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _bringFilter = new BringToFrontFilter(this);
            Application.AddMessageFilter(_bringFilter);
        }

        /// <summary>
        /// 창을 맨 앞으로 올리는 검사용 메시지 필터 클래스를 핸들 파괴와 함께 해제
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_bringFilter != null)
            {
                Application.RemoveMessageFilter(_bringFilter);
                _bringFilter = null;
            }
            base.OnHandleDestroyed(e);
        }
    }

    /// <summary>
    /// 클릭된 윈도우 핸들을 검사해 클릭된 창의 순서를 맨 앞으로 옮기는 메시지 필터 클래스
    /// </summary>
    public sealed class BringToFrontFilter : IMessageFilter
    {
        private const int WM_LBUTTONDOWN = 0x0201; // 좌클릭 메시지 상수 저장
        private readonly AppForm _form;

        public BringToFrontFilter(AppForm form)
        {
            _form = form;
        }

        public bool PreFilterMessage(ref Message m)
        {
            // 메시지가 좌클릭이 아니면 무시
            if (m.Msg != WM_LBUTTONDOWN) return false;

            // 좌클릭된 윈도우 핸들과 연결된 컨트롤 가져오기
            Control hit = Control.FromHandle(m.HWnd);
            if (hit == null) return false; // 없으면 무시

            // 좌클릭이 이 앱 폼 또는 이 앱 폼이 보유 중인 컨트롤에서 일어났다면
            if (hit == _form || _form.Contains(hit))
            {
                _form.BringToFront(); // 폼을 맨 위로 올리기

                Control desktopHost = _form.Parent;
                Control taskbar = _form.TaskbarPanel;
                
                // 바탕화면에서 항상 배경화면은 맨 뒤, 작업 표시줄은 맨 앞
                if (desktopHost != null) desktopHost.SendToBack();
                if (taskbar != null) taskbar.BringToFront();
            }

            return false;
        }
    }
}