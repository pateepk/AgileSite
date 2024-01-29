using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for SQL object edit control
    /// </summary>
    public abstract class SqlEditControl : CMSUserControl
    {
        #region "Constants"

        /// <summary>
        /// Custom prefix for stored procedure
        /// </summary>
        protected const string PROCEDURE_CUSTOM_PREFIX = "Proc_Custom_";

        /// <summary>
        /// Custom prefix for the view
        /// </summary>
        protected const string VIEW_CUSTOM_PREFIX = "View_Custom_";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets name of the database object (view/stored procedure).
        /// </summary>
        public string ObjectName
        {
            get;
            set;
        }


        /// <summary>
        /// Current state of editing control (alter view, create view, alter procedure, create procedure).
        /// </summary>
        protected SqlEditModeEnum State
        {
            get
            {
                object state = ViewState["State"] ?? SqlEditModeEnum.None;
                return (SqlEditModeEnum)state;
            }
            set
            {
                ViewState["State"] = value;
            }
        }


        /// <summary>
        /// Gets or sets type of database object (view/stored procedure).
        /// </summary>
        public bool? IsView
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether parsing code of existing view/procedure failed/
        /// </summary>
        public bool FailedToLoad
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets.
        /// </summary>
        public bool HideSaveButton
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether rollback functionality is available for current object.
        /// </summary>
        public bool RollbackAvailable
        {
            get
            {
                string fName;
                return SQLScriptExists(ObjectName, out fName);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Extracts view name and code from SQL query.
        /// </summary>
        /// <param name="query">Entire SQL query from DB</param>
        /// <param name="name">View name</param>
        /// <param name="body">View body</param>
        /// <param name="withBinding">WITHBINDING modifier</param>
        protected static bool ParseView(string query, out string name, out string body, out bool withBinding)
        {
            name = null;
            body = null;
            withBinding = false;
            Regex re = RegexHelper.GetRegex(@".*?\s*(CREATE\s+VIEW)\s+(\S+)\s+((\s|[a-zA-Z])*?)AS(\s+.*)\s*", RegexOptions.Singleline | CMSRegex.IgnoreCase);
            if (re.IsMatch(query))
            {
                Match m = re.Match(query);
                name = m.Groups[2].Value;
                withBinding = m.Groups[3].Value.ToLowerCSafe().Contains("with schemabinding");
                body = m.Groups[5].Value;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Extracts stored procedure name and body from SQL query.
        /// </summary>
        /// <param name="query">Entire SQL query from DB</param>
        /// <param name="name">Procedure name</param>
        /// <param name="param">Parameters</param>
        /// <param name="body">Procedure body</param>
        protected static bool ParseProcedure(string query, out string name, out string param, out string body)
        {
            name = null;
            param = null;
            body = null;
            Regex re = RegexHelper.GetRegex(@".*?\s*(CREATE\s+PROCEDURE)\s+(\S+)\s+(.*)\s+(AS\s+BEGIN)(\s+.*)\s+(END)\s*", RegexOptions.Singleline | CMSRegex.IgnoreCase);
            if (re.IsMatch(query))
            {
                Match m = re.Match(query);
                name = m.Groups[2].Value;
                param = m.Groups[3].Value;
                body = m.Groups[5].Value;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Parses DB object name considering database schema.
        /// </summary>
        /// <param name="objName">Object name (view or stored procedure)</param>
        /// <param name="schema">DB schema</param>
        /// <param name="name">Object name</param>
        protected static void ParseName(string objName, out string schema, out string name)
        {
            List<string> list = new List<string>();

            int length = objName.Length;
            int state = 0;
            string buff = "";

            // Loop through object name and extract text in brackets [text1].[text2]
            // or text separated by period text1.text2 ("]]" is considered to be escape sequence for "]")
            for (int i = 0; i < length; i++)
            {
                char current = objName[i];
                char? next = (i < (length - 1)) ? (char?)objName[i + 1] : null;

                switch (current)
                {
                    case '[':
                        switch (state)
                        {
                            case 0:
                                state = 1; // Currently in bracket
                                break;
                            case 1:
                                buff += current;
                                break;
                        }
                        break;

                    case ']':
                        if (state == 1)
                        {
                            switch (next)
                            {
                                case ']': // Escape sequence
                                    buff += ']';
                                    break;
                                default:
                                    state = 0;
                                    list.Add(buff);
                                    buff = "";
                                    break;
                            }
                        }
                        break;

                    case '.':
                        if (state == 0)
                        {
                            list.Add(buff);
                            buff = String.Empty;
                        }
                        else if (state == 1)
                        {
                            buff += current;
                        }
                        break;

                    default:
                        buff += current;
                        if (next == null)
                        {
                            list.Add(buff);
                            buff = String.Empty;
                        }
                        break;
                }
            }

            schema = null;
            name = null;
            if (list.Count > 0) // Find anything?
            {
                name = list[list.Count - 1];
                if (list.Count > 1)
                {
                    schema = list[list.Count - 2];
                }
            }
        }


        /// <summary>
        /// Event rised when SQL code is successfully saved.
        /// </summary>
        public event EventHandler OnSaved;


        /// <summary>
        /// Runs edited view or stored procedure.
        /// </summary>
        /// <param name="objName">Object name</param>
        /// <param name="param">Stored procedure parameters</param>
        /// <param name="body">Procedure or view body</param>
        /// <param name="binding">Schema binding flag for views</param>
        /// <param name="indexes">Schema bound indexes</param>
        protected string SaveObject(string objName, string param, string body, bool binding, string indexes)
        {
            string result = new Validator().NotEmpty(objName, GetString("systbl.viewproc.objectnameempty"))
                                           .NotEmpty(body, GetString("systbl.viewproc.bodyempty")).Result;

            if (String.IsNullOrEmpty(result))
            {
                var state = State;

                // Use special prefix for user created views or stored procedures
                if (!SystemContext.DevelopmentMode)
                {
                    switch (state)
                    {
                        case SqlEditModeEnum.CreateView:
                            objName = VIEW_CUSTOM_PREFIX + objName;
                            result = new Validator().IsIdentifier(objName, GetString("systbl.viewproc.viewnotidentifierformat")).Result;
                            break;

                        case SqlEditModeEnum.CreateProcedure:
                            objName = PROCEDURE_CUSTOM_PREFIX + objName;
                            result = new Validator().IsIdentifier(objName, GetString("systbl.viewproc.procnotidentifierformat")).Result;
                            break;
                    }
                }

                if (String.IsNullOrEmpty(result))
                {
                    try
                    {
                        // Retrieve user friendly name
                        string schema, name;
                        ParseName(objName, out schema, out name);

                        using (var tr = new CMSTransactionScope())
                        {
                            var tm = new TableManager(null);

                            switch (state)
                            {
                                case SqlEditModeEnum.CreateView:
                                    // Check if view exists
                                    if (tm.ViewExists(objName))
                                    {
                                        result = String.Format(GetString("systbl.view.alreadyexists"), objName);
                                    }
                                    else
                                    {
                                        // Create view
                                        tm.CreateView(objName, body, binding);

                                        if (binding && !String.IsNullOrEmpty(indexes))
                                        {
                                            ConnectionHelper.ExecuteQuery(indexes, null, QueryTypeEnum.SQLQuery);
                                        }
                                    }
                                    break;

                                case SqlEditModeEnum.AlterView:
                                    // Alter view
                                    {
                                        tm.AlterView(objName, body, binding);
                                        if (binding && !String.IsNullOrEmpty(indexes))
                                        {
                                            ConnectionHelper.ExecuteQuery(indexes, null, QueryTypeEnum.SQLQuery);
                                        }
                                    }
                                    break;

                                case SqlEditModeEnum.CreateProcedure:
                                    // Check if stored procedure exists
                                    if (tm.GetCode(objName) != null)
                                    {
                                        result = String.Format(GetString("systbl.proc.alreadyexists"), objName);
                                    }
                                    else
                                    {
                                        // Create procedure
                                        tm.CreateProcedure(objName, param, body);
                                    }
                                    break;

                                case SqlEditModeEnum.AlterProcedure:
                                    // Alter procedure
                                    tm.AlterProcedure(objName, param, body);
                                    break;
                            }

                            tr.Commit();
                        }

                        if (String.IsNullOrEmpty(result))
                        {
                            ObjectName = name;

                            if (OnSaved != null)
                            {
                                OnSaved(this, EventArgs.Empty);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        result = e.Message;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Check if exists SQL script for this object in /App_Data/Install
        /// </summary>
        /// <param name="objName">Name of the object</param>
        /// <param name="filePath">File name</param>
        protected static bool SQLScriptExists(string objName, out string filePath)
        {
            if (!String.IsNullOrEmpty(objName))
            {
                filePath = Path.Combine(SqlInstallationHelper.GetSQLInstallPathToObjects(), objName + ".sql");

                return File.Exists(filePath);
            }
            filePath = null;

            return TableManager.IsGeneratedSystemView(objName);
        }


        /// <summary>
        /// Shows warning is necessary
        /// </summary>
        protected void ShowWarning()
        {
            if (!String.IsNullOrEmpty(ObjectName) && !FailedToLoad)
            {
                if (IsView == true)
                {
                    if (!ObjectName.StartsWithCSafe(VIEW_CUSTOM_PREFIX, true))
                    {
                        ShowWarning(GetString("systbl.view.notsystemview"));
                    }
                }
                else if (IsView == false)
                {
                    if (!ObjectName.StartsWithCSafe(PROCEDURE_CUSTOM_PREFIX, true))
                    {
                        ShowWarning(GetString("systbl.view.notsystemproc"));
                    }
                }
            }
        }

        #endregion
    }
}
