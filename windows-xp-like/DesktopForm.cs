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
        public DesktopForm()
        {
            InitializeComponent();
        }
        private void DesktopForm_Load(object sender, EventArgs e)
        {
            // 보이는 순서가 작업 표시줄이 제일 위로, 바탕화면이 제일 뒤로 가도록 설정
            taskbarPanel.BringToFront();
            desktopHost.SendToBack();
        }

        // 바탕화면 테스트용 앱 아이콘 버튼 3개에 대한 각각의 클릭 이벤트
        private void appIcon1_Click(object sender, EventArgs e)
        {
            LaunchApp(new GameForm(), "App A", new Point(40, 40), new Size(520, 360), true);
        }

        private void appIcon2_Click(object sender, EventArgs e)
        {
            LaunchApp(new GameForm(), "App B", new Point(120, 80), new Size(520, 360), false);
        }

        private void folderIcon_Click(object sender, EventArgs e)
        {
            LaunchApp(new FolderView(), "내 문서", new Point(100, 60), new Size(450, 350), true);
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

            f.Text = title; // 텍스트는 곧 앱 이름, 없으면 매개변수 기본값
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
        /// 새로 생성된 컨트롤을 받아서 AppForm과 연결해 창으로 보여주는 메서드 
        /// </summary>
        /// <param name="actionControl"></param>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="keepAspect">창 크기 조절할 때 특정 비율을 지켜야 하면 true</param>
        private void LaunchApp(Control actionControl, string name, Point location, Size size, bool keepAspect)
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
                return;
            }

            // 앱이 없다면 새로 생성해야 하므로 매개변수에 맞게 생성
            AppForm newApp = CreateAppForm(location, size, name);

            // 폴더 뷰인 경우 더블 클릭 이벤트 등록
            // 이렇게 하지 않으면 public 메서드로 호출하도록 해야 하니 의존성 과함
            if (actionControl is FolderView folder)
            {
                folder.ItemDoubleClicked += FolderView_ItemDoubleClicked;
            }

            // AppForm에게 컨트롤을 전달해서 내부 반영하면 끝
            newApp.LoadInnerForm(actionControl, keepAspect);
        }


        /// <summary>
        /// 폴더 뷰의 파일 시스템에서 지정한 아이템에 알맞은 앱이 열리도록 하는 메서드
        /// </summary>
        /// <param name="item"></param>
        private void FolderView_ItemDoubleClicked(FileSystemItem item)
        {
            if (item == null) return;

            if (item.ActionControl == null)
            {
                MessageBox.Show("미구현된 파일 또는 폴더입니다.");
                return;
            }

            LaunchApp(item.ActionControl, item.Name,
                new Point(150, 150), new Size(520, 360), true);
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
                // 만약 AppForm 타입이고, 태그 형식이 올바르고,
                // 찾고 있는 앱 폼과 일치하고, 그 앱 폼이 존재하면
                if (control is AppForm appForm &&
                    appForm.Tag is string tag &&
                    tag == appName &&
                    !appForm.IsDisposed)
                {
                    return appForm; // 검색 성공이니 해당 폼 반환
                }
            }
            return null; // 못 찾았으면 검색 실패니 null 반환
        }
    }
}