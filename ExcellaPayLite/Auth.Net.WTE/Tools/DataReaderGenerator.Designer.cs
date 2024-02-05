namespace ssch.tools
{
    partial class DataReaderGenerator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataReaderGenerator));
            this.cmdListAllSPs = new System.Windows.Forms.Button();
            this.lstAllDB = new System.Windows.Forms.ListBox();
            this.lstSPs = new System.Windows.Forms.ListBox();
            this.cmdGenerateObject = new System.Windows.Forms.Button();
            this.txtObject = new System.Windows.Forms.TextBox();
            this.cmdExit = new System.Windows.Forms.Button();
            this.txtReferences = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.cmdCopyToClipboard = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkReportClass = new System.Windows.Forms.CheckBox();
            this.chkGenerateConstant = new System.Windows.Forms.CheckBox();
            this.txtCommandConstructor = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDBConstructor = new System.Windows.Forms.TextBox();
            this.txtObjectCaller = new System.Windows.Forms.TextBox();
            this.cmdCopyToClipboardCaller = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtConstantLine = new System.Windows.Forms.TextBox();
            this.cmdCopyToClipboardConstant = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbServers = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtSaveTo = new System.Windows.Forms.TextBox();
            this.cmdSaveToFile = new System.Windows.Forms.Button();
            this.txtSaveToFilename = new System.Windows.Forms.TextBox();
            this.txtObjectModel = new System.Windows.Forms.TextBox();
            this.btnCopyObjectModel = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdListAllSPs
            // 
            this.cmdListAllSPs.Location = new System.Drawing.Point(13, 123);
            this.cmdListAllSPs.Name = "cmdListAllSPs";
            this.cmdListAllSPs.Size = new System.Drawing.Size(144, 23);
            this.cmdListAllSPs.TabIndex = 0;
            this.cmdListAllSPs.Text = "List All SPs";
            this.cmdListAllSPs.UseVisualStyleBackColor = true;
            this.cmdListAllSPs.Click += new System.EventHandler(this.cmdListAllSPs_Click);
            // 
            // lstAllDB
            // 
            this.lstAllDB.FormattingEnabled = true;
            this.lstAllDB.Location = new System.Drawing.Point(13, 61);
            this.lstAllDB.Name = "lstAllDB";
            this.lstAllDB.Size = new System.Drawing.Size(261, 56);
            this.lstAllDB.TabIndex = 1;
            this.lstAllDB.SelectedIndexChanged += new System.EventHandler(this.lstAllDB_SelectedIndexChanged_1);
            // 
            // lstSPs
            // 
            this.lstSPs.FormattingEnabled = true;
            this.lstSPs.Location = new System.Drawing.Point(13, 152);
            this.lstSPs.Name = "lstSPs";
            this.lstSPs.Size = new System.Drawing.Size(261, 160);
            this.lstSPs.TabIndex = 2;
            // 
            // cmdGenerateObject
            // 
            this.cmdGenerateObject.Location = new System.Drawing.Point(12, 462);
            this.cmdGenerateObject.Name = "cmdGenerateObject";
            this.cmdGenerateObject.Size = new System.Drawing.Size(217, 23);
            this.cmdGenerateObject.TabIndex = 3;
            this.cmdGenerateObject.Text = "Generate Object/Code";
            this.cmdGenerateObject.UseVisualStyleBackColor = true;
            this.cmdGenerateObject.Click += new System.EventHandler(this.cmdGenerateObject_Click);
            // 
            // txtObject
            // 
            this.txtObject.Location = new System.Drawing.Point(456, 31);
            this.txtObject.Multiline = true;
            this.txtObject.Name = "txtObject";
            this.txtObject.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObject.Size = new System.Drawing.Size(519, 139);
            this.txtObject.TabIndex = 4;
            // 
            // cmdExit
            // 
            this.cmdExit.Location = new System.Drawing.Point(241, 462);
            this.cmdExit.Name = "cmdExit";
            this.cmdExit.Size = new System.Drawing.Size(89, 23);
            this.cmdExit.TabIndex = 5;
            this.cmdExit.Text = "Exit";
            this.cmdExit.UseVisualStyleBackColor = true;
            this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
            // 
            // txtReferences
            // 
            this.txtReferences.Location = new System.Drawing.Point(282, 139);
            this.txtReferences.Multiline = true;
            this.txtReferences.Name = "txtReferences";
            this.txtReferences.Size = new System.Drawing.Size(168, 98);
            this.txtReferences.TabIndex = 6;
            this.txtReferences.Text = resources.GetString("txtReferences.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(279, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Reference:";
            // 
            // txtNamespace
            // 
            this.txtNamespace.Location = new System.Drawing.Point(282, 253);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(168, 20);
            this.txtNamespace.TabIndex = 8;
            this.txtNamespace.Text = "EBPP.WServices.Structures";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(280, 240);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Namespace:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(280, 277);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Prefix:";
            // 
            // txtPrefix
            // 
            this.txtPrefix.Location = new System.Drawing.Point(283, 292);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(167, 20);
            this.txtPrefix.TabIndex = 11;
            this.txtPrefix.Text = "DR";
            // 
            // cmdCopyToClipboard
            // 
            this.cmdCopyToClipboard.Location = new System.Drawing.Point(864, 202);
            this.cmdCopyToClipboard.Name = "cmdCopyToClipboard";
            this.cmdCopyToClipboard.Size = new System.Drawing.Size(111, 23);
            this.cmdCopyToClipboard.TabIndex = 12;
            this.cmdCopyToClipboard.Text = "Copy To Clipboard";
            this.cmdCopyToClipboard.UseVisualStyleBackColor = true;
            this.cmdCopyToClipboard.Click += new System.EventHandler(this.cmdCopyToClipboard_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkReportClass);
            this.groupBox1.Controls.Add(this.chkGenerateConstant);
            this.groupBox1.Controls.Add(this.txtCommandConstructor);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtDBConstructor);
            this.groupBox1.Location = new System.Drawing.Point(13, 330);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 126);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Caller Options";
            // 
            // chkReportClass
            // 
            this.chkReportClass.AutoSize = true;
            this.chkReportClass.Location = new System.Drawing.Point(162, 96);
            this.chkReportClass.Name = "chkReportClass";
            this.chkReportClass.Size = new System.Drawing.Size(140, 17);
            this.chkReportClass.TabIndex = 20;
            this.chkReportClass.Text = "generate Report objects";
            this.chkReportClass.UseVisualStyleBackColor = true;
            // 
            // chkGenerateConstant
            // 
            this.chkGenerateConstant.AutoSize = true;
            this.chkGenerateConstant.Checked = true;
            this.chkGenerateConstant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateConstant.Location = new System.Drawing.Point(7, 97);
            this.chkGenerateConstant.Name = "chkGenerateConstant";
            this.chkGenerateConstant.Size = new System.Drawing.Size(131, 17);
            this.chkGenerateConstant.TabIndex = 19;
            this.chkGenerateConstant.Text = "generate constant line";
            this.chkGenerateConstant.UseVisualStyleBackColor = true;
            // 
            // txtCommandConstructor
            // 
            this.txtCommandConstructor.Location = new System.Drawing.Point(7, 70);
            this.txtCommandConstructor.Name = "txtCommandConstructor";
            this.txtCommandConstructor.Size = new System.Drawing.Size(175, 20);
            this.txtCommandConstructor.TabIndex = 18;
            this.txtCommandConstructor.Text = "SV.SP.{0}";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(189, 70);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Command Constructor";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(189, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "DB Constructor";
            // 
            // txtDBConstructor
            // 
            this.txtDBConstructor.Location = new System.Drawing.Point(7, 42);
            this.txtDBConstructor.Name = "txtDBConstructor";
            this.txtDBConstructor.Size = new System.Drawing.Size(175, 20);
            this.txtDBConstructor.TabIndex = 15;
            this.txtDBConstructor.Text = "databaseServer.DefaultDB";
            // 
            // txtObjectCaller
            // 
            this.txtObjectCaller.Location = new System.Drawing.Point(456, 386);
            this.txtObjectCaller.Multiline = true;
            this.txtObjectCaller.Name = "txtObjectCaller";
            this.txtObjectCaller.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObjectCaller.Size = new System.Drawing.Size(520, 96);
            this.txtObjectCaller.TabIndex = 15;
            // 
            // cmdCopyToClipboardCaller
            // 
            this.cmdCopyToClipboardCaller.Location = new System.Drawing.Point(864, 489);
            this.cmdCopyToClipboardCaller.Name = "cmdCopyToClipboardCaller";
            this.cmdCopyToClipboardCaller.Size = new System.Drawing.Size(112, 23);
            this.cmdCopyToClipboardCaller.TabIndex = 16;
            this.cmdCopyToClipboardCaller.Text = "Copy To Clipboard";
            this.cmdCopyToClipboardCaller.UseVisualStyleBackColor = true;
            this.cmdCopyToClipboardCaller.Click += new System.EventHandler(this.cmdCopyToClipboardCaller_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(453, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Reader Object:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(453, 370);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(156, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Caller Function / Constant Line:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // txtConstantLine
            // 
            this.txtConstantLine.Location = new System.Drawing.Point(456, 492);
            this.txtConstantLine.Name = "txtConstantLine";
            this.txtConstantLine.Size = new System.Drawing.Size(271, 20);
            this.txtConstantLine.TabIndex = 19;
            // 
            // cmdCopyToClipboardConstant
            // 
            this.cmdCopyToClipboardConstant.Location = new System.Drawing.Point(733, 490);
            this.cmdCopyToClipboardConstant.Name = "cmdCopyToClipboardConstant";
            this.cmdCopyToClipboardConstant.Size = new System.Drawing.Size(126, 23);
            this.cmdCopyToClipboardConstant.TabIndex = 21;
            this.cmdCopyToClipboardConstant.Text = "Copy To Clipboard";
            this.cmdCopyToClipboardConstant.UseVisualStyleBackColor = true;
            this.cmdCopyToClipboardConstant.Click += new System.EventHandler(this.cmdCopyToClipboardConstant_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(9, 488);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(222, 22);
            this.label8.TabIndex = 22;
            this.label8.Text = "Data Reader Generator";
            // 
            // cmbServers
            // 
            this.cmbServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbServers.FormattingEnabled = true;
            this.cmbServers.Location = new System.Drawing.Point(12, 34);
            this.cmbServers.Name = "cmbServers";
            this.cmbServers.Size = new System.Drawing.Size(262, 21);
            this.cmbServers.TabIndex = 23;
            this.cmbServers.SelectedIndexChanged += new System.EventHandler(this.cmbServers_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 13);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Servers:";
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(282, 31);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(163, 83);
            this.txtConnectionString.TabIndex = 25;
            this.txtConnectionString.TextChanged += new System.EventHandler(this.txtConnectionString_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(279, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(94, 13);
            this.label10.TabIndex = 26;
            this.label10.Text = "Connection String:";
            // 
            // txtSaveTo
            // 
            this.txtSaveTo.Location = new System.Drawing.Point(526, 176);
            this.txtSaveTo.Name = "txtSaveTo";
            this.txtSaveTo.Size = new System.Drawing.Size(450, 20);
            this.txtSaveTo.TabIndex = 27;
            this.txtSaveTo.Text = "R:\\Cath_Legacy\\ACS\\EBPP.WServices\\Structures\\{0}.cs";
            this.txtSaveTo.TextChanged += new System.EventHandler(this.txtSaveTo_TextChanged);
            // 
            // cmdSaveToFile
            // 
            this.cmdSaveToFile.Location = new System.Drawing.Point(755, 202);
            this.cmdSaveToFile.Name = "cmdSaveToFile";
            this.cmdSaveToFile.Size = new System.Drawing.Size(103, 23);
            this.cmdSaveToFile.TabIndex = 28;
            this.cmdSaveToFile.Text = "Save To File";
            this.cmdSaveToFile.UseVisualStyleBackColor = true;
            this.cmdSaveToFile.Click += new System.EventHandler(this.cmdSaveToFile_Click);
            // 
            // txtSaveToFilename
            // 
            this.txtSaveToFilename.Location = new System.Drawing.Point(526, 202);
            this.txtSaveToFilename.Name = "txtSaveToFilename";
            this.txtSaveToFilename.Size = new System.Drawing.Size(223, 20);
            this.txtSaveToFilename.TabIndex = 29;
            // 
            // txtObjectModel
            // 
            this.txtObjectModel.Location = new System.Drawing.Point(456, 253);
            this.txtObjectModel.Multiline = true;
            this.txtObjectModel.Name = "txtObjectModel";
            this.txtObjectModel.Size = new System.Drawing.Size(519, 93);
            this.txtObjectModel.TabIndex = 30;
            // 
            // btnCopyObjectModel
            // 
            this.btnCopyObjectModel.Location = new System.Drawing.Point(863, 352);
            this.btnCopyObjectModel.Name = "btnCopyObjectModel";
            this.btnCopyObjectModel.Size = new System.Drawing.Size(112, 23);
            this.btnCopyObjectModel.TabIndex = 31;
            this.btnCopyObjectModel.Text = "Copy To Clipboard";
            this.btnCopyObjectModel.UseVisualStyleBackColor = true;
            this.btnCopyObjectModel.Click += new System.EventHandler(this.btnCopyObjectModel_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(456, 237);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(39, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "Model:";
            // 
            // DataReaderGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 516);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.btnCopyObjectModel);
            this.Controls.Add(this.txtObjectModel);
            this.Controls.Add(this.txtSaveToFilename);
            this.Controls.Add(this.cmdSaveToFile);
            this.Controls.Add(this.txtSaveTo);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtConnectionString);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.cmbServers);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cmdCopyToClipboardConstant);
            this.Controls.Add(this.txtConstantLine);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmdCopyToClipboardCaller);
            this.Controls.Add(this.cmdGenerateObject);
            this.Controls.Add(this.cmdExit);
            this.Controls.Add(this.txtObjectCaller);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cmdCopyToClipboard);
            this.Controls.Add(this.txtPrefix);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtReferences);
            this.Controls.Add(this.txtObject);
            this.Controls.Add(this.lstSPs);
            this.Controls.Add(this.lstAllDB);
            this.Controls.Add(this.cmdListAllSPs);
            this.Name = "DataReaderGenerator";
            this.Text = "Data Reader Generator";
            this.Load += new System.EventHandler(this.DataReaderGenerator_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdListAllSPs;
        private System.Windows.Forms.ListBox lstAllDB;
        private System.Windows.Forms.ListBox lstSPs;
        private System.Windows.Forms.Button cmdGenerateObject;
        private System.Windows.Forms.TextBox txtObject;
        private System.Windows.Forms.Button cmdExit;
        private System.Windows.Forms.TextBox txtReferences;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.Button cmdCopyToClipboard;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtObjectCaller;
        private System.Windows.Forms.Button cmdCopyToClipboardCaller;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDBConstructor;
        private System.Windows.Forms.TextBox txtCommandConstructor;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkGenerateConstant;
        private System.Windows.Forms.TextBox txtConstantLine;
        private System.Windows.Forms.Button cmdCopyToClipboardConstant;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbServers;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtSaveTo;
        private System.Windows.Forms.Button cmdSaveToFile;
        private System.Windows.Forms.TextBox txtSaveToFilename;
        private System.Windows.Forms.CheckBox chkReportClass;
        private System.Windows.Forms.TextBox txtObjectModel;
        private System.Windows.Forms.Button btnCopyObjectModel;
        private System.Windows.Forms.Label label11;
    }
}