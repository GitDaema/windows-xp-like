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
            this.desktopHost = new System.Windows.Forms.Panel();
            this.appIcon2 = new System.Windows.Forms.Button();
            this.appIcon1 = new System.Windows.Forms.Button();
            this.taskbarPanel = new System.Windows.Forms.Panel();
            this.taskFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.desktopHost.SuspendLayout();
            this.taskbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // desktopHost
            // 
            this.desktopHost.Controls.Add(this.taskbarPanel);
            this.desktopHost.Controls.Add(this.appIcon2);
            this.desktopHost.Controls.Add(this.appIcon1);
            this.desktopHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.desktopHost.Location = new System.Drawing.Point(0, 0);
            this.desktopHost.Name = "desktopHost";
            this.desktopHost.Size = new System.Drawing.Size(1264, 681);
            this.desktopHost.TabIndex = 0;
            // 
            // appIcon2
            // 
            this.appIcon2.Location = new System.Drawing.Point(3, 32);
            this.appIcon2.Name = "appIcon2";
            this.appIcon2.Size = new System.Drawing.Size(75, 23);
            this.appIcon2.TabIndex = 1;
            this.appIcon2.Text = "button2";
            this.appIcon2.UseVisualStyleBackColor = true;
            this.appIcon2.Click += new System.EventHandler(this.appIcon2_Click);
            // 
            // appIcon1
            // 
            this.appIcon1.Location = new System.Drawing.Point(3, 3);
            this.appIcon1.Name = "appIcon1";
            this.appIcon1.Size = new System.Drawing.Size(75, 23);
            this.appIcon1.TabIndex = 0;
            this.appIcon1.Text = "button1";
            this.appIcon1.UseVisualStyleBackColor = true;
            this.appIcon1.Click += new System.EventHandler(this.appIcon1_Click);
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
        private System.Windows.Forms.Button appIcon1;
        private System.Windows.Forms.Button appIcon2;
        private System.Windows.Forms.Panel taskbarPanel;
        private System.Windows.Forms.FlowLayoutPanel taskFlowPanel;
    }
}

