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
using System.Data.Sql;
using System.Data.SqlClient;
using ssch.tools.Properties;

namespace ssch.tools
{
    public partial class DataReaderGenerator : Form
    {
        private Hashtable _connectionStrings = new Hashtable();
        private string strParameters = "";
        private string strAddParameters = "";

        public DataReaderGenerator()
        {
            InitializeComponent();
        }

        private void cmdListAllSPs_Click(object sender, EventArgs e)
        {
            if (lstAllDB.SelectedIndex > -1)
            {
                lstSPs.Items.Clear();
                string conName = lstAllDB.SelectedItem.ToString();
                if (_connectionStrings.ContainsKey(conName))
                {
                    string conString = _connectionStrings[conName].ToString();
                    if (conString.Length > 0)
                    {
                        DRspStoredProcedures_GetAll sps = SQLData.spStoredProcedures_GetAll(conString);
                        for (int i = 0; i < sps.Count; i++)
                        {
                            lstSPs.Items.Add(sps.ROUTINE_SCHEMA(i) + '.' + sps.ROUTINE_NAME(i));
                        }
                        if (lstSPs.Items.Count > 0)
                        {
                            lstSPs.SelectedIndex = 0;
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

        private void DataReaderGenerator_Load(object sender, EventArgs e)
        {
            if (Settings.Default.LocalDRPath.Length > 0)
            {
                txtSaveTo.Text = Settings.Default.LocalDRPath;
            }

            fillServers();
            if (cmbServers.Items.Count > 0)
            {
                cmbServers.SelectedIndex = 0;
            }


        }

        private void cmdGenerateObject_Click(object sender, EventArgs e)
        {
            
            if ((lstSPs.SelectedIndex > -1) && (lstAllDB.SelectedIndex > -1))
            {

                string conName = lstAllDB.SelectedItem.ToString();
                if (_connectionStrings.ContainsKey(conName))
                {
                    string conString = _connectionStrings[conName].ToString();
                    string completeName = lstSPs.SelectedItem.ToString();
                    string spName = "";
                    string schemaName = "";
                    int t1 = completeName.IndexOf('.');
                    if (t1 > -1)
                    {
                        spName = completeName.Substring(t1 + 1);
                        schemaName = completeName.Substring(0, t1);
                    }
                    
                    txtObjectCaller.Text = "";
                    List<string> parameters = SQLData.getParametersFromSP(conString, schemaName, spName);
                    List<SqlParameter> spParams = new List<SqlParameter>();
                    
                    List<string> columns = SQLData.getColumnsFromSP(conString, schemaName, spName, spParams);

                        txtObjectCaller.Text = buildCSharpCallerObject(schemaName, spName, parameters);
                        txtObjectModel.Text = buildCSharpObjectModel(spName, columns);
                        txtObject.Text = buildCSharpObject(schemaName, spName, columns);

                        if (chkGenerateConstant.Checked)
                        {
                            txtConstantLine.Text = buildCSharpConstantLine(schemaName, spName);
                        }
                        else
                        {
                            txtConstantLine.Text = "";
                        }

                }
            }
        }

        private string buildCSharpObjectModel(string spName, List<string> columns)
        {
            string strText = "";

            strText += "using System;\r\n";
            strText += "using System.Collections.Generic;\r\n";
            strText += "using System.Data.SqlTypes;\r\n";
            strText += "using System.Linq;\r\n";
            strText += "using System.Web;\r\n\r\n";

            strText += "namespace EBPP.WServices.Model\r\n";
            strText += "{\r\n";
            strText += "    public class " + spName + "\r\n";
            strText += "    {\r\n";

            if (columns.Count > 0)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    string[] s = columns[i].Split(',');
                    string varName = s[0].Trim();
                    string dataType = s[1].ToLower();

                    if (varName.StartsWith("@"))
                    {
                        varName = varName.Substring(1);
                    }

                    switch (dataType)
                    {
                        case "string":
                            strText += "\t\tpublic string " + varName + " {get; set;}";
                            break;
                        case "int32":
                            strText += "\t\tpublic int " + varName + " {get; set;}";
                            break;
                        case "int64":
                            strText += "\t\tpublic Int64 " + varName + " {get; set;}";
                            break;
                        case "datetime":
                            strText += "\t\tpublic DateTime " + varName + " {get; set;}";
                            break;
                        case "boolean":
                            strText += "\t\tpublic bool " + varName + " {get; set;}";
                            break;
                        case "double":
                            strText += "\t\tpublic double " + varName + " {get; set;}";
                            break;
                        case "byte":
                            strText += "\t\tpublic byte " + varName + " {get; set;}";
                            break;
                        case "decimal":
                            strText += "\t\tpublic decimal " + varName + " {get; set;}";
                            break;
                        case "guid":
                            strText += "\t\tpublic SqlGuid " + varName + " {get; set;}";
                            break;
                    }


                    strText += "\r\n";
                }
            }




            strText += "    }\r\n";
            strText += "}\r\n";

            return strText;
        }

        private string buildCSharpConstantLine(string schemaName, string spName)
        {
            string completeName = "";
            if (schemaName.ToLower() == "dbo")
            {
                completeName = spName;
            }
            else
            {
                completeName = String.Format("[{0}].{1}", schemaName, spName);
            }
            return String.Format("public const string {0} = \"{1}\";", spName, completeName);
        }

        private string buildLoadDataMethod(string schemaName, string spName, List<string> columns)
        {
            string strText = "";
            strText += String.Format("\t\tpublic void LoadData({0})\r\n", strParameters);
            strText += "\t\t{\r\n";
            strText += String.Format("\t\t\tDataAccess DB = new DataAccess({0});\r\n", txtDBConstructor.Text);
            strText += "\t\t\tSqlCommand command = new SqlCommand(" + String.Format(txtCommandConstructor.Text, (schemaName.ToLower() == "dbo" ? spName : schemaName + '.' + spName)) + ");\r\n";
            strText += "\t\t\tcommand.CommandType = CommandType.StoredProcedure;\r\n";
            strText += strAddParameters.Replace("\t\t","\t\t\t");
            strText += "\t\t\tDataSet aps = DB.ExecuteDataSet(command);\r\n";
            strText += "\t\t\tbase.setData(aps);\r\n";
            strText += "\t\t}\r\n\r\n";
            return strText;
        }

        private string buildCSharpCallerObjectHCBS(
            string schemaName, 
            string spName, 
            List<string> parameters, 
            List<string> columns)
        {
            string strText = "";
            strText += "\t\tpublic class " + spName + "Controller : DataProviderBase\r\n";
            strText += "\t\t{\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#region fields\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#endregion\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#region properties\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#endregion\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#region constructor\r\n";
            strText += "\t\t\r\n";
            strText += "\t\tpublic " + spName + "Controller()\r\n";
            strText += "\t\t{\r\n";
            strText += "\t\t}\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#endregion\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#region get methods\r\n";
            strText += "\t\t\r\n";
            strText += "\t\tpublic List<" + spName + "> Get" + spName + "List()\r\n";
            strText += "\t\t{\r\n";
            strText += "\t\tList<" + spName + "> listReturn = new List<" + spName + ">();\r\n";
            strText += "\t\ttry\r\n";
            strText += "\t\t{\r\n";
            strText += "\t\tbase._connection.Open();\r\n";

            if (parameters.Count > 0)
            {
                strText += "\t\tList<SqlParameter> spParams = new List<SqlParameter>();\r\n";
                for (int i = 0; i < parameters.Count; i++)
                {
                    string[] s = parameters[i].Split(',');
                    string varName = s[0].Trim();
                    string dataType = s[1].ToLower();

                    switch (dataType)
                    {
                        case "varchar":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", \"\"));\r\n";
                            break;
                        case "nvarchar":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", \"\"));\r\n";
                            break;
                        case "int":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", 0));\r\n";
                            break;
                        case "bigint":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", 0));\r\n";
                            break;
                        case "time":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", DateTime.Now));\r\n";
                            break;
                        case "datetime":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", DateTime.Now));\r\n";
                            break;
                        case "date":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", DateTime.Now));\r\n";
                            break;
                        case "bit":
                            strText += "\t\tspParmas.Add(new SqlParameter(\"" + varName + "\", 0));\r\n";
                            break;
                        case "boolean":
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", 0));\r\n";
                            break;
                        default:
                            strText += "\t\tspParams.Add(new SqlParameter(\"" + varName + "\", null));\r\n";
                            break;

                    }
                    
                }

            }

            if (parameters.Count > 0)
            {
                strText += "\t\tusing (DataTable dt = GetDataTable(StoredProcedure." + spName + ", spParams))\r\n";
            }
            else
            {
                strText += "\t\tusing (DataTable dt = GetDataTable(StoredProcedure." + spName + "))\r\n";
            }


            strText += "\t\t{\r\n";
            strText += "\t\tforeach (DataRow row in dt.Rows)\r\n";
            strText += "\t\t{\r\n";
            strText += "\t\t" + spName + " lookup = new " + spName + "()\r\n";
            strText += "\t\t{\r\n";



            if (columns.Count > 0)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    string[] s = columns[i].Split(',');
                    string varName = s[0].Trim();
                    string dataType = s[1].ToLower();

                    if (varName.StartsWith("@"))
                    {
                        varName = varName.Substring(1);
                    }

                    switch (dataType)
                    {
                        case "string":
                            strText += "\t\t" + varName + " = base.getValue(row, \"" + varName + "\")";
                            break;
                        case "int32":
                            strText += "\t\t" + varName + " = base.getValueInteger(row, \"" + varName + "\")";
                            break;
                        case "int64":
                            strText += "\t\t" + varName + " = base.getValueInt64(row, \"" + varName + "\")";
                            break;
                        case "datetime":
                            strText += "\t\t" + varName + " = base.getValueDateTime(row, \"" + varName + "\")";
                            break;
                        case "boolean":
                            strText += "\t\t" + varName + " = base.getValueBoolean(row, \"" + varName + "\")";
                            break;
                        case "double":
                            strText += "\t\t" + varName + " = base.getValueDouble(row, \"" + varName + "\")";
                            break;
                        case "byte":
                            strText += "\t\t" + varName + " = base.getValueByte(row, \"" + varName + "\")";
                            break;
                        case "decimal":
                            strText += "\t\t" + varName + " = base.getValueDecimal(row, \"" + varName + "\")";
                            break;
                    }

                    if (i < columns.Count - 1)
                    {
                        strText += ",\r\n";
                    }
                    else
                    {
                        strText += "\r\n";
                    }
                }
            }

            strText += "\t\t};\r\n";

            strText += "\t\tlistReturn.Add(lookup);\r\n";
            strText += "\t\t}\r\n";
            strText += "\t\t}\r\n";
            strText += "\t\t}\r\n";
            strText += "\t\tcatch (SqlException ex)\r\n";
            strText += "\t\t{\r\n";
            strText += "\t\tErrorManager.logError(ex);\r\n";
            strText += "\t\t}\r\n";
            strText += "\t\tfinally\r\n";
            strText += "\t\t{\r\n";
            strText += "\t\tbase._connection.Close();\r\n";
            strText += "\t\t}\r\n";
            strText += "\t\treturn listReturn;\r\n";
            strText += "\t\t}\r\n";
            strText += "\t\t\r\n";
            strText += "\t\t#endregion\r\n";
            strText += "\t\t}\r\n";

            return strText;
        }

        private string buildCSharpCallerObject(string schemaName, string spName, List<string> parameters)
        {
            string strText = "";
            strParameters = "";
            strAddParameters = "";
            for (int i = 0; i < parameters.Count; i++)
            {
                string[] s = parameters[i].Split(',');
                string varName = s[0].Trim();
                if (varName.StartsWith("@"))
                {
                    varName = varName.Substring(1);
                }
                string cvarName = varName;
                string dataType = s[1].ToLower();

                string varType = "";
                string sqlVarType = "";

                switch (dataType)
                {
                    case "nvarchar":
                        varType = "string";
                        sqlVarType = "NVarChar";
                        break;
                    case "varchar":
                        varType = "string";
                        sqlVarType = "VarChar";
                        break;
                    case "bit":
                        varType = "bool";
                        sqlVarType = "Bit";
                        break;
                    case "int":
                        varType = "int";
                        sqlVarType = "Int";
                        break;
                    case "bigint":
                        varType = "Int64";
                        sqlVarType = "BigInt";
                        break;
                    case "time":
                        varType = "TimeSpan";
                        if (cvarName == "TimeSpan")
                        {
                            cvarName = "tsTimeSpan";
                        }
                        sqlVarType = "Time";
                        break;
                    case "datetime":
                        varType = "DateTime";
                        if (cvarName == "DateTime")
                        {
                            cvarName = "dtDateTime";
                        }
                        sqlVarType = "DateTime";
                        break;
                    case "date":
                        varType = "DateTime";
                        if (cvarName == "DateTime")
                        {
                            cvarName = "dtDateTime";
                        }
                        sqlVarType = "Date";
                        break;
                }

                if (strParameters.Length > 0)
                {
                    strParameters += ",";
                }
                strParameters += String.Format("{0} {1}", varType, cvarName);
                strAddParameters += String.Format("\t\tcommand.Parameters.Add(\"@{0}\", SqlDbType.{1}).Value = {2};\r\n", varName, sqlVarType, cvarName);
            }

            strText += String.Format("\tpublic static {0}{1} {1}({2})\r\n", txtPrefix.Text, spName, strParameters);
            strText += "\t{\r\n";
            strText += String.Format("\t\tDataAccess DB = new DataAccess({0});\r\n", txtDBConstructor.Text);
            strText += "\t\tSqlCommand command = new SqlCommand(" + String.Format(txtCommandConstructor.Text, (schemaName.ToLower() == "dbo" ? spName : schemaName + '.' + spName)) + ");\r\n";
            strText += "\t\tcommand.CommandType = CommandType.StoredProcedure;\r\n";
            strText += strAddParameters;
            strText += "\t\tDataSet aps = DB.ExecuteDataSet(command);\r\n";
            strText += String.Format("\t\treturn new {0}{1}(aps);\r\n", txtPrefix.Text, spName);
            strText += "\t}\r\n";
            return strText;
        }

        private string buildCSharpObjectHCBS(string schemaName, string spName, List<string> columns)
        {
            string strText = String.Format("\tpublic class {0}\r\n", spName);
            strText += "\t{\r\n";
            strText += "\t\t#region fields\r\n";
            strText += "\r\n";

            for (int i = 0; i < columns.Count; i++)
            {
                string[] s = columns[i].Split(',');
                string varName = s[0].Trim();
                string dataType = s[1].ToLower();
                string varType = "";

                switch (dataType)
                {
                    case "string":
                        varType = "string";
                        break;
                    case "int32":
                        varType = "int";
                        break;
                    case "int64":
                        varType = "Int64";
                        break;
                    case "datetime":
                        varType = "DateTime";
                        break;
                    case "boolean":
                        varType = "bool";
                        break;
                    case "double":
                        varType = "double";
                        break;
                    case "byte":
                        varType = "byte";
                        break;
                    case "decimal":
                        varType = "decimal";
                        break;
                }
                strText += String.Format("\t\tpublic {0} {1} ", varType, varName) + "{ get; set; }\r\n";
            }

            strText += "\r\n";
            strText += "\t\t#endregion\r\n";
            strText += "\r\n";
            strText += "\t\t#region properties\r\n";
            strText += "\r\n";
            strText += "\t\t#endregion\r\n";
            strText += "\r\n";
            strText += "\t\t#region constructor\r\n";
            strText += "\r\n";
            strText += String.Format("\t\tpublic {0}()", spName) +  " { }\r\n";
            strText += "\r\n";
            strText += "\t\t#endregion\r\n";
            strText += "\r\n";
            strText += "\t}\r\n";
            return strText;
        }


        private string buildCSharpObject(string schemaName, string spName, List<string> columns)
        {
            string token1 = "REPORT_CLASS_TEXT";
            string strText = string.Empty;
            string reportClassText = string.Empty;
            string reportMethod = string.Empty;

            strText += txtReferences.Text + "\r\n\r\n";
            strText += String.Format("namespace {0} \r\n", txtNamespace.Text + (schemaName.ToLower() == "dbo" ? "" : "." + schemaName));
            strText += "{\r\n";
            txtSaveToFilename.Text = txtPrefix.Text + spName;
            strText += String.Format("\tpublic class {0} : TableReaderBase \r\n", txtSaveToFilename.Text);
            strText += "\t{\r\n";
            strText += "\t\tpublic class Columns \r\n\t\t{ \r\n";

            if (chkReportClass.Checked)
            {
                reportMethod += String.Format("\t\tpublic List<{0}.Report> GetReport({1})\r\n", txtPrefix.Text + spName, strParameters);
                reportMethod += "\t\t{\r\n";
                reportMethod += String.Format("\t\t\tList<{0}.Report> rpt = new List<{0}.Report>();", txtPrefix.Text + spName);
                reportMethod += "\r\n";

                reportMethod += "\t\t\tif (base.DataSource == null)\r\n";
                reportMethod += "\t\t\t{\r\n";
                string strPR = "";
                if (strParameters.Length > 0)
                {
                    string[] strParameter = strParameters.Split(',');
                    if (strParameter.Length > 0)
                    {
                        for (int i = 0; i < strParameter.Length; i++)
                        {
                            string strP = strParameter[i].Trim();
                            int t1 = strP.IndexOf(' ');
                            if (t1 > -1)
                            {
                                strP = strP.Substring(t1).Trim();
                            }
                            strPR += strP;
                            if (i < strParameter.Length - 1)
                            {
                                strPR += ',';
                            }
                        }
                    }
                }
                reportMethod += String.Format("\t\t\t\tLoadData({0});\r\n", strPR);
                reportMethod += "\t\t\t\tif (this.isError)\r\n";
                reportMethod += "\t\t\t\t{\r\n";
                reportMethod += "\t\t\t\t\tSystemException ex = new SystemException(this.ErrorMessage);\r\n";
                reportMethod += "\t\t\t\t\tthrow ex;\r\n";
                reportMethod += "\t\t\t\t}\r\n";
                reportMethod += "\t\t\t}\r\n";

                reportMethod += "\t\t\tfor (int i = 0; i < this.Count; i++)\r\n";
                reportMethod += "\t\t\t{\r\n";
                reportMethod += String.Format("\t\t\t\trpt.Add(new {0}.Report() ", txtPrefix.Text + spName);
                reportMethod += "{\r\n";
            }

            for (int i = 0; i < columns.Count; i++)
            {
                string[] s = columns[i].Split(',');
                strText += String.Format("\t\t\tpublic const string {0} = \"{0}\";\r\n", s[0]);
                if (chkReportClass.Checked)
                {
                    string cm = (i < columns.Count) ? "," : "";
                    reportMethod += String.Format("\t\t\t\t {0} = this.{0}(i) {1}\r\n", s[0], cm);
                }
            }

            strText += "\t\t}\r\n\r\n"; // private class Columns

            if (chkReportClass.Checked)
            {
                reportMethod += "\t\t\t\t});\r\n";
                reportMethod += "\t\t\t}\r\n";
                reportMethod += "\t\t\treturn rpt;\r\n";
                reportMethod += "\t\t}\r\n\r\n";


                strText += token1;
            }

            strText += String.Format("\t\tpublic {0}{1}(DataSet ds)\r\n", txtPrefix.Text, spName);
            strText += "\t\t{\r\n";
            strText += "\t\t\tbase.setData(ds);\r\n";
            strText += "\t\t}\r\n\r\n";

            if (chkReportClass.Checked)
            {
                strText += String.Format("\t\tpublic {0}{1}()\r\n", txtPrefix.Text, spName);
                strText += "\t\t{\r\n";
                strText += "\t\t}\r\n\r\n";

                reportClassText += "\t\tpublic class Report \r\n\t\t{ \r\n"; // private class Report
            }

            

            for (int i = 0; i < columns.Count; i++)
            {
                string[] s = columns[i].Split(',');
                string dataType = s[1].ToLower();
                string varType = "";
                string baseFunction = "";

                switch (dataType)
                {
                    case "guid":
                        varType = "SqlGuid";
                        baseFunction = "getSqlGuid";
                        break;
                    case "string":
                        varType = "string";
                        baseFunction = "getValue";
                        break;
                    case "int32":
                        varType = "int";
                        baseFunction = "getValueInteger";
                        break;
                    case "int64":
                        varType = "Int64";
                        baseFunction = "getValueInteger64";
                        break;
                    case "datetime":
                        varType = "DateTime";
                        baseFunction = "getValueDate";
                        break;
                    case "boolean":
                        varType = "bool";
                        baseFunction = "getValueBool";
                        break;
                    case "double":
                        varType = "double";
                        baseFunction = "getValueDouble";
                        break;
                    case "byte":
                        varType = "byte";
                        baseFunction = "getValueByte";
                        break;
                    case "decimal":
                        varType = "decimal";
                        baseFunction = "getValueDecimal";
                        break;
                }

                strText += String.Format("\t\tpublic {0} {1}(int index) \r\n", varType, s[0]);
                strText += "\t\t{\r\n";
                strText += String.Format("\t\t\treturn base.{0}(index, Columns.{1});\r\n", baseFunction, s[0]);
                strText += "\t\t}\r\n\r\n";

                if (chkReportClass.Checked)
                {
                    reportClassText += String.Format("\t\t\tpublic {0} {1}", varType, s[0]);
                    reportClassText += " { get; set; } \r\n";
                }
            }

            if (chkReportClass.Checked)
            {
                reportClassText += "\t\t}\r\n\r\n"; // private class Report
                strText += buildLoadDataMethod(schemaName, spName, columns);
                strText = strText.Replace(token1, reportClassText);
                strText += reportMethod; // adding to main text
            }

            strText += "\t}\r\n"; // public class

            strText += "}\r\n"; // namespace

            return strText;
        }

        private void cmdCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtObject.Text);
        }

        private void cmdExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdCopyToClipboardCaller_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtObjectCaller.Text);
        }

        private void cmdCopyToClipboardConstant_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtConstantLine.Text);
        }

        private void cmbServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            string currentConFile = getConnectionSettingsFilePath();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(currentConFile);
            listAllMyDatabase(xdoc);
            if (lstAllDB.Items.Count == 1)
            {
                cmdListAllSPs_Click(sender, e);
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

        private void lstAllDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshConnectionStringText();
        }

        private void cmdSaveToFile_Click(object sender, EventArgs e)
        {
            if ((txtObject.Text.Length > 0) && (txtSaveTo.Text.Length > 0) && txtSaveToFilename.Text.Length > 0)
            {
               StreamWriter sw = File.CreateText(String.Format(txtSaveTo.Text, txtSaveToFilename.Text));
               sw.Write(txtObject.Text);
               sw.Close();
            }
        }

        private void txtSaveTo_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.LocalDRPath = txtSaveTo.Text;
            Settings.Default.Save();
        }

        private void lstAllDB_SelectedIndexChanged_1(object sender, EventArgs e)
        {

            groupBox1.Visible = true;
            txtNamespace.Visible = true;
            txtPrefix.Visible = true;
            label2.Visible = true;
            label3.Visible = true;

            if (lstAllDB.SelectedItem != null)
            {
                string sv = lstAllDB.SelectedItem.ToString();
                switch (sv)
                {
                    case "DefaultDB":
                        txtNamespace.Text = "PaymentProcessor.Web.Applications";
                        txtSaveTo.Text = @"R:\Cath_Legacy\Auth.Net.WTE\PaymentProcessor.Web.Applications\structures\{0}.cs";
                        txtDBConstructor.Text = "databaseServer.DefaultDB";
                        txtCommandConstructor.Text = "SV.SP.{0}";
                        txtReferences.Text = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Text;\r\nusing System.Data;\r\nusing System.Data.Sql;\r\nusing System.Data.SqlClient;\r\nusing System.Data.SqlTypes;\r\n\r\n";
                        break;
                    case "EBPP":
                        txtNamespace.Text = "EBPP.WServices.Structures";
                        txtSaveTo.Text = @"R:\Cath_Legacy\ACS\EBPP.WServices\Structures\{0}.cs";
                        txtDBConstructor.Text = "";
                        txtCommandConstructor.Text = "SV.SP.{0}";
                        txtReferences.Text = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Text;\r\nusing System.Data;\r\nusing System.Data.Sql;\r\nusing System.Data.SqlClient;\r\nusing System.Data.SqlTypes;\r\nusing EBPP.WServices.Code;\r\n\r\n";
                        break;
                    case "EGivings":
                        txtNamespace.Text = "PaymentProcessor.Web.Applications";
                        txtSaveTo.Text = @"R:\Cath_Legacy\Auth.Net.WTE\PaymentProcessor.Web.Applications\structures\PayTrace\{0}.cs";
                        txtDBConstructor.Text = "databaseServer.EGivings";
                        txtCommandConstructor.Text = "SV.SP.EGivings.{0}";
                        txtReferences.Text = "using System;\r\nusing System.Collections.Generic;\r\nusing System.Text;\r\nusing System.Data;\r\nusing System.Data.Sql;\r\nusing System.Data.SqlClient;\r\nusing System.Data.SqlTypes;\r\n\r\n";
                        break;
                    default:
                        break;
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnCopyObjectModel_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtObjectModel.Text);
        }

        private void txtConnectionString_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
