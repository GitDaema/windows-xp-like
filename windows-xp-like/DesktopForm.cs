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
        private AppForm _appA;
        private AppForm _appB;

        private AppForm _appC;
        private AppForm _appGame;

        public DesktopForm()
        {
            InitializeComponent();
        }
        // 공용 생성 함수: AppForm을 desktopHost에 넣고 Show
        private AppForm CreateAppForm(Point location, Size size, string title = null)
        {
            var f = new AppForm
            {
                TopLevel = false,
                StartPosition = FormStartPosition.Manual,
                Location = location,
                Size = size
            };
            f.Text = title ?? "App";
            f.TaskbarPanel = taskbarPanel;

            // [NEW] 1. 작업 표시줄 버튼 생성
            Button taskButton = new Button();
            taskButton.Text = f.Text;
            taskButton.AutoSize = true; // 또는 고정 크기
            taskButton.Tag = f; // [중요] 버튼에 폼 인스턴스 연결

            f.VisibleChanged += (formSender, formArgs) =>
            {
                if (f.Visible)
                {
                    // 폼이 보이게 됨 (복원됨)
                    taskButton.BackColor = SystemColors.Control; // 원래 버튼 색상
                }
                else
                {
                    // 폼이 숨겨짐 (최소화됨)
                    taskButton.BackColor = SystemColors.WindowFrame; // 최소화 상태를 나타내는 다른 색상
                }
            };

            // [NEW] 2. 작업 표시줄 버튼 클릭 이벤트 연결
            taskButton.Click += (sender, args) =>
            {
                Form appForm = (sender as Button).Tag as Form;
                if (appForm == null || appForm.IsDisposed)
                {
                    return; // 폼이 없거나 파괴되었으면 아무것도 안 함
                }

                // [수정] 1. 최소화 상태(숨겨진)라면 복원
                if (!appForm.Visible)
                {
                    appForm.Show();
                    appForm.BringToFront();

                    // Z-Order 보정
                    taskbarPanel.BringToFront();
                    desktopHost.SendToBack();
                }
                // [수정] 2. 이미 보이는 상태일 때
                else
                {
                    appForm.Hide();
                }
            };

            // [NEW] 3. FlowLayoutPanel에 버튼 추가
            taskFlowPanel.Controls.Add(taskButton);

            // [NEW] 4. AppForm이 닫힐 때(Disposed) 작업 표시줄 버튼 제거
            f.Disposed += (sender, args) =>
            {
                taskFlowPanel.Controls.Remove(taskButton);
                taskButton.Dispose();
            };

            desktopHost.Controls.Add(f);
            f.BringToFront();
            f.Show();

            // [핵심 수정]
            // AppForm.cs의 'Any_MouseDown_BringToFront'에 있는 Z-Order 보정 로직을
            // AppForm을 '처음 생성'할 때도 동일하게 실행해야 합니다.
            // (AppForm.Show()가 호출될 때 desktopHost가 taskbarPanel 위로 올라오는 현상 수정)
            desktopHost.SendToBack();
            taskbarPanel.BringToFront();

            return f;
        }

        private void appIcon1_Click(object sender, EventArgs e)
        {
            // [수정] 이 메서드는 이제 새 메서드를 호출만 합니다.
            LaunchAppA();
        }

        // 2. [신규] 'appA'를 실행하는 전용 메서드를 만듭니다.
        // (appIcon1_Click에서 잘라낸 코드를 여기에 '붙여넣기' 합니다)
        private void LaunchAppA()
        {
            if (_appA != null && !_appA.IsDisposed)
            {
                _appA.BringToFront();
                desktopHost.SendToBack();
                taskbarPanel.BringToFront();
                return;
            }

            _appA = CreateAppForm(new Point(40, 40), new Size(520, 360), "App A");
            _appA.LoadInnerForm(new GameForm(), true); // GameForm은 true로 하신다고 했죠
            _appA.Disposed += (_, __) => _appA = null;
        }


        private void appIcon2_Click(object sender, EventArgs e)
        {
            if (_appB != null && !_appB.IsDisposed)
            {
                _appB.BringToFront();
                // [수정] 이미 열린 폼을 클릭할 때도 Z-Order 보정
                desktopHost.SendToBack();
                taskbarPanel.BringToFront();
                return;
            }

            _appB = CreateAppForm(new Point(120, 80), new Size(520, 360), "App B");
            _appB.LoadInnerForm(new GameForm(), false);
            _appB.Disposed += (_, __) => _appB = null; // 닫히면 참조 정리
        }

        private void DesktopForm_Load(object sender, EventArgs e)
        {
            taskbarPanel.BringToFront();
            desktopHost.SendToBack();
        }

        private void folderIcon_Click(object sender, EventArgs e)
        {
            // 이미 열려있으면 맨 앞으로 가져오기
            if (_appC != null && !_appC.IsDisposed)
            {
                _appC.BringToFront();
                desktopHost.SendToBack();
                taskbarPanel.BringToFront();
                return;
            }

            // 1. 새 AppForm 생성
            _appC = CreateAppForm(new Point(100, 60), new Size(450, 350), "내 문서");

            // 2. FolderView '부품' 생성
            var folder = new FolderView();

            // 3. [핵심] FolderView가 "ItemDoubleClicked" 신호를 보내면,
            //    DesktopForm의 "FolderView_ItemDoubleClicked" 메서드가 받아서 처리!
            folder.ItemDoubleClicked += FolderView_ItemDoubleClicked;

            // 4. AppForm에 GameForm 대신 FolderView를 삽입 (비율 고정 false)
            _appC.LoadInnerForm(folder, true);
            _appC.Disposed += (_, __) => _appC = null; // 닫히면 참조 정리
        }

        // 2. FolderView가 보낸 '신호'를 처리할 실제 메서드 (새로 추가)
        private void FolderView_ItemDoubleClicked(FileSystemItem item)
        {
            if (item == null) return;

            // [핵심 수정] item.Name 대신 item.ActionKey를 검사합니다.
            switch (item.ActionKey)
            {
                case "LAUNCH_APP_A":
                    // "appA를 실행하라"는 꼬리표가 붙은 아이템이므로 LaunchAppA() 호출
                    LaunchAppA();
                    break;

                case "OPEN_FOLDER":
                    // [나중 확장] "폴더를 열라"는 꼬리표
                    MessageBox.Show(item.Name + " 폴더를 엽니다. (아직 구현되지 않음)");
                    break;

                case "OPEN_FILE":
                default:
                    // "일반 파일을 열라" 또는 그 외의 모든 경우
                    MessageBox.Show(item.Name + " 파일을 열 수 없습니다.");
                    break;
            }
        }
    }
}