using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

// add reference Microsoft.SqlServer.ConnectionInfo, 
// add reference Microsoft.SqlServer.Smo
// add reference Microsoft.SqlServer.Management.Sdk.Sfc
// all references are in References folder 

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Collections.Specialized;

namespace ssch.tools
{
    public static class SQLData
    {

        public static DRspStoredProcedures_GetAll spStoredProcedures_GetAll(string connectionString)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spStoredProcedures_GetAll);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspStoredProcedures_GetAll(aps);
        }

        public static DRspFunctions_GetAll spFunctions_GetAll(string connectionString)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spFunctions_GetAll);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspFunctions_GetAll(aps);
        }

        public static DRspTables_GetAll spTables_GetAll(string connectionString)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spTables_GetAll);
            command.CommandType = CommandType.StoredProcedure;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspTables_GetAll(aps);
        }

        public static DataSet getDataSet(string connectionString, string textCommand)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(textCommand);
            command.CommandType = CommandType.Text;
            return DB.ExecuteDataSet(command);
        }

        public static DRspScannedDocument_Insert spScannedDocument_Insert(string connectionString, string FullPath)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spScannedDocument_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@FullPath", SqlDbType.VarChar).Value = FullPath;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspScannedDocument_Insert(aps);
        }

        public static DRspScannedDocument_Update spScannedDocument_Update(string connectionString, int ScannedDocumentID, string FullPath, string PrefixFolder, string FirstName, string LastName, int RecipientID, string DestinationPath, string LastError, string MID)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spScannedDocument_Update);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ScannedDocumentID", SqlDbType.Int).Value = ScannedDocumentID;
            command.Parameters.Add("@FullPath", SqlDbType.VarChar).Value = FullPath;
            command.Parameters.Add("@PrefixFolder", SqlDbType.VarChar).Value = PrefixFolder;
            command.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = FirstName;
            command.Parameters.Add("@LastName", SqlDbType.VarChar).Value = LastName;
            command.Parameters.Add("@RecipientID", SqlDbType.Int).Value = RecipientID;
            command.Parameters.Add("@DestinationPath", SqlDbType.VarChar).Value = DestinationPath;
            command.Parameters.Add("@LastError", SqlDbType.VarChar).Value = LastError;
            command.Parameters.Add("@MID", SqlDbType.VarChar).Value = MID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspScannedDocument_Update(aps);
        }

        public static DRspScannedDocument_Get spScannedDocument_Get(string connectionString, bool IsDoneParse, bool IsDoneMove)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spScannedDocument_Get);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@IsDoneParse", SqlDbType.Bit).Value = IsDoneParse ? 1 : 0;
            command.Parameters.Add("@IsDoneMove", SqlDbType.Bit).Value = IsDoneMove ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspScannedDocument_Get(aps);
        }

        public static DRspScannedDocument_GetNoRecipient spScannedDocument_GetNoRecipient(string connectionString, bool IsDoneMove)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spScannedDocument_GetNoRecipient);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@IsDoneMove", SqlDbType.Bit).Value = IsDoneMove ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspScannedDocument_GetNoRecipient(aps);
        }

        public static DRspScannedDocument_GetRecipientID spScannedDocument_GetRecipientID(string connectionString, string FirstName, string LastName, string MID, bool IsPartialFirstName, bool IsPartialLastName)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spScannedDocument_GetRecipientID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = FirstName;
            command.Parameters.Add("@LastName", SqlDbType.VarChar).Value = LastName;
            command.Parameters.Add("@MID", SqlDbType.VarChar).Value = MID;
            command.Parameters.Add("@IsPartialFirstName", SqlDbType.Bit).Value = IsPartialFirstName ? 1 : 0;
            command.Parameters.Add("@IsPartialLastName", SqlDbType.Bit).Value = IsPartialLastName ? 1 : 0;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspScannedDocument_GetRecipientID(aps);
        }

        public static DRspRecipient_GetByID spRecipient_GetByID(string connectionString, int RecipientID)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spRecipient_GetByID);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@RecipientID", SqlDbType.Int).Value = RecipientID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspRecipient_GetByID(aps);
        }

        public static DRspIncomingDocument_Insert spIncomingDocument_Insert(string connectionString, int UserID, string SourcePath, string CurrentPath, int RecipientID, bool IsActivityLog, int MoveTypeID)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spIncomingDocument_Insert);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@SourcePath", SqlDbType.VarChar).Value = SourcePath;
            command.Parameters.Add("@CurrentPath", SqlDbType.VarChar).Value = CurrentPath;
            command.Parameters.Add("@IsActivityLog", SqlDbType.Bit).Value = IsActivityLog ? 1 : 0;
            command.Parameters.Add("@MoveTypeID", SqlDbType.Int).Value = MoveTypeID;
            command.Parameters.Add("@RecipientID", SqlDbType.Int).Value = RecipientID;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspIncomingDocument_Insert(aps);
        }

        public static DRspIncomingDocument_UpdateCurrentPath spIncomingDocument_UpdateCurrentPath(string connectionString, int UserID, Int64 IncomingDocumentID, string CurrentPath)
        {
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.spIncomingDocument_UpdateCurrentPath);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.Parameters.Add("@IncomingDocumentID", SqlDbType.BigInt).Value = IncomingDocumentID;
            command.Parameters.Add("@CurrentPath", SqlDbType.VarChar).Value = CurrentPath;
            DataSet aps = DB.ExecuteDataSet(command);
            return new DRspIncomingDocument_UpdateCurrentPath(aps);
        }

        public static List<string> getColumnsFromSP(string connectionString, string schemaName, string spName, List<SqlParameter> spParams)
        {
            List<string> columns = new List<string>();
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(String.Format("[{0}].[{1}]", schemaName, spName));
            command.CommandType = CommandType.StoredProcedure;

            foreach (SqlParameter param in spParams)
                command.Parameters.Add(param);
   
            DataSet aps = DB.ExecuteDataSet(command);
            int lt = aps.Tables.Count; 
            if (lt > 1)
            {
                DataTable dt = aps.Tables[lt - 2];
                if (dt != null)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        columns.Add(String.Format("{0},{1}", dt.Columns[i].ColumnName, dt.Columns[i].DataType.Name));
                    }
                }
            }
            return columns;
        }

        public static List<string> getParametersFromSP(string connectionString, string schemaName, string spName)
        {
            List<string> parameters = new List<string>();
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.sp_help);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@objname", SqlDbType.VarChar).Value = String.Format("[{0}].[{1}]", schemaName, spName);
            DataSet aps = DB.ExecuteDataSet(command);
            int lt = aps.Tables.Count;
            if (lt > 1)
            {
                DataTable dt = aps.Tables[lt - 1];
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        parameters.Add(String.Format("{0},{1}", dt.Rows[i]["Parameter_name"], dt.Rows[i]["Type"]));
                    }
                }
            }
            return parameters;
        }

        public static List<string> getColumnsFromTable(string connectionString, string schemaName, string tableName)
        {
            List<string> columns = new List<string>();
            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand(SV.SP.sp_help);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@objname", SqlDbType.VarChar).Value = schemaName + '.' + tableName;
            DataSet aps = DB.ExecuteDataSet(command);
            int lt = aps.Tables.Count;
            if (lt > 1)
            {
                DataTable dt = aps.Tables[1]; // table of columns
                DataTable di = aps.Tables[2]; // table identity
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string isIdentity = "false";
                        string columnName = dt.Rows[i]["Column_name"].ToString();;
                        for (int j = 0; j < di.Rows.Count; j++)
			            {
                            if (di.Rows[j]["Identity"].ToString() == columnName)
                            {
                                isIdentity = "true";
                                j = di.Rows.Count;
                            }
                        }
                        columns.Add(String.Format("{0},{1},{2},{3}"
                            , columnName
                            , dt.Rows[i]["Type"]
                            , dt.Rows[i]["Length"]
                            , isIdentity
                            ));
                    }
                }
            }
            return columns;
        }

        /// <summary>
        /// It will return script text (description). Object Type are 'function', 'proc', 'table', 'index'
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static string getScriptOfObject(string connectionString, string objectType, string objectName)
        {
            string strText = "";
            switch (objectType.ToLower())
            {
                case "function":
                case "proc":
                    DataAccess DB = new DataAccess(connectionString);
                    SqlCommand command = new SqlCommand(SV.SP.sp_helptext);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@objname", SqlDbType.VarChar).Value = objectName;
                    DataSet aps = DB.ExecuteDataSet(command);
                    int lt = aps.Tables.Count;
                    if (lt > 0)
                    {
                        DataTable dt = aps.Tables[0];
                        if (dt != null)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                strText += dt.Rows[i][0].ToString();
                            }
                        }
                    }
                    break;
                case "table":
                    strText = getScriptForTable(connectionString, objectName);
                    break;
                case "index":
                    strText = getScriptForIndex(connectionString, objectName);
                    break;

            }
            
            return strText;
        }

        /// <summary>
        /// To generate script for Table in Database, by given Table Name
        /// http://www.mssqltips.com/sqlservertip/1833/generate-scripts-for-database-objects-with-smo-for-sql-server/
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static string getScriptForTable(string connectionString, string tableName)
        {

            string strText = "";

            Server myServer = new Server(new ServerConnection(new SqlConnection(connectionString)));
            Scripter scripter = new Scripter(myServer);

            Database myDatabase = myServer.Databases[0];
            if (myDatabase != null)
            {
                /* With ScriptingOptions you can specify different scripting  
                * options, for example to include IF NOT EXISTS, DROP  
                * statements, output location etc*/
                ScriptingOptions scriptOptions = new ScriptingOptions();
                scriptOptions.ScriptDrops = true;
                scriptOptions.IncludeIfNotExists = true;
                Table myTable = myDatabase.Tables[tableName];

                StringCollection tableScripts = myTable.Script(scriptOptions);
                foreach (string script in tableScripts)
                {
                    strText += script + "\r\n\r\n";
                }

                strText += "\r\n";

                tableScripts = myTable.Script();
                foreach (string script in tableScripts)
                {
                    strText += script + "\r\n";
                }

            }

            return strText;
        }

        /// <summary>
        /// To generate script for Index in Database, by given Indexx Name
        ///  http://www.mssqltips.com/sqlservertip/1833/generate-scripts-for-database-objects-with-smo-for-sql-server/
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        private static string getScriptForIndex(string connectionString, string indexName)
        {

            string strText = "";

            Server myServer = new Server(new ServerConnection(new SqlConnection(connectionString)));
            Scripter scripter = new Scripter(myServer);

            Database myDatabase = myServer.Databases[0];
            if (myDatabase != null)
            {
                /* With ScriptingOptions you can specify different scripting  
                * options, for example to include IF NOT EXISTS, DROP  
                * statements, output location etc*/
                ScriptingOptions scriptOptions = new ScriptingOptions();
                scriptOptions.ScriptDrops = false;
                scriptOptions.IncludeIfNotExists = true;

                Index indexNeeded = null;
                foreach (Table myTable in myDatabase.Tables)
                {
                    if (myTable.Indexes.Contains(indexName))
                    {
                        indexNeeded = myTable.Indexes[indexName];
                        break;
                    }
                }

                if (indexNeeded != null)
                {
                    StringCollection tableScripts = indexNeeded.Script(scriptOptions);
                    foreach (string script in tableScripts)
                    {
                        strText += script + "\r\n\r\n";
                    }

                    strText += "\r\n";

                    //tableScripts = indexNeeded.Script();
                    //foreach (string script in tableScripts)
                    //{
                    //    strText += script + "\r\n";
                    //}

                }
            }

            return strText;
        }

        public static string getInsertScriptForTable(string connectionString, string tableName)
        {
            return getInsertScriptForTable(connectionString, "dbo", tableName);
        }

        public static string getInsertScriptForTable(string connectionString, string schema, string tableName)
        {
            string strText = string.Empty;
            string identityColumn = "";
            string strP = "INSERT [" + schema + "].[" + tableName + "](";
            List<string> columns = getColumnsFromTable(connectionString, schema, tableName);
            for (int i = 0; i < columns.Count; i++)
            {
                string[] strC = columns[i].Split(',');
                if (strC[3] == "true")
                {
                    identityColumn = strC[0].Trim();
                }
                else
                {
                    strP += "[" + strC[0].Trim() + "]";
                    if (i < columns.Count - 1)
                    {
                        strP += ",";
                    }
                }
            }
            strP += ") VALUES(";

            DataAccess DB = new DataAccess(connectionString);
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT * FROM [" + schema + "].[" + tableName + "]";
            DataSet aps = DB.ExecuteDataSet(command);
            int lt = aps.Tables.Count;
            if (lt > 0)
            {
                DataTable dt = aps.Tables[0]; // data
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string strData = string.Empty;
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            if (dt.Columns[j].ColumnName != identityColumn)
                            {
                                string dataType = dt.Columns[j].DataType.ToString().ToLower();
                                switch (dataType)
                                {
                                    case "system.string":
                                    case "system.datetime":
                                        strData += "'" + dt.Rows[i][j] + "'";
                                        break;
                                    case "system.boolean":
                                        strData += dt.Rows[i][j].ToString() == "True" ? "1" : "0";
                                        break;
                                    default:
                                        strData += dt.Rows[i][j];
                                        break;
                                }


                                if (j < dt.Columns.Count - 1)
                                {
                                    strData += ", ";
                                }
                            }
                        }
                        strData += ")\r\n";
                        strText += strP + strData;
                    }
                }
            }

            return strText;
        }
    }
}
