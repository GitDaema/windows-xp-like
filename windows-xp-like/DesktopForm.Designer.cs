namespace windows_xp_like
{
    partial class DesktopForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DesktopForm));
            this.desktopHost = new System.Windows.Forms.Panel();
            this.taskbarPanel = new System.Windows.Forms.Panel();
            this.taskFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.appIcon1 = new System.Windows.Forms.Label();
            this.appIcon2 = new System.Windows.Forms.Label();
            this.folderIcon1 = new System.Windows.Forms.Label();
            this.desktopHost.SuspendLayout();
            this.taskbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // desktopHost
            // 
            this.desktopHost.Controls.Add(this.folderIcon1);
            this.desktopHost.Controls.Add(this.appIcon2);
            this.desktopHost.Controls.Add(this.appIcon1);
            this.desktopHost.Controls.Add(this.taskbarPanel);
            this.desktopHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.desktopHost.Location = new System.Drawing.Point(0, 0);
            this.desktopHost.Name = "desktopHost";
            this.desktopHost.Size = new System.Drawing.Size(1264, 681);
            this.desktopHost.TabIndex = 0;
            this.desktopHost.Click += new System.EventHandler(this.desktopHost_Click);
            // 
            // taskbarPanel
            // 
            this.taskbarPanel.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.taskbarPanel.Controls.Add(this.taskFlowPanel);
            this.taskbarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.taskbarPanel.Location = new System.Drawing.Point(0, 651);
            this.taskbarPanel.Name = "taskbarPanel";
            this.taskbarPanel.Size = new System.Drawing.Size(1264, 30);
            this.taskbarPanel.TabIndex = 1;
            // 
            // taskFlowPanel
            // 
            this.taskFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.taskFlowPanel.Location = new System.Drawing.Point(0, 0);
            this.taskFlowPanel.Name = "taskFlowPanel";
            this.taskFlowPanel.Size = new System.Drawing.Size(1264, 30);
            this.taskFlowPanel.TabIndex = 0;
            // 
            // appIcon1
            // 
            this.appIcon1.BackColor = System.Drawing.Color.Transparent;
            this.appIcon1.Image = ((System.Drawing.Image)(resources.GetObject("appIcon1.Image")));
            this.appIcon1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.appIcon1.Location = new System.Drawing.Point(12, 9);
            this.appIcon1.Name = "appIcon1";
            this.appIcon1.Size = new System.Drawing.Size(50, 50);
            this.appIcon1.TabIndex = 2;
            this.appIcon1.Tag = "Icon";
            this.appIcon1.Text = "appA";
            this.appIcon1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.appIcon1.DoubleClick += new System.EventHandler(this.appIcon1_DoubleClick);
            // 
            // appIcon2
            // 
            this.appIcon2.BackColor = System.Drawing.Color.Transparent;
            this.appIcon2.Image = ((System.Drawing.Image)(resources.GetObject("appIcon2.Image")));
            this.appIcon2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.appIcon2.Location = new System.Drawing.Point(12, 69);
            this.appIcon2.Name = "appIcon2";
            this.appIcon2.Size = new System.Drawing.Size(50, 50);
            this.appIcon2.TabIndex = 3;
            this.appIcon2.Tag = "Icon";
            this.appIcon2.Text = "appB";
            this.appIcon2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.appIcon2.DoubleClick += new System.EventHandler(this.appIcon2_DoubleClick);
            // 
            // folderIcon1
            // 
            this.folderIcon1.BackColor = System.Drawing.Color.Transparent;
            this.folderIcon1.Image = ((System.Drawing.Image)(resources.GetObject("folderIcon1.Image")));
            this.folderIcon1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.folderIcon1.Location = new System.Drawing.Point(12, 130);
            this.folderIcon1.Name = "folderIcon1";
            this.folderIcon1.Size = new System.Drawing.Size(50, 50);
            this.folderIcon1.TabIndex = 4;
            this.folderIcon1.Tag = "Icon";
            this.folderIcon1.Text = "내 문서";
            this.folderIcon1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.folderIcon1.DoubleClick += new System.EventHandler(this.folderIcon1_DoubleClick);
            // 
            // DesktopForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.desktopHost);
            this.Name = "DesktopForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.DesktopForm_Load);
            this.desktopHost.ResumeLayout(false);
            this.taskbarPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel desktopHost;
        private System.Windows.Forms.Panel taskbarPanel;
        private System.Windows.Forms.FlowLayoutPanel taskFlowPanel;
        private System.Windows.Forms.Label appIcon1;
        private System.Windows.Forms.Label folderIcon1;
        private System.Windows.Forms.Label appIcon2;
    }
}

