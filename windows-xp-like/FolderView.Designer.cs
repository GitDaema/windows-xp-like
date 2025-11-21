namespace windows_xp_like
{
    partial class FolderView
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

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderView));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.새로만들기ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.폴더ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.삭제ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeImageList = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.파일FToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.새로만들기NToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.폴더FToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.보기VToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.큰아이콘LToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.자세히DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.간단히SToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tstSearch = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.smallImageList = new System.Windows.Forms.ImageList(this.components);
            this.tsbBack = new System.Windows.Forms.ToolStripButton();
            this.tsbUp = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.새로만들기ToolStripMenuItem,
            this.삭제ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(139, 48);
            // 
            // 새로만들기ToolStripMenuItem
            // 
            this.새로만들기ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.폴더ToolStripMenuItem});
            this.새로만들기ToolStripMenuItem.Name = "새로만들기ToolStripMenuItem";
            this.새로만들기ToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.새로만들기ToolStripMenuItem.Text = "새로 만들기";
            // 
            // 폴더ToolStripMenuItem
            // 
            this.폴더ToolStripMenuItem.Name = "폴더ToolStripMenuItem";
            this.폴더ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.폴더ToolStripMenuItem.Text = "폴더";
            this.폴더ToolStripMenuItem.Click += new System.EventHandler(this.폴더ToolStripMenuItem_Click);
            // 
            // 삭제ToolStripMenuItem
            // 
            this.삭제ToolStripMenuItem.Name = "삭제ToolStripMenuItem";
            this.삭제ToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.삭제ToolStripMenuItem.Text = "삭제";
            this.삭제ToolStripMenuItem.Click += new System.EventHandler(this.삭제ToolStripMenuItem_Click);
            // 
            // largeImageList
            // 
            this.largeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("largeImageList.ImageStream")));
            this.largeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.largeImageList.Images.SetKeyName(0, "folder_icon.png");
            this.largeImageList.Images.SetKeyName(1, "text_file_icon.png");
            this.largeImageList.Images.SetKeyName(2, "snakegame_icon.png");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.파일FToolStripMenuItem,
            this.보기VToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(800, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 파일FToolStripMenuItem
            // 
            this.파일FToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.새로만들기NToolStripMenuItem});
            this.파일FToolStripMenuItem.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.파일FToolStripMenuItem.Name = "파일FToolStripMenuItem";
            this.파일FToolStripMenuItem.Size = new System.Drawing.Size(60, 21);
            this.파일FToolStripMenuItem.Text = "파일(&F)";
            // 
            // 새로만들기NToolStripMenuItem
            // 
            this.새로만들기NToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.폴더FToolStripMenuItem});
            this.새로만들기NToolStripMenuItem.Name = "새로만들기NToolStripMenuItem";
            this.새로만들기NToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.새로만들기NToolStripMenuItem.Text = "새로 만들기(&N)";
            // 
            // 폴더FToolStripMenuItem
            // 
            this.폴더FToolStripMenuItem.Name = "폴더FToolStripMenuItem";
            this.폴더FToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.폴더FToolStripMenuItem.Text = "폴더(&F)";
            this.폴더FToolStripMenuItem.Click += new System.EventHandler(this.폴더ToolStripMenuItem_Click);
            // 
            // 보기VToolStripMenuItem
            // 
            this.보기VToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.큰아이콘LToolStripMenuItem,
            this.자세히DToolStripMenuItem,
            this.간단히SToolStripMenuItem});
            this.보기VToolStripMenuItem.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.보기VToolStripMenuItem.Name = "보기VToolStripMenuItem";
            this.보기VToolStripMenuItem.Size = new System.Drawing.Size(62, 21);
            this.보기VToolStripMenuItem.Text = "보기(&V)";
            // 
            // 큰아이콘LToolStripMenuItem
            // 
            this.큰아이콘LToolStripMenuItem.Name = "큰아이콘LToolStripMenuItem";
            this.큰아이콘LToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.큰아이콘LToolStripMenuItem.Text = "큰 아이콘(&L)";
            this.큰아이콘LToolStripMenuItem.Click += new System.EventHandler(this.큰아이콘LToolStripMenuItem_Click);
            // 
            // 자세히DToolStripMenuItem
            // 
            this.자세히DToolStripMenuItem.Name = "자세히DToolStripMenuItem";
            this.자세히DToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.자세히DToolStripMenuItem.Text = "자세히(&D)";
            this.자세히DToolStripMenuItem.Click += new System.EventHandler(this.자세히DToolStripMenuItem_Click);
            // 
            // 간단히SToolStripMenuItem
            // 
            this.간단히SToolStripMenuItem.Name = "간단히SToolStripMenuItem";
            this.간단히SToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.간단히SToolStripMenuItem.Text = "간단히(&S)";
            this.간단히SToolStripMenuItem.Click += new System.EventHandler(this.간단히SToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbBack,
            this.tsbUp,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.tstSearch,
            this.toolStripSeparator2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 25);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(800, 40);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.AutoSize = false;
            this.toolStripLabel1.BackColor = System.Drawing.Color.Transparent;
            this.toolStripLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripLabel1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(37, 35);
            this.toolStripLabel1.Text = "검색";
            // 
            // tstSearch
            // 
            this.tstSearch.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tstSearch.Name = "tstSearch";
            this.tstSearch.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.tstSearch.Size = new System.Drawing.Size(130, 40);
            this.tstSearch.TextChanged += new System.EventHandler(this.tstSearch_TextChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 40);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.LabelEdit = true;
            this.listView1.LargeImageList = this.largeImageList;
            this.listView1.Location = new System.Drawing.Point(0, 65);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(800, 535);
            this.listView1.SmallImageList = this.smallImageList;
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.DoubleClick += new System.EventHandler(this.ListView1_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "이름";
            this.columnHeader1.Width = 250;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "유형";
            this.columnHeader2.Width = 120;
            // 
            // smallImageList
            // 
            this.smallImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallImageList.ImageStream")));
            this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.smallImageList.Images.SetKeyName(0, "folder_icon.png");
            this.smallImageList.Images.SetKeyName(1, "text_file_icon.png");
            this.smallImageList.Images.SetKeyName(2, "snakegame_icon.png");
            // 
            // tsbBack
            // 
            this.tsbBack.Enabled = false;
            this.tsbBack.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tsbBack.Image = global::windows_xp_like.Properties.Resources.tb_back_icon;
            this.tsbBack.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.tsbBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBack.Name = "tsbBack";
            this.tsbBack.Size = new System.Drawing.Size(38, 37);
            this.tsbBack.Text = "뒤로";
            this.tsbBack.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbBack.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.tsbBack.Click += new System.EventHandler(this.tsbBack_Click);
            // 
            // tsbUp
            // 
            this.tsbUp.AutoSize = false;
            this.tsbUp.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tsbUp.Image = global::windows_xp_like.Properties.Resources.tb_up_icon;
            this.tsbUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUp.Name = "tsbUp";
            this.tsbUp.Size = new System.Drawing.Size(38, 37);
            this.tsbUp.Text = "위로";
            this.tsbUp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbUp.Click += new System.EventHandler(this.tsbUp_Click);
            // 
            // FolderView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "FolderView";
            this.Size = new System.Drawing.Size(800, 600);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList largeImageList;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 새로만들기ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 폴더ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 삭제ToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 파일FToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 새로만들기NToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 폴더FToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 보기VToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 큰아이콘LToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 자세히DToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 간단히SToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbBack;
        private System.Windows.Forms.ToolStripButton tsbUp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox tstSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList smallImageList;
    }
}
