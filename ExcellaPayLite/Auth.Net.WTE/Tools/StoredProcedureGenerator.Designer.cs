namespace ssch.tools
{
    partial class StoredProcedureGenerator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StoredProcedureGenerator));
            this.cmdListAllTables = new System.Windows.Forms.Button();
            this.lstAllDB = new System.Windows.Forms.ListBox();
            this.lstTables = new System.Windows.Forms.ListBox();
            this.cmdGenerateSP = new System.Windows.Forms.Button();
            this.txtObject = new System.Windows.Forms.TextBox();
            this.cmdExit = new System.Windows.Forms.Button();
            this.txtSPTemplate = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAuthorName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDecsription = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radAlter = new System.Windows.Forms.RadioButton();
            this.radCreate = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rad3045UpdateOne = new System.Windows.Forms.RadioButton();
            this.rad3045Set = new System.Windows.Forms.RadioButton();
            this.rad3045Update = new System.Windows.Forms.RadioButton();
            this.radUpdate = new System.Windows.Forms.RadioButton();
            this.radGet = new System.Windows.Forms.RadioButton();
            this.radSet = new System.Windows.Forms.RadioButton();
            this.cmdCopyToClipboard = new System.Windows.Forms.Button();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.cmbServers = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.txtSPTemplateHCBS = new System.Windows.Forms.TextBox();
            this.txtSPTemplateNonHCBS = new System.Windows.Forms.TextBox();
            this.radInsertData = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdListAllTables
            // 
            this.cmdListAllTables.Location = new System.Drawing.Point(13, 155);
            this.cmdListAllTables.Name = "cmdListAllTables";
            this.cmdListAllTables.Size = new System.Drawing.Size(144, 23);
            this.cmdListAllTables.TabIndex = 0;
            this.cmdListAllTables.Text = "List All Tables";
            this.cmdListAllTables.UseVisualStyleBackColor = true;
            this.cmdListAllTables.Click += new System.EventHandler(this.cmdListAllTables_Click);
            // 
            // lstAllDB
            // 
            this.lstAllDB.FormattingEnabled = true;
            this.lstAllDB.Location = new System.Drawing.Point(13, 54);
            this.lstAllDB.Name = "lstAllDB";
            this.lstAllDB.Size = new System.Drawing.Size(241, 95);
            this.lstAllDB.TabIndex = 1;
            this.lstAllDB.SelectedIndexChanged += new System.EventHandler(this.lstAllDB_SelectedIndexChanged_1);
            // 
            // lstTables
            // 
            this.lstTables.FormattingEnabled = true;
            this.lstTables.Location = new System.Drawing.Point(13, 184);
            this.lstTables.Name = "lstTables";
            this.lstTables.Size = new System.Drawing.Size(241, 277);
            this.lstTables.TabIndex = 2;
            // 
            // cmdGenerateSP
            // 
            this.cmdGenerateSP.Location = new System.Drawing.Point(539, 467);
            this.cmdGenerateSP.Name = "cmdGenerateSP";
            this.cmdGenerateSP.Size = new System.Drawing.Size(217, 23);
            this.cmdGenerateSP.TabIndex = 3;
            this.cmdGenerateSP.Text = "Generate Stored Procedure";
            this.cmdGenerateSP.UseVisualStyleBackColor = true;
            this.cmdGenerateSP.Click += new System.EventHandler(this.cmdGenerateSP_Click);
            // 
            // txtObject
            // 
            this.txtObject.Location = new System.Drawing.Point(539, 31);
            this.txtObject.Multiline = true;
            this.txtObject.Name = "txtObject";
            this.txtObject.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObject.Size = new System.Drawing.Size(437, 430);
            this.txtObject.TabIndex = 4;
            // 
            // cmdExit
            // 
            this.cmdExit.Location = new System.Drawing.Point(887, 466);
            this.cmdExit.Name = "cmdExit";
            this.cmdExit.Size = new System.Drawing.Size(89, 23);
            this.cmdExit.TabIndex = 5;
            this.cmdExit.Text = "Exit";
            this.cmdExit.UseVisualStyleBackColor = true;
            this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
            // 
            // txtSPTemplate
            // 
            this.txtSPTemplate.Location = new System.Drawing.Point(263, 70);
            this.txtSPTemplate.Multiline = true;
            this.txtSPTemplate.Name = "txtSPTemplate";
            this.txtSPTemplate.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSPTemplate.Size = new System.Drawing.Size(266, 127);
            this.txtSPTemplate.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(260, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Stored Procedure Template:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtAuthorName
            // 
            this.txtAuthorName.Location = new System.Drawing.Point(264, 221);
            this.txtAuthorName.Name = "txtAuthorName";
            this.txtAuthorName.Size = new System.Drawing.Size(265, 20);
            this.txtAuthorName.TabIndex = 8;
            this.txtAuthorName.Text = "Chacha Nugroho";
            this.txtAuthorName.TextChanged += new System.EventHandler(this.txtAuthorName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(261, 205);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Author:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(261, 244);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Description:";
            // 
            // txtDecsription
            // 
            this.txtDecsription.Location = new System.Drawing.Point(264, 260);
            this.txtDecsription.Multiline = true;
            this.txtDecsription.Name = "txtDecsription";
            this.txtDecsription.Size = new System.Drawing.Size(267, 52);
            this.txtDecsription.TabIndex = 11;
            this.txtDecsription.Text = "some description";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radAlter);
            this.groupBox1.Controls.Add(this.radCreate);
            this.groupBox1.Location = new System.Drawing.Point(263, 417);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 44);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Create or Alter:";
            // 
            // radAlter
            // 
            this.radAlter.AutoSize = true;
            this.radAlter.Location = new System.Drawing.Point(122, 20);
            this.radAlter.Name = "radAlter";
            this.radAlter.Size = new System.Drawing.Size(60, 17);
            this.radAlter.TabIndex = 1;
            this.radAlter.Text = "ALTER";
            this.radAlter.UseVisualStyleBackColor = true;
            this.radAlter.CheckedChanged += new System.EventHandler(this.radAlter_CheckedChanged);
            // 
            // radCreate
            // 
            this.radCreate.AutoSize = true;
            this.radCreate.Location = new System.Drawing.Point(23, 20);
            this.radCreate.Name = "radCreate";
            this.radCreate.Size = new System.Drawing.Size(68, 17);
            this.radCreate.TabIndex = 0;
            this.radCreate.Text = "CREATE";
            this.radCreate.UseVisualStyleBackColor = true;
            this.radCreate.CheckedChanged += new System.EventHandler(this.radCreate_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(536, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Stored Procedure Script:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(8, 464);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(274, 22);
            this.label8.TabIndex = 22;
            this.label8.Text = "Stored Procedure Generator";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radInsertData);
            this.groupBox2.Controls.Add(this.rad3045UpdateOne);
            this.groupBox2.Controls.Add(this.rad3045Set);
            this.groupBox2.Controls.Add(this.rad3045Update);
            this.groupBox2.Controls.Add(this.radUpdate);
            this.groupBox2.Controls.Add(this.radGet);
            this.groupBox2.Controls.Add(this.radSet);
            this.groupBox2.Location = new System.Drawing.Point(263, 318);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 93);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Type Of Stored Procedure";
            // 
            // rad3045UpdateOne
            // 
            this.rad3045UpdateOne.AutoSize = true;
            this.rad3045UpdateOne.Location = new System.Drawing.Point(24, 67);
            this.rad3045UpdateOne.Name = "rad3045UpdateOne";
            this.rad3045UpdateOne.Size = new System.Drawing.Size(122, 17);
            this.rad3045UpdateOne.TabIndex = 5;
            this.rad3045UpdateOne.TabStop = true;
            this.rad3045UpdateOne.Text = "3045 UPDATE ONE";
            this.rad3045UpdateOne.UseVisualStyleBackColor = true;
            this.rad3045UpdateOne.CheckedChanged += new System.EventHandler(this.rad3045UpdateOne_CheckedChanged);
            // 
            // rad3045Set
            // 
            this.rad3045Set.AutoSize = true;
            this.rad3045Set.Location = new System.Drawing.Point(24, 43);
            this.rad3045Set.Name = "rad3045Set";
            this.rad3045Set.Size = new System.Drawing.Size(73, 17);
            this.rad3045Set.TabIndex = 4;
            this.rad3045Set.TabStop = true;
            this.rad3045Set.Text = "3045 SET";
            this.rad3045Set.UseVisualStyleBackColor = true;
            this.rad3045Set.CheckedChanged += new System.EventHandler(this.rad3045Set_CheckedChanged);
            // 
            // rad3045Update
            // 
            this.rad3045Update.AutoSize = true;
            this.rad3045Update.Location = new System.Drawing.Point(122, 43);
            this.rad3045Update.Name = "rad3045Update";
            this.rad3045Update.Size = new System.Drawing.Size(96, 17);
            this.rad3045Update.TabIndex = 3;
            this.rad3045Update.TabStop = true;
            this.rad3045Update.Text = "3045 UPDATE";
            this.rad3045Update.UseVisualStyleBackColor = true;
            this.rad3045Update.CheckedChanged += new System.EventHandler(this.rad3045Update_CheckedChanged);
            // 
            // radUpdate
            // 
            this.radUpdate.AutoSize = true;
            this.radUpdate.Location = new System.Drawing.Point(196, 19);
            this.radUpdate.Name = "radUpdate";
            this.radUpdate.Size = new System.Drawing.Size(69, 17);
            this.radUpdate.TabIndex = 2;
            this.radUpdate.TabStop = true;
            this.radUpdate.Text = "UPDATE";
            this.radUpdate.UseVisualStyleBackColor = true;
            this.radUpdate.CheckedChanged += new System.EventHandler(this.radUpdate_CheckedChanged);
            // 
            // radGet
            // 
            this.radGet.AutoSize = true;
            this.radGet.Location = new System.Drawing.Point(123, 19);
            this.radGet.Name = "radGet";
            this.radGet.Size = new System.Drawing.Size(47, 17);
            this.radGet.TabIndex = 1;
            this.radGet.Text = "GET";
            this.radGet.UseVisualStyleBackColor = true;
            this.radGet.CheckedChanged += new System.EventHandler(this.radGet_CheckedChanged);
            // 
            // radSet
            // 
            this.radSet.AutoSize = true;
            this.radSet.Location = new System.Drawing.Point(24, 20);
            this.radSet.Name = "radSet";
            this.radSet.Size = new System.Drawing.Size(46, 17);
            this.radSet.TabIndex = 0;
            this.radSet.Text = "SET";
            this.radSet.UseVisualStyleBackColor = true;
            this.radSet.CheckedChanged += new System.EventHandler(this.radSet_CheckedChanged);
            // 
            // cmdCopyToClipboard
            // 
            this.cmdCopyToClipboard.Location = new System.Drawing.Point(763, 466);
            this.cmdCopyToClipboard.Name = "cmdCopyToClipboard";
            this.cmdCopyToClipboard.Size = new System.Drawing.Size(118, 23);
            this.cmdCopyToClipboard.TabIndex = 25;
            this.cmdCopyToClipboard.Text = "Copy To Clipboard";
            this.cmdCopyToClipboard.UseVisualStyleBackColor = true;
            this.cmdCopyToClipboard.Click += new System.EventHandler(this.cmdCopyToClipboard_Click);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(212, 19);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(69, 17);
            this.radioButton5.TabIndex = 2;
            this.radioButton5.Text = "UPDATE";
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // cmbServers
            // 
            this.cmbServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbServers.FormattingEnabled = true;
            this.cmbServers.Location = new System.Drawing.Point(13, 31);
            this.cmbServers.Name = "cmbServers";
            this.cmbServers.Size = new System.Drawing.Size(241, 21);
            this.cmbServers.TabIndex = 26;
            this.cmbServers.SelectedIndexChanged += new System.EventHandler(this.cmbServers_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "Servers:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(260, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 13);
            this.label6.TabIndex = 28;
            this.label6.Text = "Connection String:";
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(263, 31);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(262, 20);
            this.txtConnectionString.TabIndex = 29;
            // 
            // txtSPTemplateHCBS
            // 
            this.txtSPTemplateHCBS.Location = new System.Drawing.Point(340, 93);
            this.txtSPTemplateHCBS.Multiline = true;
            this.txtSPTemplateHCBS.Name = "txtSPTemplateHCBS";
            this.txtSPTemplateHCBS.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSPTemplateHCBS.Size = new System.Drawing.Size(164, 67);
            this.txtSPTemplateHCBS.TabIndex = 30;
            this.txtSPTemplateHCBS.Text = resources.GetString("txtSPTemplateHCBS.Text");
            this.txtSPTemplateHCBS.Visible = false;
            // 
            // txtSPTemplateNonHCBS
            // 
            this.txtSPTemplateNonHCBS.Location = new System.Drawing.Point(300, 93);
            this.txtSPTemplateNonHCBS.Multiline = true;
            this.txtSPTemplateNonHCBS.Name = "txtSPTemplateNonHCBS";
            this.txtSPTemplateNonHCBS.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSPTemplateNonHCBS.Size = new System.Drawing.Size(119, 67);
            this.txtSPTemplateNonHCBS.TabIndex = 31;
            this.txtSPTemplateNonHCBS.Text = resources.GetString("txtSPTemplateNonHCBS.Text");
            this.txtSPTemplateNonHCBS.Visible = false;
            // 
            // radInsertData
            // 
            this.radInsertData.AutoSize = true;
            this.radInsertData.Location = new System.Drawing.Point(163, 67);
            this.radInsertData.Name = "radInsertData";
            this.radInsertData.Size = new System.Drawing.Size(97, 17);
            this.radInsertData.TabIndex = 6;
            this.radInsertData.TabStop = true;
            this.radInsertData.Text = "INSERT DATA";
            this.radInsertData.UseVisualStyleBackColor = true;
            this.radInsertData.CheckedChanged += new System.EventHandler(this.radInsertData_CheckedChanged);
            // 
            // StoredProcedureGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 495);
            this.Controls.Add(this.txtSPTemplateNonHCBS);
            this.Controls.Add(this.txtSPTemplateHCBS);
            this.Controls.Add(this.txtConnectionString);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbServers);
            this.Controls.Add(this.cmdCopyToClipboard);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmdGenerateSP);
            this.Controls.Add(this.cmdExit);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtDecsription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtAuthorName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSPTemplate);
            this.Controls.Add(this.txtObject);
            this.Controls.Add(this.lstTables);
            this.Controls.Add(this.lstAllDB);
            this.Controls.Add(this.cmdListAllTables);
            this.Name = "StoredProcedureGenerator";
            this.Text = "Stored Procedure Generator";
            this.Load += new System.EventHandler(this.StoredProcedureGenerator_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdListAllTables;
        private System.Windows.Forms.ListBox lstAllDB;
        private System.Windows.Forms.ListBox lstTables;
        private System.Windows.Forms.Button cmdGenerateSP;
        private System.Windows.Forms.TextBox txtObject;
        private System.Windows.Forms.Button cmdExit;
        private System.Windows.Forms.TextBox txtSPTemplate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAuthorName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDecsription;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RadioButton radAlter;
        private System.Windows.Forms.RadioButton radCreate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button cmdCopyToClipboard;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radGet;
        private System.Windows.Forms.RadioButton radSet;
        private System.Windows.Forms.ComboBox cmbServers;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radUpdate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.TextBox txtSPTemplateHCBS;
        private System.Windows.Forms.TextBox txtSPTemplateNonHCBS;
        private System.Windows.Forms.RadioButton rad3045Update;
        private System.Windows.Forms.RadioButton rad3045Set;
        private System.Windows.Forms.RadioButton rad3045UpdateOne;
        private System.Windows.Forms.RadioButton radInsertData;
    }
}