using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
                // 만약 컨트롤이 레이블이면서, 태그가 아이콘이면
                if (control is Label iconLabel && iconLabel.Tag?.ToString() == "Icon")
                {
                    // 필수 아이콘 이벤트를 자동으로 연결
                    iconLabel.Click += Icon_Click;
                    iconLabel.MouseEnter += Icon_MouseEnter;
                    iconLabel.MouseLeave += Icon_MouseLeave;
                }
            }
        }

        private void InitFileSystem()
        {
            _fileSystemData.Clear();

            // 루트에 내 문서 폴더와 Game.exe 파일 생성 루트는 부모 없으므로 null
            _fileSystemData.Add("ROOT", new FolderData("바탕 화면", null, new List<FileSystemItem>
            {
                new FileSystemItem("내 문서", "NAVIGATE_DOCS"),
                new FileSystemItem("Game.exe", new GameForm())
            }));

            // 내 문서 폴더 안에는 새 폴더와 미구현 파일 존재, 내 문서 폴더의 부모는 루트
            _fileSystemData.Add("NAVIGATE_DOCS", new FolderData("내 문서", "ROOT", new List<FileSystemItem>
            {
                new FileSystemItem("새 폴더", "NAVIGATE_NEWFOLDER"),
                new FileSystemItem("보고서.txt", false)
            }));

            // 새 폴더 폴더 안에는 미구현 파일 존재, 새 폴더의 부모는 내 문서 폴더
            _fileSystemData.Add("NAVIGATE_NEWFOLDER", new FolderData("새 폴더", "NAVIGATE_DOCS", new List<FileSystemItem>
            {
                new FileSystemItem("비어있음.txt", false)
            }));
        }

        // 바탕화면 테스트용 앱 아이콘 버튼 3개에 대한 각각의 클릭 이벤트
        private void appIcon1_DoubleClick(object sender, EventArgs e)
        {
            LaunchAppFromIcon(new GameForm(), "App A", new Point(40, 40), new Size(520, 360), true);
        }

        private void appIcon2_DoubleClick(object sender, EventArgs e)
        {
            LaunchAppFromIcon(new GameForm(), "App B", new Point(120, 80), new Size(520, 360), false);
        }

        private void folderIcon1_DoubleClick(object sender, EventArgs e)
        {
            LaunchAppFromIcon(new FolderView(), "내 문서", new Point(100, 60), new Size(450, 350), true, "NAVIGATE_DOCS");
        }

        /// <summary>
        /// 생성한 AppForm을 desktopHost에 넣어서 반환하는 메서드 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private AppForm CreateAppForm(Point location, Size size, string title = "NoNameApp")
        {
            // 새 앱 폼을 만들면서 초기화
            // 최상위 요소면 추가가 불가능하니 TopLevel은 false로
            AppForm f = new AppForm
            {
                TopLevel = false,
                StartPosition = FormStartPosition.Manual,
                Location = location,
                Size = size
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
            // 작업 표시줄 버튼이 검색 없이도 연결된 폼을 알 수 있도록 태그로 설정
            taskButton.Tag = f; 

            // 앱 폼은 작업 표시줄 버튼의 존재를 모르기 때문에 디자이너뷰에서 이벤트 추가 불가
            // 앱 폼에 버튼 변수를 만들어 전달하는 건 서로를 참조하는 구조라서 과한 의존성 발생
            // 따라서 람다 식을 이용한 이벤트 구독 방식을 이용
            // 별도의 메서드가 불필요하고 지역 변수에 직접 접근하는 실행문을 등록 가능

            // 폼이 보이는 여부가 바뀌었을 때 발생하는 이벤트
            f.VisibleChanged += (formSender, formArgs) =>
            {
                //폼이 보이게 되었는지 여부에 따라 작업 표시줄 버튼 색 변경
                if (f.Visible)
                    taskButton.BackColor = SystemColors.Control;
                else
                    taskButton.BackColor = SystemColors.WindowFrame;
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
        private void LaunchAppFromIcon(Control actionControl, string name, Point location, Size size, bool keepAspect, string folderStartKey = "ROOT")
        {
            LaunchApp(actionControl, name, location, size, keepAspect, folderStartKey);

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
        private void LaunchApp(Control actionControl, string name, Point location, Size size, bool keepAspect, string folderStartKey = "ROOT")
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
            AppForm newApp = CreateAppForm(location, size, name);

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
        private void OnAppLaunchRequested(FileSystemItem item)
        {
            if (item == null) return;

            // ActionControl이 있어야 앱 폼에서 실행 가능한 파일이라는 뜻
            if (item.ActionControl != null)
            {
                LaunchApp(item.ActionControl, item.Name, new Point(150, 150), new Size(520, 360), true);
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