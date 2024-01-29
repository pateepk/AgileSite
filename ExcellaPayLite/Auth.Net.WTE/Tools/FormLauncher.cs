using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ssch.tools.Properties;

namespace ssch.tools
{
    public partial class FormLauncher : Form
    {
        public FormLauncher()
        {
            InitializeComponent();
        }

        public const string LOAD_DB_SCRIPT = "LOAD_DB_SCRIPT";                  // arguments for Program 04 : Load Current DB Script
        public const string GENERATE_INSERT_SCRIPT = "GENERATE_INSERT_SCRIPT";  // arguments for Program 05 : Generate Insert Script For Table
        public const string ADD_NEW_DBSCRIPT = "ADD_NEW_DBSCRIPT";              // arguments for Program 06 : Add new (template) file with standard format to DB script folder
        public const string ADD_TO_DEPLOYMENT = "ADD_TO_DEPLOYMENT";            // arguments for Program 07 : Add current file to deployment for next release

        // Add new form here #1
        private string[] AForms = { 
                                        "01. Data Reader Generator (Generate objects from Stored Procedure)"
                                       ,"02. Stored Procedure Generator (Generate SQL Script for set/get/update)"
                                       ,"03. Braintree Sandbox Testing"
                                       ,"04. EGivings Copy dll to prod"
                                  };

        // Add new form here #2 (new function)
        public static void Form01()
        {
            Application.Run(new DataReaderGenerator());
        }

        public static void Form02()
        {
            Application.Run(new StoredProcedureGenerator());
        }

        public static void Form03()
        {
            Application.Run(new BraintreeSandBox());
        }

        public static void Form04()
        {
            Application.Run(new EGivingsCopyToProd());
        }

        private void cmdRun_Click(object sender, EventArgs e)
        {
            System.Threading.Thread tform = null;
            int findex = cmbFormList.SelectedIndex + 1;

            // Add new form here #3 (new case)
            switch (findex)
            {
                case 1:
                    tform = new System.Threading.Thread(new System.Threading.ThreadStart(Form01));
                    break;
                case 2:
                    tform = new System.Threading.Thread(new System.Threading.ThreadStart(Form02));
                    break;
                case 3:
                    tform = new System.Threading.Thread(new System.Threading.ThreadStart(Form03));
                    break;
                case 4:
                    tform = new System.Threading.Thread(new System.Threading.ThreadStart(Form04));
                    break;
            }
            tform.SetApartmentState(ApartmentState.STA);
            tform.Start();
            this.Close();
        }

        private void FormLauncher_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < AForms.Length; i++)
            {
                cmbFormList.Items.Add(AForms[i]);
            }
            if (cmbFormList.Items.Count > 0)
            {
                cmbFormList.SelectedIndex = Settings.Default.LastForm;
            }

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string cmd = args[1].Trim();
                switch (cmd)
                {
                    // Add new form here #4 if new program is External Tools in Visual Studio
                    case LOAD_DB_SCRIPT:
                        cmbFormList.SelectedIndex = 3;
                        
                        break;
                    // Add new form here #5 if new program is External Tools in Visual Studio
                    case GENERATE_INSERT_SCRIPT:
                        cmbFormList.SelectedIndex = 4;
                        break;
                    // Add new form here #6 if new program is External Tools in Visual Studio
                    case ADD_NEW_DBSCRIPT:
                        cmbFormList.SelectedIndex = 5;
                        break;
                    // Add new form here #7 if new program is External Tools in Visual Studio
                    case ADD_TO_DEPLOYMENT:
                        cmbFormList.SelectedIndex = 6;
                        break;
                }
                cmdRun_Click(null, null);
            }

        }

        private void cmbFormList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.LastForm = cmbFormList.SelectedIndex;
            Settings.Default.Save();
        }

    }
}
