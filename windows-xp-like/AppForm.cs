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

        // ===== Clamp 가드 =====
        private bool _clampingSize;

        // ===== 폼 디자인 =====
        private Color _titleBarColor = Color.FromArgb(0, 84, 227); // 예시: Windows 10/11 블루
        private Color _titleTextColor = Color.White;

        private Form _innerForm = null;

        public AppForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimumSize = new Size(320, 200);
            this.DoubleBuffered = true;

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

            // [수정] '맨 앞으로' 훅킹 (패널 제거)
            HookBringToFront(this);
            HookBringToFront(closeButton);
            // TitleBar_Drag_MouseDown 이벤트 핸들러 제거됨

            UpdateFormRegion();
        }

        // ===== [핵심] WndProc (이동과 크기 조절 모두 담당) =====
        protected override void WndProc(ref Message m)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                base.WndProc(ref m);
                return;
            }

            if (m.Msg == WM_NCHITTEST)
            {
                // 1. 마우스 좌표를 폼 클라이언트 기준으로 변환
                Point pos = this.PointToClient(new Point(m.LParam.ToInt32()));

                // 2. [수정] 닫기 버튼 영역은 '클라이언트'로 처리 (버튼이 클릭되도록)
                if (closeButton.Bounds.Contains(pos))
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                // 3. [유지] 폼 가장자리 (크기 조절) - *가장 먼저* 확인
                bool onTopEdge = pos.Y < GRIP_SIZE;
                bool onLeftEdge = pos.X < GRIP_SIZE;
                bool onRightEdge = pos.X >= ClientSize.Width - GRIP_SIZE;
                bool onBottomEdge = pos.Y >= ClientSize.Height - GRIP_SIZE;

                // 코너
                if (onTopEdge && onLeftEdge) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (onTopEdge && onRightEdge) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (onBottomEdge && onLeftEdge) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (onBottomEdge && onRightEdge) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                // 변
                if (onTopEdge) { m.Result = (IntPtr)HTTOP; return; }
                if (onLeftEdge) { m.Result = (IntPtr)HTLEFT; return; }
                if (onRightEdge) { m.Result = (IntPtr)HTRIGHT; return; }
                if (onBottomEdge) { m.Result = (IntPtr)HTBOTTOM; return; }

                // 4. [NEW] 타이틀바 (이동)
                //    가장자리가 *아닌* 경우, 타이틀바 영역(Padding.Top)인지 확인
                if (pos.Y < this.Padding.Top)
                {
                    m.Result = (IntPtr)HTCAPTION;
                    return;
                }

                // 5. 나머지(콘텐츠 영역)
                m.Result = (IntPtr)HTCLIENT;
                return;
            }

            base.WndProc(ref m);
        }

        // ===== [NEW] 타이틀 바 그리기 =====
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 폼의 클라이언트 영역 크기
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            using (Brush titleBrush = new SolidBrush(_titleBarColor))
            {
                // 1. 타이틀바 배경 그리기 (상단 패딩 영역)
                Rectangle titleRect = new Rectangle(0, 0, width, this.Padding.Top);
                e.Graphics.FillRectangle(titleBrush, titleRect);

                // 2. [추가] 왼쪽 테두리 그리기 (좌측 패딩 영역)
                Rectangle leftRect = new Rectangle(
                    0,
                    this.Padding.Top,
                    this.Padding.Left,
                    height - this.Padding.Top - this.Padding.Bottom);
                e.Graphics.FillRectangle(titleBrush, leftRect);

                // 3. [추가] 오른쪽 테두리 그리기 (우측 패딩 영역)
                Rectangle rightRect = new Rectangle(
                    width - this.Padding.Right,
                    this.Padding.Top,
                    this.Padding.Right,
                    height - this.Padding.Top - this.Padding.Bottom);
                e.Graphics.FillRectangle(titleBrush, rightRect);

                // 4. [추가] 하단 테두리 그리기 (하단 패딩 영역)
                Rectangle bottomRect = new Rectangle(
                    0,
                    height - this.Padding.Bottom,
                    width,
                    this.Padding.Bottom);
                e.Graphics.FillRectangle(titleBrush, bottomRect);
            }

            // 5. 타이틀바 텍스트 (Padding을 고려하여 그리기)
            Rectangle textRect = new Rectangle(
                this.Padding.Left, // 왼쪽 패딩(Grip) 이후부터 텍스트 시작
                0,
                // 텍스트 너비 = 전체폭 - 좌패딩 - 우패딩 - 닫기버튼폭 - 여유공간
                width - this.Padding.Left - this.Padding.Right - closeButton.Width - 6,
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
            CenterInnerForm();
        }


        // ===== [추가] 폼의 Region을 둥글게 설정하는 메서드 =====
        private void UpdateFormRegion()
        {
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
        public void LoadInnerForm(Form inner)
        {
            if (inner == null) return;

            // [수정] 기존 폼들 제거 (이전과 동일)
            foreach (Control c in this.Controls)
            {
                if (c is Form && c != inner)
                {
                    this.Controls.Remove(c);
                    c.Dispose();
                }
            }
            _innerForm = null; // [추가] 기존 폼 참조 초기화

            inner.TopLevel = false;
            inner.FormBorderStyle = FormBorderStyle.None;

            // [핵심 수정]
            // 1. 늘어나지 않도록 Dock = None
            inner.Dock = DockStyle.None;
            // 2. 부모 크기 조절 시 위치가 마음대로 변하지 않도록 Anchor = Top, Left
            //    (또는 Anchor = None)
            inner.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // [추가] 내부 폼 참조 저장
            _innerForm = inner;

            this.Controls.Add(inner);
            inner.Show();

            HookBringToFront(inner);
            inner.SendToBack();

            // [추가] 폼을 중앙에 1차 배치
            CenterInnerForm();
        }

        // ===== [추가] 내부 폼을 중앙에 배치하는 메서드 =====
        private void CenterInnerForm()
        {
            // 로드된 폼이 없거나, 이미 닫혔으면 아무것도 안 함
            if (_innerForm == null || _innerForm.IsDisposed) return;

            // AppForm의 Padding 안쪽 영역 (컨텐츠 영역)을 가져옴
            Rectangle contentRect = this.DisplayRectangle;

            // 중앙 좌표 계산
            int x = contentRect.Left + (contentRect.Width - _innerForm.Width) / 2;
            int y = contentRect.Top + (contentRect.Height - _innerForm.Height) / 2;

            // 내부 폼의 위치 설정
            _innerForm.Location = new Point(x, y);
        }

        // ===== Close =====
        // (수정 없음)
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Parent?.Controls.Remove(this);
            this.Dispose();
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
            Control c = sender as Control;
            Form f = c?.FindForm();
            if (f != null && f is AppForm)
            {
                f.BringToFront();
            }
        }
    }
}