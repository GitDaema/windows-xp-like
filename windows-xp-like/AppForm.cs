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

        // ===== Clamp 가드 =====
        private bool _clampingSize;

        // ===== 폼 디자인 =====
        private Color _titleBarColor = Color.FromArgb(0, 84, 227); // 예시: Windows 10/11 블루
        private Color _titleTextColor = Color.White;

        private Form _innerForm = null;
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

            // [NEW 2] WM_SIZING (비율 강제)
            // (WM_NCHITTEST *이후*, base.WndProc *이전*에 위치)
            if (m.Msg == WM_SIZING && _isRatioLocked && _innerFormRatio > 0.0)
            {
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
                    // 너비가 변할 때: 너비를 기준으로 높이 강제 조절
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        contentHeight = (int)(contentWidth / _innerFormRatio);
                        newHeight = contentHeight + padV;
                        rect.Bottom = rect.Top + newHeight;
                        break;

                    // 높이가 변할 때: 높이를 기준으로 너비 강제 조절
                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        contentWidth = (int)(contentHeight * _innerFormRatio);
                        newWidth = contentWidth + padH;
                        rect.Right = rect.Left + newWidth;
                        break;

                    // 코너가 변할 때: 너비 기준(우선)으로 높이 조절
                    case WMSZ_TOPLEFT:
                    case WMSZ_TOPRIGHT:
                        contentHeight = (int)(contentWidth / _innerFormRatio);
                        newHeight = contentHeight + padV;
                        rect.Top = rect.Bottom - newHeight; // 상단 코너이므로 Top 위치를 보정
                        break;
                    case WMSZ_BOTTOMLEFT:
                    case WMSZ_BOTTOMRIGHT:
                        contentHeight = (int)(contentWidth / _innerFormRatio);
                        newHeight = contentHeight + padV;
                        rect.Bottom = rect.Top + newHeight; // 하단 코너이므로 Bottom 위치를 보정
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
            UpdateInnerFormLayout();
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
        public void LoadInnerForm(Form inner, bool isRatioLocked = true)
        {
            if (inner == null) return;

            _isRatioLocked = isRatioLocked;

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

            _innerForm = inner;
            inner.TopLevel = false;
            inner.FormBorderStyle = FormBorderStyle.None;
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
            Form f = c?.FindForm(); // f는 AppForm 인스턴스입니다.

            if (f != null && f is AppForm)
            {
                // 1. (기존) AppForm을 부모(desktopHost) 안에서 맨 앞으로 가져옵니다.
                f.BringToFront();

                // 2. [핵심 수정] 
                //    AppForm의 부모(desktopHost)와 그 부모(DesktopForm)를 찾습니다.
                if (f.Parent != null && f.Parent.Parent != null)
                {
                    Control desktopHost = f.Parent; // 이것이 'desktopHost' 패널입니다.
                    Form desktopForm = f.Parent.Parent as Form; // 이것이 'DesktopForm'입니다.

                    if (desktopForm != null)
                    {
                        Control taskbar = desktopForm.Controls["taskbarPanel"];
                        if (taskbar != null)
                        {
                            // [중요] 매번 Z 순서를 완벽하게 재설정합니다.

                            // 1. desktopHost를 맨 뒤로 보냅니다.
                            //    (데스크탑 아이콘들보다도 뒤로 가서 "배경" 역할을 하게 됨)
                            desktopHost.SendToBack();

                            // 2. taskbarPanel을 맨 앞으로 보냅니다.
                            //    (desktopHost와 아이콘들 모두를 덮음)
                            taskbar.BringToFront();
                        }
                    }
                }
            }
        }
    }
}