namespace Plugin.MenuArrangement
{
	partial class PanelMenuArrangement
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelMenuArrangement));
			this.tvMenuItems = new System.Windows.Forms.TreeView();
			this.tsMain = new System.Windows.Forms.ToolStrip();
			this.tsbnMoveUp = new System.Windows.Forms.ToolStripButton();
			this.tsbnMoveDown = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbnIndent = new System.Windows.Forms.ToolStripButton();
			this.tsbnOutdent = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbnReset = new System.Windows.Forms.ToolStripButton();
			this.tsMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// tvMenuItems
			// 
			this.tvMenuItems.AllowDrop = true;
			this.tvMenuItems.CheckBoxes = true;
			this.tvMenuItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvMenuItems.HideSelection = false;
			this.tvMenuItems.Location = new System.Drawing.Point(0, 25);
			this.tvMenuItems.Name = "tvMenuItems";
			this.tvMenuItems.Size = new System.Drawing.Size(306, 328);
			this.tvMenuItems.TabIndex = 0;
			this.tvMenuItems.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvMenuItems_AfterCheck);
			this.tvMenuItems.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvMenuItems_ItemDrag);
			this.tvMenuItems.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvMenuItems_AfterSelect);
			this.tvMenuItems.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvMenuItems_DragDrop);
			this.tvMenuItems.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvMenuItems_DragEnter);
			this.tvMenuItems.DragOver += new System.Windows.Forms.DragEventHandler(this.tvMenuItems_DragOver);
			// 
			// tsMain
			// 
			this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbnMoveUp,
            this.tsbnMoveDown,
            this.toolStripSeparator1,
            this.tsbnOutdent,
            this.tsbnIndent,
            this.toolStripSeparator2,
            this.tsbnReset});
			this.tsMain.Location = new System.Drawing.Point(0, 0);
			this.tsMain.Name = "tsMain";
			this.tsMain.Size = new System.Drawing.Size(306, 25);
			this.tsMain.TabIndex = 8;
			// 
			// tsbnMoveUp
			// 
			this.tsbnMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnMoveUp.Enabled = false;
			this.tsbnMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("tsbnMoveUp.Image")));
			this.tsbnMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnMoveUp.Name = "tsbnMoveUp";
			this.tsbnMoveUp.Size = new System.Drawing.Size(23, 22);
			this.tsbnMoveUp.Text = "Move &Up";
			this.tsbnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
			// 
			// tsbnMoveDown
			// 
			this.tsbnMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnMoveDown.Enabled = false;
			this.tsbnMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("tsbnMoveDown.Image")));
			this.tsbnMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnMoveDown.Name = "tsbnMoveDown";
			this.tsbnMoveDown.Size = new System.Drawing.Size(23, 22);
			this.tsbnMoveDown.Text = "Move &Down";
			this.tsbnMoveDown.Click += new System.EventHandler(this.tsbnMoveDown_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbnIndent
			// 
			this.tsbnIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnIndent.Enabled = false;
			this.tsbnIndent.Image = ((System.Drawing.Image)(resources.GetObject("tsbnIndent.Image")));
			this.tsbnIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnIndent.Name = "tsbnIndent";
			this.tsbnIndent.Size = new System.Drawing.Size(23, 22);
			this.tsbnIndent.Text = "&Indent →";
			this.tsbnIndent.Click += new System.EventHandler(this.tsbnIndent_Click);
			// 
			// tsbnOutdent
			// 
			this.tsbnOutdent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnOutdent.Enabled = false;
			this.tsbnOutdent.Image = ((System.Drawing.Image)(resources.GetObject("tsbnOutdent.Image")));
			this.tsbnOutdent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnOutdent.Name = "tsbnOutdent";
			this.tsbnOutdent.Size = new System.Drawing.Size(23, 22);
			this.tsbnOutdent.Text = "← &Outdent";
			this.tsbnOutdent.Click += new System.EventHandler(this.tsbnOutdent_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbnReset
			// 
			this.tsbnReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnReset.Enabled = false;
			this.tsbnReset.Image = ((System.Drawing.Image)(resources.GetObject("tsbnReset.Image")));
			this.tsbnReset.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnReset.Name = "tsbnReset";
			this.tsbnReset.Size = new System.Drawing.Size(23, 22);
			this.tsbnReset.Text = "&Reset";
			this.tsbnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// PanelMenuArrangement
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tvMenuItems);
			this.Controls.Add(this.tsMain);
			this.Name = "PanelMenuArrangement";
			this.Size = new System.Drawing.Size(306, 353);
			this.tsMain.ResumeLayout(false);
			this.tsMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TreeView tvMenuItems;
		private System.Windows.Forms.ToolStrip tsMain;
		private System.Windows.Forms.ToolStripButton tsbnMoveUp;
		private System.Windows.Forms.ToolStripButton tsbnMoveDown;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsbnIndent;
		private System.Windows.Forms.ToolStripButton tsbnOutdent;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton tsbnReset;
	}
}
