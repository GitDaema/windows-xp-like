namespace windows_xp_like
{
    partial class AppForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppForm));
            this.minimizeButton = new System.Windows.Forms.Button();
            this.maximizeButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.win_bt_imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // minimizeButton
            // 
            this.minimizeButton.ImageIndex = 0;
            this.minimizeButton.ImageList = this.win_bt_imageList;
            this.minimizeButton.Location = new System.Drawing.Point(363, 214);
            this.minimizeButton.Name = "minimizeButton";
            this.minimizeButton.Size = new System.Drawing.Size(26, 26);
            this.minimizeButton.TabIndex = 2;
            this.minimizeButton.UseVisualStyleBackColor = true;
            // 
            // maximizeButton
            // 
            this.maximizeButton.ImageIndex = 1;
            this.maximizeButton.ImageList = this.win_bt_imageList;
            this.maximizeButton.Location = new System.Drawing.Point(547, 167);
            this.maximizeButton.Name = "maximizeButton";
            this.maximizeButton.Size = new System.Drawing.Size(26, 26);
            this.maximizeButton.TabIndex = 3;
            this.maximizeButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.BackColor = System.Drawing.Color.Transparent;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.closeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.closeButton.ImageKey = "win_close_button.png";
            this.closeButton.ImageList = this.win_bt_imageList;
            this.closeButton.Location = new System.Drawing.Point(609, 41);
            this.closeButton.Name = "closeButton";
            this.closeButton.Padding = new System.Windows.Forms.Padding(0, 0, 1, 1);
            this.closeButton.Size = new System.Drawing.Size(26, 26);
            this.closeButton.TabIndex = 1;
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // win_bt_imageList
            // 
            this.win_bt_imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("win_bt_imageList.ImageStream")));
            this.win_bt_imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.win_bt_imageList.Images.SetKeyName(0, "win_minimize_button.png");
            this.win_bt_imageList.Images.SetKeyName(1, "win_maximize_button.png");
            this.win_bt_imageList.Images.SetKeyName(2, "win_close_button.png");
            // 
            // AppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.maximizeButton);
            this.Controls.Add(this.minimizeButton);
            this.Controls.Add(this.closeButton);
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "AppForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AppForm";
            this.Load += new System.EventHandler(this.AppForm_Load);
            this.SizeChanged += new System.EventHandler(this.AppForm_SizeChanged);
            this.Move += new System.EventHandler(this.AppForm_Move);
            this.ParentChanged += new System.EventHandler(this.AppForm_ParentChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button minimizeButton;
        private System.Windows.Forms.Button maximizeButton;
        private System.Windows.Forms.ImageList win_bt_imageList;
    }
}