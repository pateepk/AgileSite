using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using ssch.tools.Properties;

namespace ssch.tools
{
    public partial class StoredProcedureGenerator : Form
    {
        private Hashtable _connectionStrings = new Hashtable();
        private string comboRadio1Value = "";
        private string comboRadio2Value = "";

        private bool isHCBS()
        {
            return (lstAllDB.SelectedItem.ToString().ToUpper() == "HCBS");
        }

        public StoredProcedureGenerator()
        {
            InitializeComponent();
        }

        private void cmdListAllTables_Click(object sender, EventArgs e)
        {
            if (lstAllDB.SelectedIndex > -1)
            {
                lstTables.Items.Clear();
                string conName = lstAllDB.SelectedItem.ToString();
                if (_connectionStrings.ContainsKey(conName))
                {
                    string conString = _connectionStrings[conName].ToString();
                    if (conString.Length > 0)
                    {
                        DRspTables_GetAll tbs = SQLData.spTables_GetAll(conString);
                        for (int i = 0; i < tbs.Count; i++)
                        {
                            lstTables.Items.Add(tbs.schema(i) + '.' + tbs.name(i));
                        }
                        if (lstTables.Items.Count > 0)
                        {
                            lstTables.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        private string cutLastWord(string s, string w)
        {
            if (s.EndsWith(w, StringComparison.CurrentCultureIgnoreCase))
            {
                s = s.Substring(0, s.Length - w.Length);
            }
            return s;
        }

        private string getConnectionSettingsFilePath()
        {
            string currentPath = Directory.GetCurrentDirectory();
            currentPath = cutLastWord(currentPath, "\\debug");
            currentPath = cutLastWord(currentPath, "\\bin");
            currentPath = cutLastWord(currentPath, "\\Tools");
            currentPath += "\\ExcellaLite\\App_Data\\AppSettings\\ConnectionStrings.xml";
            return currentPath;
        }

        private void listAllMyDatabase(XmlDocument xdoc)
        {
            lstAllDB.Items.Clear();
            _connectionStrings.Clear();
            string machineName = "";
            if (cmbServers.SelectedIndex > 0)
            {
                machineName = cmbServers.SelectedIndex.ToString();
            }
            else
            {
                machineName = cmbServers.SelectedItem.ToString();
            }
            if (xdoc != null)
            {
                XmlNodeList xlist = xdoc.SelectNodes("AppSettings/Data[Type[. = 'ConnectionString']]");
                if (xlist.Count > 0)
                {
                    for (int i = 0; i < xlist.Count; i++)
                    {
                        string conName = "";
                        string conString = "";

                        XmlNode xname = xlist[i].SelectSingleNode("Name");
                        if (xname != null)
                        {
                            conName = xname.InnerText;
                        }
                        XmlNode xnode = xlist[i].SelectSingleNode(String.Format("Value[@WebServer='{0}']", machineName));
                        if (xnode != null)
                        {
                            XmlNode xvalue = xnode.Attributes["Value"];
                            if (xvalue != null)
                            {
                                conString = xvalue.Value;
                                if (conString.Length > 0)
                                {
                                    lstAllDB.Items.Add(conName);
                                    _connectionStrings.Add(conName, conString);
                                }
                            }
                        }
                    }
                }
            }
            if (lstAllDB.Items.Count > 0)
            {
                lstAllDB.SelectedIndex = 0;
            }
        }

        private void fillServers()
        {
            cmbServers.Items.Clear();
            cmbServers.Items.Add(Environment.MachineName.ToUpper());
            cmbServers.Items.Add("Localhost");
            cmbServers.Items.Add("Development");
            cmbServers.Items.Add("Staging");
            cmbServers.Items.Add("Production");
        }

        private void StoredProcedureGenerator_Load(object sender, EventArgs e)
        {
            fillServers();

            if (Settings.Default.Author.Length > 0)
            {
                txtAuthorName.Text = Settings.Default.Author;
            }

            if (cmbServers.Items.Count > 0)
            {
                cmbServers.SelectedIndex = 0;
            }
            radSet.Checked = true;
            radCreate.Checked = true;
        }

        private void cmdGenerateSP_Click(object sender, EventArgs e)
        {
            if ((lstTables.SelectedIndex > -1) && (lstAllDB.SelectedIndex > -1))
            {

                string conName = lstAllDB.SelectedItem.ToString();
                if (_connectionStrings.ContainsKey(conName))
                {
                    string conString = _connectionStrings[conName].ToString();
                    string completeName = lstTables.SelectedItem.ToString();
                    txtObject.Text = "";
                    // 3045 Script
                    //for (int i = 0; i < tableNames.Length; i++)
                    //{
                    //    completeName = "CAPC." + tableNames[i];
                        string tableName = "";
                        string schemaName = "";
                        int t1 = completeName.IndexOf('.');
                        if (t1 > -1)
                        {
                            tableName = completeName.Substring(t1 + 1);
                            schemaName = completeName.Substring(0, t1);
                        }
                        else
                        {
                            tableName = completeName;
                        }
                        List<string> columns = SQLData.getColumnsFromTable(conString, schemaName, tableName);
                        switch (comboRadio1Value)
                        {
                            case "3045update":
                            case "3045get":
                            case "3045set":
                            case "3045updateone":
                                txtObject.Text += buildStoredProcedureObject3045(schemaName, tableName, columns);
                                // 3405 Script for all:
                                //txtObject.Text += "\r\nGO\r\n\r\n";
                                break;
                            case "insertdata":
                                txtObject.Text += SQLData.getInsertScriptForTable(conString, schemaName, tableName);
                                break;
                            default:
                                txtObject.Text = buildStoredProcedureObject(schemaName, tableName, columns);
                                break;
                        }
                    //}
                }
            }
        }

        private string buildStoredProcedureObject3045(string schemaName, string tableName, List<string> columns)
        {
            bool isAssessmentIDThere = false;
            string strException = "@InsertDate, @UpdateDate, @AssessmentID,";
            string strText = "";
            string strSQL1 = "";
            string strSQL2 = "";

            string strSQL3 = "";
            string strSQL4 = "";
            string strParamaters1 = "";

            string strParamaters = "";
            string strIdentityColumn = "";
            string strIdentityDataType = "";
            string strIdentityDefValue = "";
            string strOrgDeclares = "";
            string strOrgSelect = "";
            string strOrgReplacement = "";
            string cNameForNoID = "";
            string cTypeForNoID = "";
            string defValueForNoID = "";
            for (int i = 0; i < columns.Count; i++)
            {
                // index: 0 = column name, 1: data type, 2: length, 3: is identity
                string[] s = columns[i].Split(',');
                string cName = s[0].Trim();
                string cType = s[1].Trim();
                int cLength = int.Parse(s[2].Trim());
                bool isIdentity = bool.Parse(s[3].Trim());

                string dataType = "";
                string defValue = "";
                string sComma = "";
                switch (cType)
                {
                    case "int":
                        dataType = "int";
                        defValue = "0";
                        break;
                    case "bigint":
                        dataType = "bigint";
                        defValue = "0";
                        break;
                    case "datetime":
                        dataType = "datetime";
                        defValue = "NULL";
                        break;
                    case "date":
                        dataType = "date";
                        defValue = "NULL";
                        break;
                    case "time":
                        dataType = "time";
                        defValue = "NULL";
                        break;
                    case "nvarchar":
                        dataType = String.Format("nvarchar({0})", cLength / 2);
                        defValue = "''";
                        break;
                    case "varchar":
                        dataType = String.Format("varchar({0})", cLength);
                        defValue = "''";
                        break;
                    case "bit":
                        dataType = "bit";
                        defValue = "0";
                        break;
                    case "float":
                        dataType = "float";
                        defValue = "0";
                        break;
                    case "text":
                        dataType = String.Format("varchar(MAX)");
                        defValue = "''";
                        break;
                    case "ntext":
                        dataType = String.Format("nvarchar(MAX)");
                        defValue = "''";
                        break;
                    case "money":
                        dataType = "money";
                        defValue = "0";
                        break;
                    case "tinyint":
                        dataType = "tinyint";
                        defValue = "0";
                        break;
                    case "numeric":
                        dataType = "numeric";
                        defValue = "0";
                        break;
                    case "decimal":
                        dataType = "decimal (9,2)"; // assume clength = 92
                        defValue = "0";
                        break;
                }


                if (strParamaters.Length > 0)
                {
                    sComma = ",";
                }
                else
                {
                    sComma = " ";
                }

                if ((isIdentity) && (strIdentityColumn.Length == 0))
                {
                    strIdentityColumn = cName;
                    strIdentityDataType = cType;
                    strIdentityDefValue = defValue;
                }

                switch (comboRadio1Value)
                {
                    case "3045set":
                        if (!isIdentity)
                        {
                            if (strException.IndexOf("@" + cName + ",") == -1)
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, "NULL");
                                strSQL1 += String.Format("\t\t{0}{1}\r\n", sComma, cName);
                                strSQL2 += String.Format("\t\t{0}@{1}\r\n", sComma, cName);
                            }
                            else
                            {
                                string sComma2 = "";
                                if (strSQL1.Length > 0)
                                {
                                    sComma2 = ",";
                                }
                                else
                                {
                                    sComma2 = " ";
                                }
                                if ((cName == "InsertDate") || (cName == "UpdateDate"))
                                {
                                    strSQL1 += String.Format("\t\t{0}{1}\r\n", sComma2, cName);
                                    strSQL2 += String.Format("\t\t{0}@DNow\r\n", sComma2);
                                }
                                if (cName == "AssessmentID")
                                {
                                    isAssessmentIDThere = true;
                                    strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                                    strSQL1 += String.Format("\t\t{0}{1}\r\n", sComma2, cName);
                                    strSQL2 += String.Format("\t\t{0}@{1}\r\n", sComma2, cName);
                                }
                            }
                        }
                        break;
                    case "3045get":
                        if (isIdentity)
                        {
                            if (isHCBS())
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2}\r\n", sComma, cName, dataType);
                            }
                            else
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                            }
                        }
                        if (strSQL1.Length > 0)
                        {
                            sComma = ",";
                        }
                        else
                        {
                            sComma = " ";
                        }
                        strSQL1 += String.Format("\t\t{0}{1}\r\n", sComma, cName);
                        break;
                    case "3045update":

                        if (strException.IndexOf("@" + cName + ",") == -1)
                        {
                            if (isHCBS())
                            {
                                string sComma1 = "";
                                if (strOrgSelect.Length > 0)
                                {
                                    sComma1 = ",";
                                }
                                else
                                {
                                    sComma1 = " ";
                                }
                                // for 3045, default input values are NULLS
                                if (isIdentity)
                                {
                                    strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                                }
                                else
                                {
                                    strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, "NULL");
                                    strOrgDeclares += String.Format("\tDECLARE @Org_{0} {1}\r\n", cName, dataType);
                                    strOrgSelect += String.Format("\t\t{0}@Org_{1} = {1}\r\n", sComma1, cName);
                                    strOrgReplacement += String.Format("\tIF @{0} IS NOT NULL\r\n", cName);
                                    strOrgReplacement += String.Format("\tBEGIN\r\n");
                                    strOrgReplacement += String.Format("\t\tSET @Org_{0} = @{0}\r\n", cName);
                                    strOrgReplacement += String.Format("\tEND\r\n");
                                }
                            }
                            else
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                            }
                        }
                        if (!isIdentity)
                        {
                            if (strSQL1.Length > 0)
                            {
                                sComma = ",";
                            }
                            else
                            {
                                sComma = " ";
                            }
                            if (cName == "UpdateDate")
                            {
                                strSQL1 += String.Format("\t\t{0}{1} = @DNow\r\n", sComma, cName);
                            }
                            else
                            {
                                if (strException.IndexOf("@" + cName + ",") == -1)
                                {
                                    strSQL1 += String.Format("\t\t{0}{1} = @Org_{1}\r\n", sComma, cName);
                                }
                            }
                        }
                        break;
                    case "3045updateone":
                        #region SET
                        if (!isIdentity)
                        {
                            string sComma2 = "";
                            if (strSQL3.Length > 0)
                            {
                                sComma2 = ",";
                            }
                            else
                            {
                                sComma2 = " ";
                            }

                            if (strException.IndexOf("@" + cName + ",") == -1)
                            {
                                strParamaters1 += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma2, cName, dataType, "NULL");
                                strSQL3 += String.Format("\t\t{0}{1}\r\n", sComma2, cName);
                                strSQL4 += String.Format("\t\t{0}@{1}\r\n", sComma2, cName);
                            }
                            else
                            {

                                if ((cName == "InsertDate") || (cName == "UpdateDate"))
                                {
                                    strSQL3 += String.Format("\t\t{0}{1}\r\n", sComma2, cName);
                                    strSQL4 += String.Format("\t\t{0}@DNow\r\n", sComma2);
                                }
                                if (cName == "AssessmentID")
                                {
                                    isAssessmentIDThere = true;
                                    strParamaters1 += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                                    strSQL3 += String.Format("\t\t{0}{1}\r\n", sComma2, cName);
                                    strSQL4 += String.Format("\t\t{0}@{1}\r\n", sComma2, cName);
                                }
                            }
                        }
                        #endregion

                        #region UPDATE
                        if (strException.IndexOf("@" + cName + ",") == -1)
                        {
                            if (isHCBS())
                            {
                                string sComma1 = "";
                                if (strOrgSelect.Length > 0)
                                {
                                    sComma1 = ",";
                                }
                                else
                                {
                                    sComma1 = " ";
                                }
                                // for 3045, default input values are NULLS
                                if (isIdentity)
                                {
                                    strParamaters += String.Format("\t\t{0}@AssessmentID int = 0\r\n", sComma, cName, dataType, defValue);
                                }
                                else
                                {
                                    strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, "NULL");
                                    strOrgDeclares += String.Format("\t\tDECLARE @Org_{0} {1}\r\n", cName, dataType);
                                    strOrgSelect += String.Format("\t\t\t{0}@Org_{1} = {1}\r\n", sComma1, cName);
                                    strOrgReplacement += String.Format("\t\tIF @{0} IS NOT NULL\r\n", cName);
                                    strOrgReplacement += String.Format("\t\tBEGIN\r\n");
                                    strOrgReplacement += String.Format("\t\t\tSET @Org_{0} = @{0}\r\n", cName);
                                    strOrgReplacement += String.Format("\t\tEND\r\n");
                                }
                            }
                            else
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                            }
                        }
                        if (!isIdentity)
                        {
                            if (strSQL1.Length > 0)
                            {
                                sComma = ",";
                            }
                            else
                            {
                                sComma = " ";
                            }
                            if (cName == "UpdateDate")
                            {
                                strSQL1 += String.Format("\t\t{0}{1} = @DNow\r\n", sComma, cName);
                            }
                            else
                            {
                                if (strException.IndexOf("@" + cName + ",") == -1)
                                {
                                    strSQL1 += String.Format("\t\t{0}{1} = @Org_{1}\r\n", sComma, cName);
                                }
                            }
                        }
                        #endregion

                        break;
                }

                if (i == 0)
                {
                    cNameForNoID = cName;
                    cTypeForNoID = cType;
                    defValueForNoID = defValue;
                }

                //strText += ""; String.Format("\t\t\tpublic const string {0} = \"{1}\";\r\n", s[0], s[1]);
            }

            if ((strIdentityColumn.Length == 0) && (columns.Count > 0))
            {
                strIdentityColumn = cNameForNoID;
                strIdentityDataType = cTypeForNoID;
                strIdentityDefValue = defValueForNoID;
            }


            switch (comboRadio1Value)
            {
                case "3045set":
                    strText += "\r\n";
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tDECLARE @{0} {1}\r\n", strIdentityColumn, strIdentityDataType);
                        strText += String.Format("\tSET @{0} = {1}\r\n\r\n", strIdentityColumn, strIdentityDefValue);
                    }

                    if (isAssessmentIDThere)
                    {
                        strText += String.Format("\tIF @AssessmentID > 0 \r\n", strIdentityDataType);
                        strText += String.Format("\tBEGIN \r\n\r\n", strIdentityDataType);
                    }

                    strText += String.Format("\tDECLARE @DNow datetime \r\n");
                    strText += String.Format("\tSET @DNow = GETDATE() \r\n\r\n", strIdentityDefValue);

                    strText += String.Format("\tINSERT INTO {0}( \r\n", tableName);
                    strText += strSQL1;
                    strText += "\t\t)\r\n";
                    strText += "\tVALUES(\r\n";
                    strText += strSQL2;
                    strText += "\t\t)\r\n\r\n";
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tSET @{0} = SCOPE_IDENTITY()\r\n\r\n", strIdentityColumn);
                        strText += String.Format("\tSELECT @{0} AS {0}\r\n", strIdentityColumn);
                    }
                    if (isAssessmentIDThere)
                    {
                        strText += String.Format("\r\n\tEND \r\n", strIdentityDataType);
                    }
                    break;
                case "3045get":
                    strText += "\r\n";
                    strText += String.Format("\tSELECT \r\n", tableName);
                    strText += strSQL1;
                    strText += String.Format("\tFROM {0} WITH (NOLOCK)\r\n", tableName);
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tWHERE {0} = @{0}\r\n\r\n", strIdentityColumn);
                    }
                    break;
                case "3045update":
                    strText += "\r\n";
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tDECLARE @RowUpdated {0} \r\n", strIdentityDataType);
                        strText += String.Format("\tSET @RowUpdated = {0} \r\n\r\n", strIdentityDefValue);

                        strText += String.Format("\tDECLARE @DNow datetime \r\n");
                        strText += String.Format("\tSET @DNow = GETDATE() \r\n\r\n", strIdentityDefValue);

                        strText += String.Format("\t-- Declare Original Holder Variables \r\n");
                        strText += String.Format("{0}\r\n\r\n", strOrgDeclares);

                        strText += String.Format("\t-- Read Original Values \r\n");
                        strText += String.Format("\tSELECT \r\n");
                        strText += String.Format("{0}", strOrgSelect);
                        strText += String.Format("\tFROM {0}", tableName);
                        strText += String.Format("\tWHERE {0} = @{0}\r\n\r\n", strIdentityColumn);

                        strText += String.Format("\t-- Replace If there is input on it \r\n");
                        strText += String.Format("{0}\r\n\r\n", strOrgReplacement);

                        strText += String.Format("\t-- Now Update Them \r\n");
                        strText += String.Format("\tUPDATE {0} SET \r\n", tableName);
                        strText += strSQL1;
                        strText += String.Format("\tWHERE {0} = @{0}\r\n\r\n", strIdentityColumn);

                        strText += String.Format("\tSET @RowUpdated = @@ROWCOUNT\r\n\r\n");
                        strText += String.Format("\tSELECT @RowUpdated AS RowUpdated");
                    }
                    break;
                case "3045updateone":
                    strText += "\r\n";
                    if (strIdentityColumn.Length > 0)
                    {
	
                        strText += String.Format("\tDECLARE @isNewID bit \r\n");
                        strText += String.Format("\tSET @isNewID = 0 \r\n\r\n");

                        strText += String.Format("\tDECLARE @DNow datetime \r\n");
                        strText += String.Format("\tSET @DNow = GETDATE() \r\n\r\n");

                        strText += String.Format("\tDECLARE @{0} {1} \r\n", strIdentityColumn, strIdentityDataType);
                        strText += String.Format("\tSET @{0} = {1} \r\n\r\n", strIdentityColumn, strIdentityDefValue);

                        strText += String.Format("\tSELECT @{0} = {0} \r\n", strIdentityColumn);
                        strText += String.Format("\tFROM {0} WHERE AssessmentID = @AssessmentID \r\n\r\n", tableName);

                        strText += String.Format("\t-- If exists update current information \r\n");
                        strText += String.Format("\tIF @{0}  > 0 \r\n", strIdentityColumn);
                        strText += String.Format("\tBEGIN\r\n\r\n");
	

                        strText += String.Format("\t\t-- Declare Original Holder Variables \r\n");
                        strText += String.Format("{0}\r\n\r\n", strOrgDeclares);

                        strText += String.Format("\t\t-- Read Original Values \r\n");
                        strText += String.Format("\t\tSELECT \r\n");
                        strText += String.Format("{0}", strOrgSelect);
                        strText += String.Format("\t\tFROM {0}", tableName);
                        strText += String.Format("\t\tWHERE {0} = @{0}\r\n\r\n", strIdentityColumn);

                        strText += String.Format("\t\t-- Replace If there is input on it \r\n");
                        strText += String.Format("{0}\r\n\r\n", strOrgReplacement);

                        strText += String.Format("\t\t-- Now Update Them \r\n");
                        strText += String.Format("\t\tUPDATE {0} SET \r\n", tableName);
                        strText += strSQL1;
                        strText += String.Format("\t\tWHERE {0} = @{0}\r\n\r\n", strIdentityColumn);

                        strText += String.Format("\tEND ELSE\r\n");
                        strText += String.Format("\t-- insert new information\r\n");
                        strText += String.Format("\tBEGIN\r\n\r\n");

                        strText += String.Format("\t\tINSERT INTO {0}( \r\n", tableName);
                        strText += strSQL3;
                        strText += "\t\t)\r\n";
                        strText += "\t\tVALUES(\r\n";
                        strText += strSQL4;
                        strText += "\t\t)\r\n\r\n";
                        if (strIdentityColumn.Length > 0)
                        {
                            strText += String.Format("\t\tSET @{0} = SCOPE_IDENTITY()\r\n", strIdentityColumn);
                            strText += String.Format("\t\tSET @isNewID = 1\r\n", strIdentityColumn);
                        }

                        strText += String.Format("\tEND\r\n\r\n");

                        strText += String.Format("\tSELECT @{0} AS {0}\r\n", strIdentityColumn);
                        strText += String.Format("\t\t  ,@isNewID AS isNewID\r\n\r\n");

                    }
                    break;
            }

            string spName = "";
            if (isHCBS())
            {
                switch (comboRadio1Value)
                {
                    case "3045set":
                        spName = String.Format("sp_{0}_Insert", tableName);
                        break;
                    case "3045update":
                        spName = String.Format("sp_{0}_Update", tableName);
                        break;
                    case "3045get":
                        spName = String.Format("sp_{0}_GetByID", tableName);
                        break;
                    case "3045updateone":
                        spName = String.Format("sp_{0}_UpdateOne", tableName);
                        break;
                }
            }
            else
            {
                spName = comboRadio1Value + tableName;
            }

            return String.Format(txtSPTemplate.Text
                , txtAuthorName.Text
                , DateTime.Now.ToString()
                , txtDecsription.Text
                , comboRadio2Value
                , schemaName
                , spName
                , strParamaters
                , strText
                );
        }


        private string buildStoredProcedureObject(string schemaName, string tableName, List<string> columns)
        {
            string strText = "";
            string strSQL1 = "";
            string strSQL2 = "";
            string strParamaters = "";
            string strIdentityColumn = "";
            string strIdentityDataType = "";
            string strIdentityDefValue = "";
            string cNameForNoID = "";
            string cTypeForNoID = "";
            string defValueForNoID = "";
            for (int i = 0; i < columns.Count; i++)
            {
                // index: 0 = column name, 1: data type, 2: length, 3: is identity
                string[] s = columns[i].Split(',');
                string cName = s[0].Trim();
                string cType = s[1].Trim();
                int cLength = int.Parse(s[2].Trim());
                bool isIdentity = bool.Parse(s[3].Trim());

                string dataType = "";
                string defValue = "";
                string sComma = "";
                switch (cType)
                {
                    case "int":
                        dataType = "int";
                        defValue = "0";
                        break;
                    case "bigint":
                        dataType = "bigint";
                        defValue = "0";
                        break;
                    case "datetime":
                        dataType = "datetime";
                        defValue = "NULL";
                        break;
                    case "date":
                        dataType = "date";
                        defValue = "NULL";
                        break;
                    case "time":
                        dataType = "time";
                        defValue = "NULL";
                        break;
                    case "nvarchar":
                        dataType = String.Format("nvarchar({0})", cLength / 2);
                        defValue = "''";
                        break;
                    case "varchar":
                        dataType = String.Format("varchar({0})", cLength);
                        defValue = "''";
                        break;
                    case "bit":
                        dataType = "bit";
                        defValue = "0";
                        break;
                    case "float":
                        dataType = "float";
                        defValue = "0";
                        break;
                    case "text":
                        dataType = "text";
                        defValue = "''";
                        break;
                    case "ntext":
                        dataType = "ntext";
                        defValue = "''";
                        break;
                    case "money":
                        dataType = "money";
                        defValue = "0";
                        break;
                    case "tinyint":
                        dataType = "tinyint";
                        defValue = "0";
                        break;
                    case "numeric":
                        dataType = "numeric";
                        defValue = "0";
                        break;
                }

                if (strParamaters.Length > 0)
                {
                    sComma = ",";
                }
                else
                {
                    sComma = " ";
                }

                if ((isIdentity) && (strIdentityColumn.Length == 0))
                {
                    strIdentityColumn = cName;
                    strIdentityDataType = cType;
                    strIdentityDefValue = defValue;
                }

                switch (comboRadio1Value)
                {
                    case "set":
                        if (!isIdentity)
                        {
                            if (isHCBS())
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2}\r\n", sComma, cName, dataType);
                            }
                            else
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                            }
                            strSQL1 += String.Format("\t\t{0}{1}\r\n", sComma, cName);
                            strSQL2 += String.Format("\t\t{0}@{1}\r\n", sComma, cName);
                        }
                        break;
                    case "get":
                        if (isIdentity)
                        {
                            if (isHCBS())
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2}\r\n", sComma, cName, dataType);
                            }
                            else
                            {
                                strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                            }
                        }
                        if (strSQL1.Length > 0)
                        {
                            sComma = ",";
                        }
                        else
                        {
                            sComma = " ";
                        }
                        strSQL1 += String.Format("\t\t{0}{1}\r\n", sComma, cName);
                        break;
                    case "update":
                        if (isHCBS())
                        {
                            strParamaters += String.Format("\t\t{0}@{1} {2}\r\n", sComma, cName, dataType);
                        }
                        else
                        {
                            strParamaters += String.Format("\t\t{0}@{1} {2} = {3}\r\n", sComma, cName, dataType, defValue);
                        }
                        if (!isIdentity)
                        {
                            if (strSQL1.Length > 0)
                            {
                                sComma = ",";
                            }
                            else
                            {
                                sComma = " ";
                            }
                            strSQL1 += String.Format("\t\t{0}{1} = @{1}\r\n", sComma, cName);
                        }
                        break;
                }

                if (i == 0)
                {
                    cNameForNoID = cName;
                    cTypeForNoID = cType;
                    defValueForNoID = defValue;
                }

                //strText += ""; String.Format("\t\t\tpublic const string {0} = \"{1}\";\r\n", s[0], s[1]);
            }

            if ((strIdentityColumn.Length == 0) && (columns.Count > 0))
            {
                strIdentityColumn = cNameForNoID;
                strIdentityDataType = cTypeForNoID;
                strIdentityDefValue = defValueForNoID;
            }


            switch (comboRadio1Value)
            {
                case "set":
                    strText += "\r\n";
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tDECLARE @{0} {1}\r\n", strIdentityColumn, strIdentityDataType);
                        strText += String.Format("\tSET @{0} = {1}\r\n\r\n", strIdentityColumn, strIdentityDefValue);
                    }
                    strText += String.Format("\tINSERT INTO {0}( \r\n", tableName);
                    strText += strSQL1;
                    strText += "\t\t)\r\n";
                    strText += "\tVALUES(\r\n";
                    strText += strSQL2;
                    strText += "\t\t)\r\n\r\n";
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tSET @{0} = SCOPE_IDENTITY()\r\n\r\n", strIdentityColumn);
                        strText += String.Format("\tSELECT @{0} AS {0}", strIdentityColumn);
                    }
                    break;
                case "get":
                    strText += "\r\n";
                    strText += String.Format("\tSELECT \r\n", tableName);
                    strText += strSQL1;
                    strText += String.Format("\tFROM {0} WITH (NOLOCK)\r\n", tableName);
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tWHERE {0} = @{0}\r\n\r\n", strIdentityColumn);
                    }
                    break;
                case "update":
                    strText += "\r\n";
                    if (strIdentityColumn.Length > 0)
                    {
                        strText += String.Format("\tDECLARE @RowUpdated {0} \r\n", strIdentityDataType);
                        strText += String.Format("\tSET @RowUpdated = {0} \r\n\r\n", strIdentityDefValue);

                        strText += String.Format("\tUPDATE {0} SET \r\n", tableName);
                        strText += strSQL1;
                        strText += String.Format("\tWHERE {0} = @{0}\r\n\r\n", strIdentityColumn);

                        strText += String.Format("\tSET @RowUpdated = @@ROWCOUNT\r\n\r\n");
                        strText += String.Format("\tSELECT @RowUpdated AS RowUpdated");
                    }
                    break;
            }

            string spName = "";
            if (isHCBS())
            {
                switch (comboRadio1Value)
                {
                    case "set":
                        spName = String.Format("sp_{0}_Insert", tableName);
                        break;
                    case "update":
                        spName = String.Format("sp_{0}_Update", tableName);
                        break;
                    case "get":
                        spName = String.Format("sp_{0}_GetByID", tableName);
                        break;
                }
            }
            else
            {
                spName = comboRadio1Value + tableName;
            }

            return String.Format(txtSPTemplate.Text
                , txtAuthorName.Text
                , DateTime.Now.ToString()
                , txtDecsription.Text
                , comboRadio2Value
                , schemaName
                , spName 
                , strParamaters
                , strText
                );
        }

        private void cmdCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtObject.Text);
        }

        private void cmdExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmbServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            string currentConFile = getConnectionSettingsFilePath();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(currentConFile);
            listAllMyDatabase(xdoc);
            if (lstAllDB.Items.Count == 1)
            {
                cmdListAllTables_Click(sender, e);
            }
            refreshConnectionStringText();
        }

        private void refreshConnectionStringText()
        {
            txtConnectionString.Text = "";
            if (lstAllDB.SelectedItem != null)
            {
                string conName = lstAllDB.SelectedItem.ToString();
                if (_connectionStrings.ContainsKey(conName))
                {
                    txtConnectionString.Text = _connectionStrings[conName].ToString();
                }
            }
        }

        private void radSet_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio1Value = "set";
            }
        }

        private void radGet_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio1Value = "get";
            }
        }

        private void radUpdate_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio1Value = "update";
            }
        }

        private void rad3045Update_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio1Value = "3045update";
            }
        }

        private void radCreate_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio2Value = "CREATE";
            }
        }

        private void radAlter_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio2Value = "ALTER";
            }
        }

        private void cmdCopyToClipboard_Click_1(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtObject.Text);
        }

        private void lstAllDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshConnectionStringText();
        }

        private void txtAuthorName_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.Author = txtAuthorName.Text;
            Settings.Default.Save();
        }

        private void lstAllDB_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (isHCBS())
            {
                txtSPTemplate.Text = txtSPTemplateHCBS.Text;
            }
            else
            {
                txtSPTemplate.Text = txtSPTemplateNonHCBS.Text;
            }
        }

        private void rad3045Set_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio1Value = "3045set";
            }
        }

        private void rad3045UpdateOne_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio1Value = "3045updateone";
            }
        }

        private void radInsertData_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if ((rb != null) && (rb.Checked))
            {
                comboRadio1Value = "insertdata";
            }
        }




    }
}
