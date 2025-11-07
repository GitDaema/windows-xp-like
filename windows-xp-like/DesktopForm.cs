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

            desktopHost.Controls.Add(f);
            f.BringToFront();
            f.Show();
            return f;
        }

        private void appIcon1_Click(object sender, EventArgs e)
        {
            if (_appA != null && !_appA.IsDisposed)
            {
                _appA.BringToFront();
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
                return;
            }

            _appB = CreateAppForm(new Point(120, 80), new Size(560, 380), "App B");
            // _appB.LoadInnerForm(new GameForm());
            _appB.Disposed += (_, __) => _appB = null; // 닫히면 참조 정리
        }
    }
}
