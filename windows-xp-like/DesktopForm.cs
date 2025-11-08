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

            // [NEW] 1. 작업 표시줄 버튼 생성
            Button taskButton = new Button();
            taskButton.Text = f.Text;
            taskButton.AutoSize = true; // 또는 고정 크기
            taskButton.Tag = f; // [중요] 버튼에 폼 인스턴스 연결

            // [NEW] 2. 작업 표시줄 버튼 클릭 이벤트 연결
            taskButton.Click += (sender, args) =>
            {
                Form appForm = (sender as Button).Tag as Form;
                if (appForm != null && !appForm.IsDisposed)
                {
                    // TODO: 최소화 상태라면 복원 (Show)

                    // 일단은 맨 앞으로 가져오기
                    appForm.BringToFront();

                    // [수정] 작업 표시줄 버튼 클릭 시에도 Z-Order 보정
                    taskbarPanel.BringToFront();
                    desktopHost.SendToBack();
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
            if (_appA != null && !_appA.IsDisposed)
            {
                _appA.BringToFront();
                // [수정] 이미 열린 폼을 클릭할 때도 Z-Order 보정
                desktopHost.SendToBack();
                taskbarPanel.BringToFront();
                return;
            }

            _appA = CreateAppForm(new Point(40, 40), new Size(520, 360), "App A");
            _appA.LoadInnerForm(new GameForm());
            _appA.Disposed += (_, __) => _appA = null; // 닫히면 참조 정리
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
    }
}