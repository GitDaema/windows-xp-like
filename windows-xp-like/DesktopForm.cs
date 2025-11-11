using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace windows_xp_like
{
    public partial class DesktopForm : Form
    {
        // 폴더 탐색을 위해 모든 폴더의 데이터를 관리하는 딕셔너리
        // 폴더 키를 이용해 폴더 데이터 객체를 찾아 정보를 꺼내 적용하는 방식으로 사용할 예정
        private Dictionary<string, FolderData> _fileSystemData = new Dictionary<string, FolderData>();

        private Label _selectedIcon = null; // 현재 바탕화면에서 선택된 아이콘 레이블

        private Color taskFlowBarUpColor = Color.FromArgb(56, 137, 255);
        private Color taskFlowBarDownColor = Color.FromArgb(0, 48, 208);

        public DesktopForm()
        {
            InitializeComponent();
        }
        private void DesktopForm_Load(object sender, EventArgs e)
        {
            // 루트 디렉토리에 파일 생성해 초기화
            InitFileSystem();
            // 보이는 순서가 작업 표시줄이 제일 위로, 바탕화면이 제일 뒤로 가도록 설정
            taskbarPanel.BringToFront();
            desktopHost.SendToBack();

            // 바탕화면의 요소를 담는 데스크탑 호스트가 가진 모든 컨트롤 순회
            foreach (Control control in desktopHost.Controls)
            {
                // 만약 컨트롤이 레이블이면서, 태그가 정상적으로 존재한다면
                if (control is Label iconLabel && iconLabel.Tag != null)
                {
                    // 필수 아이콘 이벤트를 자동으로 연결
                    iconLabel.Click += Icon_Click;
                    iconLabel.MouseEnter += Icon_MouseEnter;
                    iconLabel.MouseLeave += Icon_MouseLeave;

                    // 아이콘의 텍스트 그림자 효과를 직접 그리기 위한 이벤트 연결
                    iconLabel.Paint += new PaintEventHandler(IconLabel_Paint);
                }
            }

            // 현재 시간을 알려주기 위한 타이머 시작 및 업데이트
            clockTimer.Start();
            UpdateClock();
        }

        private void InitFileSystem()
        {
            _fileSystemData.Clear();

            //_fileSystemData.Add("ROOT", new FolderData("바탕 화면", null, new List<FileSystemItem>
            //{
            //    new FileSystemItem("내 문서", "NAVIGATE_DOCS"),
            //    new FileSystemItem("Game.exe", new GameForm())
            //}));

            _fileSystemData.Add("NAVIGATE_DOCS", new FolderData("내 문서", null, new List<FileSystemItem>
            {
                new FileSystemItem("새 폴더", "NAVIGATE_NEWFOLDER"),
                new FileSystemItem("Game.exe", new GameForm())
            }));

            _fileSystemData.Add("NAVIGATE_NEWFOLDER", new FolderData("새 폴더", "NAVIGATE_DOCS", new List<FileSystemItem>
            {
                new FileSystemItem("비어있음.txt", false)
            }));
        }

        // 바탕화면 테스트용 앱 아이콘 버튼 3개에 대한 각각의 클릭 이벤트
        private void appIcon1_DoubleClick(object sender, EventArgs e)
        {
            LaunchAppFromIcon(new GameForm(), "App A", new Point(40, 40), new Size(520, 360), true, appIcon1.Image);
        }

        private void appIcon2_DoubleClick(object sender, EventArgs e)
        {
            LaunchAppFromIcon(new GameForm(), "App B", new Point(120, 80), new Size(520, 360), false, appIcon2.Image);
        }

        private void folderIcon1_DoubleClick(object sender, EventArgs e)
        {
            LaunchAppFromIcon(new FolderView(), "내 문서", new Point(100, 60), new Size(450, 350), true, folderIcon1.Image, "NAVIGATE_DOCS");
        }

        /// <summary>
        /// 생성한 AppForm을 desktopHost에 넣어서 반환하는 메서드 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private AppForm CreateAppForm(Point location, Size size, string title = "NoNameApp", Image icon = null)
        {
            // 새 앱 폼을 만들면서 초기화
            // 최상위 요소면 추가가 불가능하니 TopLevel은 false로
            AppForm f = new AppForm
            {
                TopLevel = false,
                StartPosition = FormStartPosition.Manual,
                Location = location,
                Size = size,
                IconImage = icon,
            };

            // 이름은 ID 역할, Text는 변경 가능성이 높으므로 ID는 아니지만 같은 텍스트로 초기화
            f.Name = title;
            f.Text = title;
            f.Tag = f.Text; // 텍스트 그대로 태그 설정
            f.TaskbarPanel = taskbarPanel; // 접근 가능하도록 작업 표시줄 전달

            // 작업 표시줄에 방금 열린 창을 나타내는 버튼 생성
            Button taskButton = new Button();
            taskButton.Text = f.Text;
            taskButton.AutoSize = true;

            // 버튼의 모양, 글자 색 및 정렬 등 기본 스타일 초기화
            taskButton.FlatStyle = FlatStyle.Standard;
            taskButton.FlatAppearance.BorderSize = 0;
            taskButton.ForeColor = Color.White;
            taskButton.TextAlign = ContentAlignment.MiddleCenter;
            taskButton.AutoSize = false;
            taskButton.Size = new Size(160, 28);
            taskButton.Margin = new Padding(1, 1, 1, 1);

            // 동적으로 생성된 컨트롤이므로 코드에서 직접 페인트 이벤트 연결
            taskButton.Paint += new PaintEventHandler(taskButton_Paint);

            // 작업 표시줄 버튼이 검색 없이도 연결된 폼을 알 수 있도록 태그로 설정
            taskButton.Tag = f;

            // 앱 폼은 작업 표시줄 버튼의 존재를 모르기 때문에 디자이너뷰에서 이벤트 추가 불가
            // 앱 폼에 버튼 변수를 만들어 전달하는 건 서로를 참조하는 구조라서 과한 의존성 발생
            // 따라서 람다 식을 이용한 이벤트 구독 방식을 이용
            // 별도의 메서드가 불필요하고 지역 변수에 직접 접근하는 실행문을 등록 가능

            // 폼 보이는 여부에 따라 버튼 색이 바뀌어야 하므로 다시 그리도록 신호 보내기
            f.VisibleChanged += (formSender, formArgs) =>
            {
                taskButton.Invalidate(); 
            };

            // 작업 표시줄 버튼 클릭 이벤트도 마찬가지로 이벤트 구독 방식
            taskButton.Click += (sender, args) =>
            {
                // 앱 폼을 찾을 때는 미리 전달해 놓은 태그로 가져오기
                Form appForm = (sender as Button).Tag as Form;

                // 태그에 등록된 폼이 없거나 삭제되었으면
                if (appForm == null || appForm.IsDisposed) return; 

                if (!appForm.Visible) // 클릭했을 때 최소화 상태라면
                {
                    // 최소화 상태를 풀어야 하니 Show, 창 보이는 순서 제일 앞으로 변경
                    appForm.Show();
                    appForm.BringToFront();

                    taskbarPanel.BringToFront();
                    desktopHost.SendToBack();
                }
                else // 이미 보이는 상태일 때는
                {
                    appForm.Hide(); // 최소화해야 하니 Hide
                }
            };

            // 작업 표시줄 패널에 방금 만든 작업 표시줄 버튼 추가
            taskFlowPanel.Controls.Add(taskButton);

            // 앱 폼이 닫힐 때 자동으로 작업 표시줄 버튼도 제거하도록 이벤트 구독
            f.Disposed += (sender, args) =>
            {
                taskFlowPanel.Controls.Remove(taskButton);
                taskButton.Dispose();
            };

            // 바탕화면 컨트롤 저장소에 실제로 폼 추가
            desktopHost.Controls.Add(f);

            // 새로 열었으므로 우선 Show 및 창 순서 가장 위로
            f.BringToFront();
            f.Show();

            desktopHost.SendToBack();
            taskbarPanel.BringToFront();

            return f;
        }

        /// <summary>
        /// 바탕화면 아이콘에서 앱을 실행하고, 실행 직후 아이콘 선택을 해제해 주는 메서드
        /// </summary>
        private void LaunchAppFromIcon(Control actionControl, string name, Point location, Size size, bool keepAspect, Image icon, string folderStartKey = "ROOT")
        {
            LaunchApp(actionControl, name, location, size, keepAspect, icon, folderStartKey);

            desktopHost_Click(null, null);
        }

        /// <summary>
        /// 새로 생성된 컨트롤을 받아서 AppForm과 연결해 창으로 보여주는 메서드 
        /// </summary>
        /// <param name="actionControl"></param>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="keepAspect">창 크기 조절할 때 특정 비율을 지켜야 하면 true</param>
        private void LaunchApp(Control actionControl, string name, Point location, Size size, bool keepAspect, Image icon, string folderStartKey = "ROOT")
        {
            // 만약 새로 생성할 컨트롤이 없거나 이름이 없으면
            if (actionControl == null || string.IsNullOrEmpty(name)) return;

            // 이미 실행 중인 탭인지 확인하기 위해 태그 이름 검색
            AppForm existingApp = FindRunningApp(name);

            if (existingApp != null) // 만약 실행 중인 탭으로 존재한다면
            {
                // 새로 생성하지 않고 맨 앞으로 가져오기만 하면 끝
                existingApp.BringToFront();
                desktopHost.SendToBack();
                taskbarPanel.BringToFront();

                if (!existingApp.Visible) // 창이 열려 있지만 최소화 상태면
                {
                    existingApp.Show(); // 최소화 상태를 풀어서 보여주기
                }
                return;
            }

            // 앱이 없다면 새로 생성해야 하므로 매개변수에 맞게 생성
            AppForm newApp = CreateAppForm(location, size, name, icon);

            // 폴더 뷰인 경우 앱 실행 요청 구독
            // 이렇게 하지 않으면 public 메서드로 호출하도록 해야 하니 의존성 과함
            if (actionControl is FolderView folderView)
            {
                // 디렉토리 탐색을 도울 컨트롤러 생성
                FolderNavigationController controller = new FolderNavigationController(
                    folderView,
                    newApp,
                    _fileSystemData,
                    folderStartKey
                );

                // 앱 실행 요청을 구독해 신호를 받으면 해당 정보로 앱 실행하도록 설정
                controller.AppLaunchRequested += OnAppLaunchRequested;

                newApp.Tag = controller; // 컨트롤러는 태그에 보관

                controller.Start();
            }

            // AppForm에게 컨트롤을 전달해서 내부 반영하면 끝
            newApp.LoadInnerForm(actionControl, keepAspect);
        }

        /// <summary>
        /// 태그를 검색어로 현재 실행 중인 특정 AppForm을 찾아 반환하는 메서드
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        private AppForm FindRunningApp(string appName)
        {
            // desktopHost의 모든 컨트롤을 foreach로 순회
            foreach (Control control in desktopHost.Controls)
            {
                // 만약 AppForm 타입이고, 폼의 이름이 앱 이름과 같고, 아직 존재하면
                if (control is AppForm appForm &&
                    appForm.Name == appName &&
                    !appForm.IsDisposed)
                {
                    return appForm; // 검색 성공이니 해당 폼 반환
                }
            }
            return null; // 못 찾았으면 검색 실패니 null 반환
        }

        /// <summary>
        /// 컨트롤러의 요청을 받아 파일 아이템이 가진 컨트롤을 앱 폼에서 실행하는 메서드
        /// </summary>
        /// <param name="item"></param>
        private void OnAppLaunchRequested(FileSystemItem item, Image icon)
        {
            if (item == null) return;

            // ActionControl이 있어야 앱 폼에서 실행 가능한 파일이라는 뜻
            if (item.ActionControl != null)
            {
                // 이벤트로 전달 받은 아이콘을 여기서 넘겨줘야 파일 시스템 아이템에서 얻어온 이미지 반영 가능
                LaunchApp(item.ActionControl, item.Name, new Point(150, 150), new Size(520, 360), true, icon);
            }
            else
            {
                MessageBox.Show(item.Name + " 파일을 열 수 없습니다.");
            }
        }

        /// <summary>
        /// 아이콘이 아닌 바탕화면을 클릭했을 때 아이콘 선택을 해제하는 메서드
        /// </summary>
        private void desktopHost_Click(object sender, EventArgs e)
        {
            // 만약 선택된 아이콘이 있었다면
            if (_selectedIcon != null)
            {
                _selectedIcon.BackColor = Color.Transparent;
                _selectedIcon = null; // 선택 해제
            }
        }

        /// <summary>
        /// 아이콘에 마우스가 들어왔을 때 아이콘 배경 색을 약간 바꾸는 메서드
        /// </summary>
        private void Icon_MouseEnter(object sender, EventArgs e)
        {
            var hoverLabel = sender as Label;

            // 선택된 상태가 아닐 때만 변경해야 서로 겹치지 않음
            if (hoverLabel != _selectedIcon)
            {
                hoverLabel.BackColor = SystemColors.ControlLight;
            }
        }

        /// <summary>
        /// 아이콘에서 마우스가 나갔을 때 아이콘 배경 색을 다시 기본값으로 바꾸는 메서드
        /// </summary>
        private void Icon_MouseLeave(object sender, EventArgs e)
        {
            var hoverLabel = sender as Label;

            if (hoverLabel != _selectedIcon)
            {
                hoverLabel.BackColor = Color.Transparent;
            }
        }

        /// <summary>
        /// 아이콘을 한 번 클릭했을 때 아이콘 배경 색을 바꾸는 메서드 
        /// </summary>
        private void Icon_Click(object sender, EventArgs e)
        {
            var clickedLabel = sender as Label;
            if (clickedLabel == null) return;

            // 만약 이전에 다른 아이콘이 선택되어 있었다면
            if (_selectedIcon != null && _selectedIcon != clickedLabel)
            {
                _selectedIcon.BackColor = Color.Transparent; // 이전 아이콘은 기본 색으로 복원
            }

            // 새로 클릭한 아이콘을 선택한 아이콘으로 지정
            _selectedIcon = clickedLabel;
            _selectedIcon.BackColor = SystemColors.ControlLight;
        }

        private void taskbarPanel_Paint(object sender, PaintEventArgs e)
        {
            // 그라데이션을 그릴 Graphics 객체 가져오기
            Graphics g = e.Graphics;

            Rectangle rect = taskbarPanel.ClientRectangle;

            // 창의 상단바처럼 수직 방향 그라데이션 브러시 생성
            // using을 이용해 자동으로 메모리 반환해서 누수 방지
            using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect,
                    taskFlowBarUpColor,
                    taskFlowBarDownColor,
                    LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect); // 패널 배경에 그라데이션 채우기
            }
        }

        /// <summary>
        /// 매초 호출되어 시계 레이블의 텍스트를 갱신하는 메서드
        /// </summary>
        private void clockTimer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        /// <summary>
        /// 현재 시간을 포맷에 맞게 레이블 텍스트에 업데이트하는 메서드
        /// </summary>
        private void UpdateClock()
        {
            DateTime now = DateTime.Now;
            // tt는 오전/오후, h:mm은 12시간 기준 시:분
            string time = now.ToString("tt h:mm");
            // yyyy-MM-dd는 연-월-일
            string date = now.ToString("yyyy-MM-dd");

            // \n 개행 문자를 이용해 2줄로 표시
            clockLabel.Text = $"{time}\n{date}";
        }

        /// <summary>
        /// 작업 표시줄 버튼의 Paint 이벤트가 발생할 때마다 호출되어 버튼의 모양과 색을 그리는 메서드
        /// </summary>
        private void taskButton_Paint(object sender, PaintEventArgs e)
        {
            Button btn = (Button)sender;
            AppForm form = (AppForm)btn.Tag; // 버튼의 태그에 달린 앱 폼 최소화 확인용으로 가져오기

            if (form == null || form.IsDisposed) // 혹시라도 폼 연결이 끊겼을 경우 기본 그리기 형식
            {
                e.Graphics.Clear(btn.BackColor);
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font,
                    btn.ClientRectangle, btn.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = btn.ClientRectangle;
            int cornerRadius = 6; // 모서리 둥근 정도(반지름)

            // 둥근 사각형 경로 생성(둥근 상단바 원리와 비슷)
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            path.AddArc(rect.Right - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            path.AddArc(rect.Right - cornerRadius * 2, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            path.CloseFigure();

            Color color1, color2, borderColor;

            if (form.Visible) // 활성 상태면 좀 더 밝은 색
            {
                color1 = Color.FromArgb(80, 170, 255);
                color2 = Color.FromArgb(40, 120, 240);
                borderColor = Color.FromArgb(0, 80, 200);
            }
            else // 비활성 상태면 좀 더 어두운 색
            {
                color1 = Color.FromArgb(0, 60, 160);
                color2 = Color.FromArgb(0, 88, 200);
                borderColor = Color.FromArgb(0, 40, 130);
            }

            // 브러시로 그라데이션 배경 채운 뒤 메모리 누수 막기 위한 자동 해제
            using (LinearGradientBrush brush = new LinearGradientBrush(
            rect, color1, color2, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // 테두리는 굵기가 얇은 펜으로 그리기
            using (Pen pen = new Pen(borderColor, 1))
            {
                g.DrawPath(pen, path);
            }

            if (form.IconImage != null)
            {
                int iconSize = 16;
                int iconX = rect.X + 4;
                int iconY = rect.Y + (rect.Height - iconSize) / 2;

                g.DrawImage(form.IconImage, iconX, iconY, iconSize, iconSize);

                // 앱 폼 상단바와 마찬가지로 아이콘이 있을 때는 텍스트가 오른쪽으로 밀려나므로 아이콘 왼쪽 시작 위치 + 아이콘 크기 기준 
                Rectangle textRect = new Rectangle(iconX + iconSize + 4, rect.Y,
                                                   rect.Width - (iconX + iconSize + 4), rect.Height);
                TextRenderer.DrawText(g, btn.Text, btn.Font, textRect, btn.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            else // 아이콘 없으면 그대로 왼쪽으로 붙여서 정렬
            {
                TextRenderer.DrawText(g, btn.Text, btn.Font, rect, btn.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        /// <summary>
        /// 바탕화면 아이콘 레이블의 텍스트를 직접 그려 그림자 효과 등을 넣는 이벤트 메서드
        /// </summary>
        private void IconLabel_Paint(object sender, PaintEventArgs e)
        {
            if (!(sender is Label))
                return;

            Label lbl = (Label)sender;
            Graphics g = e.Graphics;

            string text = lbl.Text;

            if (string.IsNullOrEmpty(text)) 
                return;

            // 텍스트 정렬은 아이콘 아래, 중앙 정렬 그대로
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                    TextFormatFlags.Bottom |
                                    TextFormatFlags.WordBreak; // 줄바꿈 허용

            // 1 픽셀 오른쪽 밑에 텍스트 그림자부터 그리기(순서상 더 아래에 위치해야 하므로 먼저 드로우)
            Rectangle shadowRect = new Rectangle(lbl.ClientRectangle.X + 1, lbl.ClientRectangle.Y + 1,
                                                lbl.ClientRectangle.Width, lbl.ClientRectangle.Height);
            TextRenderer.DrawText(g, text, lbl.Font, shadowRect, Color.Black, flags);

            // 그림자를 그린 뒤 원래 자리에 본문 텍스트 그리기
            TextRenderer.DrawText(g, text, lbl.Font, lbl.ClientRectangle, lbl.ForeColor, flags);
        }

    }

    /// <summary>
    ///  디렉토리 탐색을 위해 부모의 키 정보, 담고 있는 파일 등을 속성으로 갖는 폴더 데이터 저장용 클래스 
    /// </summary>
    public class FolderData
    {
        public string Name { get; set; }
        public string ParentKey { get; set; } // 부모의 폴더 키
        public List<FileSystemItem> Items { get; set; } // 폴더가 담고 있는 파일
        public FolderData(string name, string parentKey, List<FileSystemItem> items)
        {
            Name = name;
            ParentKey = parentKey; 
            Items = items;
        }
    }
}