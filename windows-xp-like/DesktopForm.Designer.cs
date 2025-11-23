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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DesktopForm));
            this.desktopHost = new System.Windows.Forms.Panel();
            this.startMenuPanel = new System.Windows.Forms.Panel();
            this.offButton = new System.Windows.Forms.Button();
            this.off_bt_imageList = new System.Windows.Forms.ImageList(this.components);
            this.folderIcon1 = new System.Windows.Forms.Label();
            this.appIcon2 = new System.Windows.Forms.Label();
            this.appIcon1 = new System.Windows.Forms.Label();
            this.taskbarPanel = new System.Windows.Forms.Panel();
            this.taskFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.clockLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.start_bt_imageList = new System.Windows.Forms.ImageList(this.components);
            this.clockTimer = new System.Windows.Forms.Timer(this.components);
            this.desktopHost.SuspendLayout();
            this.startMenuPanel.SuspendLayout();
            this.taskbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // desktopHost
            // 
            this.desktopHost.BackColor = System.Drawing.Color.Transparent;
            this.desktopHost.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.desktopHost.Controls.Add(this.startMenuPanel);
            this.desktopHost.Controls.Add(this.folderIcon1);
            this.desktopHost.Controls.Add(this.appIcon2);
            this.desktopHost.Controls.Add(this.appIcon1);
            this.desktopHost.Controls.Add(this.taskbarPanel);
            this.desktopHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.desktopHost.Location = new System.Drawing.Point(0, 0);
            this.desktopHost.Name = "desktopHost";
            this.desktopHost.Size = new System.Drawing.Size(1264, 681);
            this.desktopHost.TabIndex = 0;
            // 
            // startMenuPanel
            // 
            this.startMenuPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startMenuPanel.BackColor = System.Drawing.Color.Transparent;
            this.startMenuPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("startMenuPanel.BackgroundImage")));
            this.startMenuPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.startMenuPanel.Controls.Add(this.offButton);
            this.startMenuPanel.Location = new System.Drawing.Point(0, 254);
            this.startMenuPanel.Name = "startMenuPanel";
            this.startMenuPanel.Size = new System.Drawing.Size(319, 394);
            this.startMenuPanel.TabIndex = 5;
            // 
            // offButton
            // 
            this.offButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("offButton.BackgroundImage")));
            this.offButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.offButton.FlatAppearance.BorderSize = 0;
            this.offButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.offButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.offButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.offButton.ImageIndex = 0;
            this.offButton.ImageList = this.off_bt_imageList;
            this.offButton.Location = new System.Drawing.Point(216, 362);
            this.offButton.Name = "offButton";
            this.offButton.Size = new System.Drawing.Size(98, 31);
            this.offButton.TabIndex = 6;
            this.offButton.UseVisualStyleBackColor = true;
            this.offButton.Click += new System.EventHandler(this.offButton_Click);
            this.offButton.MouseEnter += new System.EventHandler(this.offButton_MouseEnter);
            this.offButton.MouseLeave += new System.EventHandler(this.offButton_MouseLeave);
            // 
            // off_bt_imageList
            // 
            this.off_bt_imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("off_bt_imageList.ImageStream")));
            this.off_bt_imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.off_bt_imageList.Images.SetKeyName(0, "pc_off_bt.png");
            this.off_bt_imageList.Images.SetKeyName(1, "pc_off_bt_hover.png");
            // 
            // folderIcon1
            // 
            this.folderIcon1.BackColor = System.Drawing.Color.Transparent;
            this.folderIcon1.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.folderIcon1.ForeColor = System.Drawing.SystemColors.Control;
            this.folderIcon1.Image = global::windows_xp_like.Properties.Resources.folder_icon;
            this.folderIcon1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.folderIcon1.Location = new System.Drawing.Point(10, 208);
            this.folderIcon1.Name = "folderIcon1";
            this.folderIcon1.Size = new System.Drawing.Size(85, 85);
            this.folderIcon1.TabIndex = 4;
            this.folderIcon1.Tag = "Icon";
            this.folderIcon1.Text = "내 문서";
            this.folderIcon1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.folderIcon1.DoubleClick += new System.EventHandler(this.folderIcon1_DoubleClick);
            // 
            // appIcon2
            // 
            this.appIcon2.BackColor = System.Drawing.Color.Transparent;
            this.appIcon2.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.appIcon2.ForeColor = System.Drawing.SystemColors.Control;
            this.appIcon2.Image = ((System.Drawing.Image)(resources.GetObject("appIcon2.Image")));
            this.appIcon2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.appIcon2.Location = new System.Drawing.Point(10, 106);
            this.appIcon2.Name = "appIcon2";
            this.appIcon2.Size = new System.Drawing.Size(85, 85);
            this.appIcon2.TabIndex = 3;
            this.appIcon2.Tag = "Icon";
            this.appIcon2.Text = "벽돌깨기";
            this.appIcon2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.appIcon2.DoubleClick += new System.EventHandler(this.appIcon2_DoubleClick);
            // 
            // appIcon1
            // 
            this.appIcon1.BackColor = System.Drawing.Color.Transparent;
            this.appIcon1.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.appIcon1.ForeColor = System.Drawing.SystemColors.Control;
            this.appIcon1.Image = ((System.Drawing.Image)(resources.GetObject("appIcon1.Image")));
            this.appIcon1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.appIcon1.Location = new System.Drawing.Point(10, 9);
            this.appIcon1.Name = "appIcon1";
            this.appIcon1.Size = new System.Drawing.Size(85, 85);
            this.appIcon1.TabIndex = 2;
            this.appIcon1.Tag = "Icon";
            this.appIcon1.Text = "지뢰찾기";
            this.appIcon1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.appIcon1.DoubleClick += new System.EventHandler(this.appIcon1_DoubleClick);
            // 
            // taskbarPanel
            // 
            this.taskbarPanel.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.taskbarPanel.Controls.Add(this.taskFlowPanel);
            this.taskbarPanel.Controls.Add(this.clockLabel);
            this.taskbarPanel.Controls.Add(this.startButton);
            this.taskbarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.taskbarPanel.Location = new System.Drawing.Point(0, 648);
            this.taskbarPanel.Name = "taskbarPanel";
            this.taskbarPanel.Size = new System.Drawing.Size(1264, 33);
            this.taskbarPanel.TabIndex = 1;
            this.taskbarPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.taskbarPanel_Paint);
            // 
            // taskFlowPanel
            // 
            this.taskFlowPanel.BackColor = System.Drawing.Color.Transparent;
            this.taskFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.taskFlowPanel.Location = new System.Drawing.Point(125, 0);
            this.taskFlowPanel.Name = "taskFlowPanel";
            this.taskFlowPanel.Size = new System.Drawing.Size(1019, 33);
            this.taskFlowPanel.TabIndex = 0;
            // 
            // clockLabel
            // 
            this.clockLabel.BackColor = System.Drawing.Color.Transparent;
            this.clockLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.clockLabel.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.clockLabel.ForeColor = System.Drawing.Color.White;
            this.clockLabel.Location = new System.Drawing.Point(1144, 0);
            this.clockLabel.Name = "clockLabel";
            this.clockLabel.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.clockLabel.Size = new System.Drawing.Size(120, 33);
            this.clockLabel.TabIndex = 1;
            this.clockLabel.Text = "오후 12:00 2025-11-11";
            this.clockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.clockLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.clockLabel_Paint);
            // 
            // startButton
            // 
            this.startButton.BackColor = System.Drawing.Color.Transparent;
            this.startButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.startButton.FlatAppearance.BorderSize = 0;
            this.startButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.startButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startButton.Font = new System.Drawing.Font("휴먼둥근헤드라인", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.startButton.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.startButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.startButton.ImageIndex = 0;
            this.startButton.ImageList = this.start_bt_imageList;
            this.startButton.Location = new System.Drawing.Point(0, 0);
            this.startButton.Margin = new System.Windows.Forms.Padding(0);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(125, 33);
            this.startButton.TabIndex = 2;
            this.startButton.UseVisualStyleBackColor = false;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // start_bt_imageList
            // 
            this.start_bt_imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("start_bt_imageList.ImageStream")));
            this.start_bt_imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.start_bt_imageList.Images.SetKeyName(0, "start_bt_kr_idle.png");
            this.start_bt_imageList.Images.SetKeyName(1, "start_bt_kr_hover.png");
            this.start_bt_imageList.Images.SetKeyName(2, "start_bt_kr_press.png");
            this.start_bt_imageList.Images.SetKeyName(3, "start_bt_idle.png");
            this.start_bt_imageList.Images.SetKeyName(4, "start_bt_hover.png");
            this.start_bt_imageList.Images.SetKeyName(5, "start_bt_press.png");
            // 
            // DesktopForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1264, 681);
            this.Controls.Add(this.desktopHost);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1280, 720);
            this.Name = "DesktopForm";
            this.Text = "Window XP";
            this.Load += new System.EventHandler(this.DesktopForm_Load);
            this.desktopHost.ResumeLayout(false);
            this.startMenuPanel.ResumeLayout(false);
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
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label clockLabel;
        private System.Windows.Forms.Timer clockTimer;
        private System.Windows.Forms.ImageList start_bt_imageList;
        private System.Windows.Forms.Panel startMenuPanel;
        private System.Windows.Forms.Button offButton;
        private System.Windows.Forms.ImageList off_bt_imageList;
    }
}

