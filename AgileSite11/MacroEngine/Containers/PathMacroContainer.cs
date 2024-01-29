using System;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Object encapsulating path macro resolving as ISimpleDataContainer.
    /// </summary>
    public class PathMacroContainer : ISimpleDataContainer
    {
        #region "Variables"

        /// <summary>
        /// Regular expression to capture the path level macro.
        /// </summary>
        protected static Regex mLevelRegExp = null;


        /// <summary>
        /// Path segments array.
        /// </summary>
        protected string[] mPathSegments = null;


        /// <summary>
        /// Current path.
        /// </summary>
        protected string mCurrentPath = null;


        /// <summary>
        /// Resolver used to resolve the paths.
        /// </summary>
        protected MacroResolver mResolver = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current path.
        /// </summary>
        public string CurrentPath
        {
            get
            {
                // Load current alias path
                if (mCurrentPath == null)
                {
                    EvaluationResult result = mResolver.ResolveMacroExpression("CurrentPath");
                    if (result != null)
                    {
                        mCurrentPath = ValidationHelper.GetString(result.Result, "");
                    }
                    else
                    {
                        mCurrentPath = "";
                    }
                }
                return mCurrentPath;
            }
            set
            {
                mCurrentPath = value;
                mPathSegments = null;
            }
        }


        /// <summary>
        /// Regular expression to capture the path level macro.
        /// </summary>
        public static Regex LevelRegExp
        {
            get
            {
                // Expression groups:                                       (1:index)
                return mLevelRegExp ?? (mLevelRegExp = RegexHelper.GetRegex("{(-?\\d+)}"));
            }
            set
            {
                mLevelRegExp = value;
            }
        }


        /// <summary>
        /// Path segments array.
        /// </summary>
        private string[] PathSegments
        {
            get
            {
                return mPathSegments ?? (mPathSegments = CurrentPath.Trim('/').Split('/'));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of PathMacroContainer.
        /// </summary>
        /// <param name="resolver">Resolver to resolve path macros</param>
        public PathMacroContainer(MacroResolver resolver)
        {
            mResolver = resolver;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Resolves the given alias path, applies the path segment to the given format string {0} for level 0.
        /// </summary>
        /// <param name="format">Path pattern</param>
        /// <param name="escapeSpecChars">Indicates whether special characters in the automatically added part of path should be escaped for valid SQL LIKE query </param>
        public string ResolvePath(string format, bool escapeSpecChars = false)
        {
            // If not path given, resolve as current
            if (string.IsNullOrEmpty(format))
            {
                return CurrentPath;
            }

            // Indicates whether escape SpecChars method is allowed (only dynamic path can be escaped)
            bool allowEscape = false;
            bool processSegments = false;

            string path = format;
            Match m = LevelRegExp.Match(format);
            if (m.Success)
            {
                // Resolve levels
                path = LevelRegExp.Replace(format, LevelMatch);
                processSegments = true;
            }

            // Add current path if starting
            if ((path == ".") || (path == "..") || path.StartsWithCSafe("./") || path.StartsWithCSafe("../"))
            {
                path = CurrentPath.TrimEnd('/') + "/" + path;
                allowEscape = true;
                processSegments = true;
            }

            // Do not process path without any dynamic parameter
            if (!processSegments)
            {
                return path;
            }

            // Resolve relative shifts (".", "..")
            string[] segments = path.Trim('/').Split('/');
            path = "/";
            foreach (string segment in segments)
            {
                switch (segment)
                {
                    case ".":
                        // Keep current
                        allowEscape = false;
                        break;

                    case "..":
                        // One parent higher
                        path = DataHelper.GetParentPath(path);
                        allowEscape = false;
                        break;

                    default:
                        // Add regular segment
                        if (segment != "")
                        {
                            path = path.TrimEnd('/') + "/";
                            if (allowEscape && escapeSpecChars)
                            {
                                path += SqlHelper.EscapeLikeText(segment);
                            }
                            else
                            {
                                path += segment;
                            }
                        }
                        break;
                }
            }

            return path;
        }


        /// <summary>
        /// Match evaluator for the path level macro evaluation.
        /// </summary>
        /// <param name="m">Regular expression match</param>
        private string LevelMatch(Match m)
        {
            // Process the index
            string indexString = m.Groups[1].ToString();
            int index = ValidationHelper.GetInteger(indexString, int.MinValue);
            if (index != int.MinValue)
            {
                // Negative index (relative from the current document)
                if (index < 0)
                {
                    index = PathSegments.Length + index;
                }

                // Do not evaluate larger indexes than 20
                if (index > 20)
                {
                    return m.ToString();
                }

                // If within available segments, return the segment
                if ((index >= 0) && (PathSegments.Length > index))
                {
                    return PathSegments[index]; 
                }
            }

            return "";
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Resolves the path macro.
        /// </summary>
        /// <param name="pathMacro">Path macro to resolve</param>
        public object GetValue(string pathMacro)
        {
            // Escape special characters only if AvoidInjection flag is true
            var escapeSpecialChars = (mResolver.Settings != null) && mResolver.Settings.AvoidInjection;

            return ResolvePath(pathMacro, escapeSpecialChars);
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="pathMacro">Path macro</param>
        /// <param name="value">New value</param>
        public bool SetValue(string pathMacro, object value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}