namespace ADCS.SidExtension.PolicyModule.Forms {
    partial class ConfigDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigDialog));
            this.btnClose = new System.Windows.Forms.Button();
            this.gbUntrustedSidPolicy = new System.Windows.Forms.GroupBox();
            this.cmbUntrustedSidPolicy = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvMap = new System.Windows.Forms.DataGridView();
            this.cbTrustedSidPolicy = new System.Windows.Forms.GroupBox();
            this.cmbTrustedSidPolicy = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.gbLogging = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbLogging = new System.Windows.Forms.ComboBox();
            this.gbNativePolicy = new System.Windows.Forms.GroupBox();
            this.cmbNativePolicy = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbActiveDirectory = new System.Windows.Forms.GroupBox();
            this.chkDoNotUseGC = new System.Windows.Forms.CheckBox();
            this.ttChkDoNotUseGC = new System.Windows.Forms.ToolTip(this.components);
            this.gbUntrustedSidPolicy.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMap)).BeginInit();
            this.cbTrustedSidPolicy.SuspendLayout();
            this.gbLogging.SuspendLayout();
            this.gbNativePolicy.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.gbActiveDirectory.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(706, 583);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // gbUntrustedSidPolicy
            // 
            this.gbUntrustedSidPolicy.Controls.Add(this.cmbUntrustedSidPolicy);
            this.gbUntrustedSidPolicy.Controls.Add(this.label1);
            this.gbUntrustedSidPolicy.Location = new System.Drawing.Point(13, 13);
            this.gbUntrustedSidPolicy.Name = "gbUntrustedSidPolicy";
            this.gbUntrustedSidPolicy.Size = new System.Drawing.Size(381, 172);
            this.gbUntrustedSidPolicy.TabIndex = 1;
            this.gbUntrustedSidPolicy.TabStop = false;
            this.gbUntrustedSidPolicy.Text = "Untrusted SID extension policy";
            // 
            // cmbUntrustedSidPolicy
            // 
            this.cmbUntrustedSidPolicy.FormattingEnabled = true;
            this.cmbUntrustedSidPolicy.Location = new System.Drawing.Point(10, 145);
            this.cmbUntrustedSidPolicy.Name = "cmbUntrustedSidPolicy";
            this.cmbUntrustedSidPolicy.Size = new System.Drawing.Size(188, 21);
            this.cmbUntrustedSidPolicy.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(368, 122);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvMap);
            this.groupBox2.Location = new System.Drawing.Point(13, 347);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(768, 230);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Template/Requester mapping";
            // 
            // dgvMap
            // 
            this.dgvMap.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgvMap.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMap.Location = new System.Drawing.Point(10, 20);
            this.dgvMap.Name = "dgvMap";
            this.dgvMap.Size = new System.Drawing.Size(752, 204);
            this.dgvMap.TabIndex = 0;
            // 
            // cbTrustedSidPolicy
            // 
            this.cbTrustedSidPolicy.Controls.Add(this.cmbTrustedSidPolicy);
            this.cbTrustedSidPolicy.Controls.Add(this.label2);
            this.cbTrustedSidPolicy.Location = new System.Drawing.Point(400, 13);
            this.cbTrustedSidPolicy.Name = "cbTrustedSidPolicy";
            this.cbTrustedSidPolicy.Size = new System.Drawing.Size(381, 172);
            this.cbTrustedSidPolicy.TabIndex = 3;
            this.cbTrustedSidPolicy.TabStop = false;
            this.cbTrustedSidPolicy.Text = "Trusted SID extension policy";
            // 
            // cmbTrustedSidPolicy
            // 
            this.cmbTrustedSidPolicy.FormattingEnabled = true;
            this.cmbTrustedSidPolicy.Location = new System.Drawing.Point(10, 145);
            this.cmbTrustedSidPolicy.Name = "cmbTrustedSidPolicy";
            this.cmbTrustedSidPolicy.Size = new System.Drawing.Size(186, 21);
            this.cmbTrustedSidPolicy.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(7, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(368, 122);
            this.label2.TabIndex = 0;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(625, 583);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // gbLogging
            // 
            this.gbLogging.Controls.Add(this.label3);
            this.gbLogging.Controls.Add(this.cmbLogging);
            this.gbLogging.Location = new System.Drawing.Point(13, 191);
            this.gbLogging.Name = "gbLogging";
            this.gbLogging.Size = new System.Drawing.Size(381, 100);
            this.gbLogging.TabIndex = 5;
            this.gbLogging.TabStop = false;
            this.gbLogging.Text = "Logging level";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(368, 50);
            this.label3.TabIndex = 1;
            this.label3.Text = "Select the logging level. Log file is saved in %windir% folder.\r\nDefault level is" +
    " \'None\'.";
            // 
            // cmbLogging
            // 
            this.cmbLogging.FormattingEnabled = true;
            this.cmbLogging.Location = new System.Drawing.Point(10, 73);
            this.cmbLogging.Name = "cmbLogging";
            this.cmbLogging.Size = new System.Drawing.Size(188, 21);
            this.cmbLogging.TabIndex = 0;
            // 
            // gbNativePolicy
            // 
            this.gbNativePolicy.Controls.Add(this.cmbNativePolicy);
            this.gbNativePolicy.Controls.Add(this.label4);
            this.gbNativePolicy.Location = new System.Drawing.Point(400, 191);
            this.gbNativePolicy.Name = "gbNativePolicy";
            this.gbNativePolicy.Size = new System.Drawing.Size(381, 100);
            this.gbNativePolicy.TabIndex = 6;
            this.gbNativePolicy.TabStop = false;
            this.gbNativePolicy.Text = "Native policy module";
            // 
            // cmbNativePolicy
            // 
            this.cmbNativePolicy.FormattingEnabled = true;
            this.cmbNativePolicy.Location = new System.Drawing.Point(10, 73);
            this.cmbNativePolicy.Name = "cmbNativePolicy";
            this.cmbNativePolicy.Size = new System.Drawing.Size(188, 21);
            this.cmbNativePolicy.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(368, 50);
            this.label4.TabIndex = 0;
            this.label4.Text = "Select the underlying policy module to pass requests to.\r\nDefault is \'Windows Def" +
    "ault\' policy module.";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 616);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(797, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(142, 17);
            this.toolStripStatusLabel1.Text = "© PKI Solutions LLC, 2023";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(130, 17);
            this.toolStripStatusLabel2.Text = "Author: Vadims Podans";
            // 
            // gbActiveDirectory
            // 
            this.gbActiveDirectory.Controls.Add(this.chkDoNotUseGC);
            this.gbActiveDirectory.Location = new System.Drawing.Point(14, 297);
            this.gbActiveDirectory.Name = "gbActiveDirectory";
            this.gbActiveDirectory.Size = new System.Drawing.Size(768, 44);
            this.gbActiveDirectory.TabIndex = 8;
            this.gbActiveDirectory.TabStop = false;
            this.gbActiveDirectory.Text = "Active Directory";
            // 
            // chkDoNotUseGC
            // 
            this.chkDoNotUseGC.AutoSize = true;
            this.chkDoNotUseGC.Location = new System.Drawing.Point(10, 20);
            this.chkDoNotUseGC.Name = "chkDoNotUseGC";
            this.chkDoNotUseGC.Size = new System.Drawing.Size(150, 17);
            this.chkDoNotUseGC.TabIndex = 0;
            this.chkDoNotUseGC.Text = "Do not use Global Catalog";
            this.ttChkDoNotUseGC.SetToolTip(this.chkDoNotUseGC, resources.GetString("chkDoNotUseGC.ToolTip"));
            this.chkDoNotUseGC.UseVisualStyleBackColor = true;
            // 
            // ConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(797, 638);
            this.Controls.Add(this.gbActiveDirectory);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.gbNativePolicy);
            this.Controls.Add(this.gbLogging);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.cbTrustedSidPolicy);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.gbUntrustedSidPolicy);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.gbUntrustedSidPolicy.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMap)).EndInit();
            this.cbTrustedSidPolicy.ResumeLayout(false);
            this.gbLogging.ResumeLayout(false);
            this.gbNativePolicy.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbActiveDirectory.ResumeLayout(false);
            this.gbActiveDirectory.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox gbUntrustedSidPolicy;
        private System.Windows.Forms.ComboBox cmbUntrustedSidPolicy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvMap;
        private System.Windows.Forms.GroupBox cbTrustedSidPolicy;
        private System.Windows.Forms.ComboBox cmbTrustedSidPolicy;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox gbLogging;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbLogging;
        private System.Windows.Forms.GroupBox gbNativePolicy;
        private System.Windows.Forms.ComboBox cmbNativePolicy;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.GroupBox gbActiveDirectory;
        private System.Windows.Forms.CheckBox chkDoNotUseGC;
        private System.Windows.Forms.ToolTip ttChkDoNotUseGC;
    }
}