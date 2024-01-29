namespace ssch.tools
{
    partial class FormLauncher
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
            this.cmdRun = new System.Windows.Forms.Button();
            this.cmbFormList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmdRun
            // 
            this.cmdRun.Location = new System.Drawing.Point(378, 12);
            this.cmdRun.Name = "cmdRun";
            this.cmdRun.Size = new System.Drawing.Size(75, 23);
            this.cmdRun.TabIndex = 0;
            this.cmdRun.Text = "Run";
            this.cmdRun.UseVisualStyleBackColor = true;
            this.cmdRun.Click += new System.EventHandler(this.cmdRun_Click);
            // 
            // cmbFormList
            // 
            this.cmbFormList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormList.FormattingEnabled = true;
            this.cmbFormList.Location = new System.Drawing.Point(13, 13);
            this.cmbFormList.Name = "cmbFormList";
            this.cmbFormList.Size = new System.Drawing.Size(359, 21);
            this.cmbFormList.TabIndex = 1;
            this.cmbFormList.SelectedIndexChanged += new System.EventHandler(this.cmbFormList_SelectedIndexChanged);
            // 
            // FormLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 54);
            this.Controls.Add(this.cmbFormList);
            this.Controls.Add(this.cmdRun);
            this.Name = "FormLauncher";
            this.Text = "Form Launcher (V.1.2.1)";
            this.Load += new System.EventHandler(this.FormLauncher_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdRun;
        private System.Windows.Forms.ComboBox cmbFormList;
    }
}