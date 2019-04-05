namespace AutoWork_Plat1
{
    partial class frmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.登陆账户 = new System.Windows.Forms.ToolStripMenuItem();
            this.日志 = new System.Windows.Forms.ToolStripMenuItem();
            this.配置 = new System.Windows.Forms.ToolStripMenuItem();
            this.重启 = new System.Windows.Forms.ToolStripMenuItem();
            this.清空 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lvRecorder = new System.Windows.Forms.ListBox();
            this.登陆优惠大厅 = new System.Windows.Forms.ToolStripMenuItem();
            this.登陆BB后台 = new System.Windows.Forms.ToolStripMenuItem();
            this.登陆GPK = new System.Windows.Forms.ToolStripMenuItem();
            this.登陆彩金后台 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.登陆账户,
            this.日志,
            this.配置,
            this.重启,
            this.清空});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(670, 25);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 登陆账户
            // 
            this.登陆账户.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.登陆优惠大厅,
            this.登陆BB后台,
            this.登陆GPK,
            this.登陆彩金后台});
            this.登陆账户.Name = "登陆账户";
            this.登陆账户.Size = new System.Drawing.Size(68, 21);
            this.登陆账户.Text = "登陆账户";
            // 
            // 日志
            // 
            this.日志.Name = "日志";
            this.日志.Size = new System.Drawing.Size(68, 21);
            this.日志.Text = "查看日志";
            this.日志.Click += new System.EventHandler(this.日志_Click);
            // 
            // 配置
            // 
            this.配置.Name = "配置";
            this.配置.Size = new System.Drawing.Size(68, 21);
            this.配置.Text = "查看配置";
            this.配置.Click += new System.EventHandler(this.配置_Click);
            // 
            // 重启
            // 
            this.重启.Name = "重启";
            this.重启.Size = new System.Drawing.Size(68, 21);
            this.重启.Text = "重启程序";
            this.重启.Click += new System.EventHandler(this.重启_Click);
            // 
            // 清空
            // 
            this.清空.Name = "清空";
            this.清空.Size = new System.Drawing.Size(68, 21);
            this.清空.Text = "清空显示";
            this.清空.Click += new System.EventHandler(this.清空_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 372);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(670, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // lvRecorder
            // 
            this.lvRecorder.BackColor = System.Drawing.SystemColors.Control;
            this.lvRecorder.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvRecorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvRecorder.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lvRecorder.FormattingEnabled = true;
            this.lvRecorder.ItemHeight = 19;
            this.lvRecorder.Location = new System.Drawing.Point(0, 25);
            this.lvRecorder.Name = "lvRecorder";
            this.lvRecorder.Size = new System.Drawing.Size(670, 347);
            this.lvRecorder.TabIndex = 11;
            // 
            // 登陆优惠大厅
            // 
            this.登陆优惠大厅.Name = "登陆优惠大厅";
            this.登陆优惠大厅.Size = new System.Drawing.Size(152, 22);
            this.登陆优惠大厅.Text = "登陆优惠大厅";
            this.登陆优惠大厅.Click += new System.EventHandler(this.登陆优惠大厅_Click);
            // 
            // 登陆BB后台
            // 
            this.登陆BB后台.Name = "登陆BB后台";
            this.登陆BB后台.Size = new System.Drawing.Size(152, 22);
            this.登陆BB后台.Text = "登陆BB后台";
            this.登陆BB后台.Click += new System.EventHandler(this.登陆BB后台_Click);
            // 
            // 登陆GPK
            // 
            this.登陆GPK.Name = "登陆GPK";
            this.登陆GPK.Size = new System.Drawing.Size(152, 22);
            this.登陆GPK.Text = "登陆GPK";
            this.登陆GPK.Click += new System.EventHandler(this.登陆GPK_Click);
            // 
            // 登陆彩金后台
            // 
            this.登陆彩金后台.Name = "登陆彩金后台";
            this.登陆彩金后台.Size = new System.Drawing.Size(152, 22);
            this.登陆彩金后台.Text = "登陆彩金后台";
            this.登陆彩金后台.Click += new System.EventHandler(this.登陆彩金后台_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 394);
            this.Controls.Add(this.lvRecorder);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "frmMain";
            this.Text = "活动处理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 登陆账户;
        private System.Windows.Forms.ToolStripMenuItem 日志;
        private System.Windows.Forms.ToolStripMenuItem 配置;
        private System.Windows.Forms.ToolStripMenuItem 重启;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ListBox lvRecorder;
        private System.Windows.Forms.ToolStripMenuItem 清空;
        private System.Windows.Forms.ToolStripMenuItem 登陆优惠大厅;
        private System.Windows.Forms.ToolStripMenuItem 登陆BB后台;
        private System.Windows.Forms.ToolStripMenuItem 登陆GPK;
        private System.Windows.Forms.ToolStripMenuItem 登陆彩金后台;
    }
}

