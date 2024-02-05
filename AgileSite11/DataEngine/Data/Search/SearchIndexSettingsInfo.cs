using System;
using System.Collections;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Search Index Settings Info object.
    /// </summary>
    public class SearchIndexSettingsInfo : IDataContainer
    {

        #region "Variables & Constants"

        /// <summary>
        /// Public constant for item of type allowed.
        /// </summary>
        public const string TYPE_ALLOWED = "allowed";

        /// <summary>
        /// Public constant for item of type excluded.
        /// </summary>
        public const string TYPE_EXLUDED = "excluded";

        /// <summary>
        /// Collection of the data contents.
        /// </summary>
        protected Hashtable mData = new Hashtable();

        /// <summary>
        /// Split class names.
        /// </summary>
        private string[] splitClassNames = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets item identifier.
        /// </summary>
        public virtual Guid ID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("id"), Guid.Empty);
            }
            set
            {
                SetValue("id", value.ToString());
            }
        }


        /// <summary>
        /// Gets or sets type.
        /// </summary>
        public virtual string Type
        {
            get
            {
                return ValidationHelper.GetString(GetValue("type"), String.Empty);
            }
            set
            {
                SetValue("type", value);
            }
        }


        /// <summary>
        /// Gets or sets class names of the custom tables.
        /// </summary>
        public virtual string ClassNames
        {
            get
            {
                return ValidationHelper.GetString(GetValue("classnames"), String.Empty);
            }
            set
            {
                SetValue("classnames", value);
            }
        }


        /// <summary>
        /// Gets or sets the where condition.
        /// </summary>
        public virtual string WhereCondition
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WhereCondition"), String.Empty);
            }
            set
            {
                SetValue("WhereCondition", value);
            }
        }


        /// <summary>
        /// Gets or sets the item sitename.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("sitename"), String.Empty);
            }
            set
            {
                SetValue("sitename", value);
            }
        }


        /// <summary>
        /// Gets or sets the forum names.
        /// </summary>
        public virtual string ForumNames
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ForumNames"), String.Empty);
            }
            set
            {
                SetValue("ForumNames", value);
            }
        }


        /// <summary>
        /// Gets or sets path.
        /// </summary>
        public virtual string Path
        {
            get
            {
                return ValidationHelper.GetString(GetValue("path"), String.Empty);
            }
            set
            {
                SetValue("path", value);
            }
        }


        /// <summary>
        /// Enables or disables forums inclusion.
        /// </summary>
        public virtual bool IncludeForums
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("inclforums"), false);
            }
            set
            {
                SetValue("inclforums", value);
            }
        }


        /// <summary>
        /// Enables or disables blogs inclusion.
        /// </summary>
        public virtual bool IncludeBlogs
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("inclblogcomm"), false);
            }
            set
            {
                SetValue("inclblogcomm", value);
            }
        }


        /// <summary>
        /// Enables or disables message communication inclusion.
        /// </summary>
        public virtual bool IncludeMessageCommunication
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("inclmessagecomm"), false);
            }
            set
            {
                SetValue("inclmessagecomm", value);
            }
        }


        /// <summary>
        /// Enables or disables categories inclusion.
        /// </summary>
        public virtual bool IncludeCategories
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("inclcats"), false);
            }
            set
            {
                SetValue("inclcats", value);
            }
        }


        /// <summary>
        /// Enables or disables attachment content indexing. If true, all the attachments with supported file 
        /// type (for which there is an extractor and which is allowed in the Site Manager setting) the text is 
        /// extracted and included in the search index.
        /// </summary>
        public virtual bool IncludeAttachments
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("inclatt"), false);
            }
            set
            {
                SetValue("inclatt", value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if there is classname in this item which matches specified parameter.
        /// </summary>
        /// <param name="className">ClassName to be matched</param>        
        public bool MatchClassNames(string className)
        {
            // Check whether classnames is empty - means all classes
            if (string.IsNullOrEmpty(ClassNames))
            {
                return true;
            }

            // If not split yet, do it
            if (splitClassNames == null)
            {
                splitClassNames = ClassNames.Split(';');
            }

            foreach (string cn in splitClassNames)
            {
                if (CMSString.Compare(className, cn, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if <see cref="Path"/> in this item matches specified parameter.
        /// </summary>
        /// <param name="path">Path to be matched</param>        
        public bool MatchPath(string path)
        {
            return SqlHelper.MatchLikePattern(path, Path);
        }

        #endregion


        #region "IDataContainer Members"

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
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Column names.
        /// </summary>
        public virtual List<string> ColumnNames
        {
            get
            {
                lock (mData)
                {
                    return TypeHelper.NewList(mData.Keys);
                }
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetValue(string columnName, out object value)
        {
            if ((mData == null) || (mData[columnName] == DBNull.Value))
            {
                value = null;
            }
            else
            {
                value = mData[columnName];
            }

            return true;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object GetValue(string columnName)
        {
            if ((mData == null) || (mData[columnName] == DBNull.Value))
            {
                return null;
            }
            else
            {
                return mData[columnName];
            }
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public virtual bool SetValue(string columnName, object value)
        {
            if (mData != null)
            {
                // Clear array of splitted classnames
                if (columnName.ToLowerCSafe() == "classnames")
                {
                   splitClassNames = null;
                }

                mData[columnName] = value;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool ContainsColumn(string columnName)
        {
            return mData.Contains(columnName);
        }

        #endregion
    }
}