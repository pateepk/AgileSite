using System;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;

namespace CMS.UIControls
{
    /// <summary>
    /// Files log control for debug purposes.
    /// </summary>
    public class FilesLog : LogControl
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the name of the type of the tasks which should be displayed.
        /// </summary>
        public string ProviderName
        {
            get;
            set;
        }


        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return FileDebug.Settings;
            }
        }


        /// <summary>
        /// If true, the debug list only write operations (delete, create, update)
        /// </summary>
        public bool WriteOnly
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns formatted text in a div (text written to a file) if is not null or empty, otherwise returns empty string.
        /// </summary>
        /// <param name="text">Text</param>
        public static string GetText(object text)
        {
            string textStr = ValidationHelper.GetString(text, "");
            if (!string.IsNullOrEmpty(textStr))
            {
                return "<div style=\"margin-top: 2px; border-top: solid 1px #dddddd;\">" + HTMLHelper.HTMLEncode(textStr) + "</div>";
            }
            return "";
        }


        /// <summary>
        /// Returns relative path if the file is in application path, otherwise returns absolute path.
        /// </summary>
        /// <param name="path">File path</param>
        public static string GetPath(object path)
        {
            string[] paths = ValidationHelper.GetString(path, "").Split('|');
            string appPath = HttpContext.Current.Server.MapPath("~/");
            string retval = "";
            int i = 0;
            foreach (string pathStr in paths)
            {
                if (i > 0)
                {
                    retval += "<div style=\"margin-top: 2px; border-top: solid 1px #dddddd;\">";
                }
                if (pathStr.StartsWithCSafe(appPath))
                {
                    retval += "~/" + Path.EnsureSlashes(pathStr.Substring(appPath.Length));
                }
                else
                {
                    retval += pathStr;
                }
                if (i > 0)
                {
                    retval += "</div>";
                }
                i++;
            }

            return retval;
        }


        /// <summary>
        /// Returns the list of items
        /// </summary>
        /// <param name="text">Text with the list</param>
        public static string GetList(object text)
        {
            string[] items = ValidationHelper.GetString(text, "").Split('|');
            string retval = "";
            int i = 0;
            foreach (string item in items)
            {
                if (i > 0)
                {
                    retval += "<div style=\"margin-top: 2px; border-top: solid 1px #dddddd;\">";
                }
                retval += item;

                if (i > 0)
                {
                    retval += "</div>";
                }
                i++;
            }

            return retval;
        }


        /// <summary>
        /// Gets the warning icon.
        /// </summary>
        /// <param name="notclosed">If true, the warning sign is shown</param>
        /// <param name="operation">Icon title</param>
        public new static object GetWarning(object notclosed, object operation)
        {
            if (ValidationHelper.GetString(operation, "").ToLowerCSafe() != "open")
            {
                return null;
            }

            return LogControl.GetWarning(notclosed, ResHelper.GetString("FilesLog.NotClosed"));
        }


        /// <summary>
        /// Returns file operation string.
        /// </summary>
        /// <param name="operation">File operation</param>
        /// <param name="parameters">Parameters of the operation</param>
        public static string GetFileOperation(object operation, object parameters)
        {
            return GetFileOperation(operation, parameters, true);
        }


        /// <summary>
        /// Returns file operation string.
        /// </summary>
        /// <param name="operation">File operation</param>
        /// <param name="parameters">Parameters of the operation</param>
        /// <param name="operationToUpper">Indicates if the operation should be in upper case</param>
        public static string GetFileOperation(object operation, object parameters, bool operationToUpper)
        {
            string operationStr = ValidationHelper.GetString(operation, "");
            if (operationToUpper)
            {
                operationStr = operationStr.ToUpperCSafe();
            }
            string parametersStr = ValidationHelper.GetString(parameters, "").ToLowerCSafe();
            if (parametersStr != "")
            {
                return "<strong>" + operationStr + "</strong> (" + parametersStr + ")";
            }
            else
            {
                return "<strong>" + operationStr + "</strong>";
            }
        }


        /// <summary>
        /// Returns size, number of accesses and size chart.
        /// </summary>
        /// <param name="maxSize">Maximal size</param>
        /// <param name="size">Actual size</param>
        /// <param name="accesses">Number of accesses</param>
        /// <param name="surround">If true, result, if not empty, is surrounded with div</param>
        public static string GetSizeAndAccesses(object maxSize, object size, object accesses, bool surround)
        {
            int sizeInt = ValidationHelper.GetInteger(size, -1);
            int accessesInt = ValidationHelper.GetInteger(accesses, -1);
            if (sizeInt < 0)
            {
                return "";
            }
            else
            {
                string retval = DataHelper.GetSizeString(sizeInt) + " (" + accessesInt + ")<br/>" + GetChart(maxSize, size, 10240, 0, 0);
                if (surround)
                {
                    retval = "<div style=\"margin-top: 2px; border-top: solid 1px #dddddd;\">" + retval + "</div>";
                }
                return retval;
            }
        }

        #endregion
    }
}