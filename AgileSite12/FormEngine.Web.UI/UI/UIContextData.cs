using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;

using System;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class used for storing context data for UI.
    /// </summary>
    public class UIContextData : XmlData
    {
        #region "Variables"

        private MacroResolver mContextResolver;
        private bool mResolveMacros = true;

        /// <summary>
        /// Name prefix for categories change
        /// </summary>
        public static String CATEGORYNAMEPREFIX = "category_name_";

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether property should be resolved with contexts resolver.
        /// </summary>
        public bool ResolveMacros
        {
            get
            {
                return mResolveMacros;
            }
            set
            {
                mResolveMacros = value;
            }
        }


        /// <summary>
        /// Override indexer, add fallback to querystring
        /// </summary>
        /// <param name="key">Key name to collection</param>
        public override object this[String key]
        {
            get
            {
                return GetKeyValue(key, true, false);
            }
            set
            {
                base[key] = value;
            }
        }


        /// <summary>
        /// Data context resolver.
        /// </summary>
        public MacroResolver ContextResolver
        {
            get
            {
                return mContextResolver ?? (mContextResolver = MacroContext.CurrentResolver.CreateChild());
            }
            set
            {
                mContextResolver = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor. Sets excluded columns property.
        /// </summary>
        public UIContextData()
        {
            ExcludedColumns = CATEGORYNAMEPREFIX;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Event fired when the UI context dynamic data is requested
        /// </summary>
        public event EventHandler<UIContextEventArgs> OnGetValue;

        #endregion


        #region "Methods"

        /// <summary>
        /// Clones current object
        /// </summary>
        public new UIContextData Clone()
        {
            UIContextData newData = new UIContextData();
            newData.mData = new StringSafeDictionary<object>(mData);

            return newData;
        }


        /// <summary>
        /// Returns value assigned to given key
        /// </summary>
        /// <param name="key">Key for assigned data</param>
        /// <param name="useQueryStringFallBack">Indicates whether data will be got from query string if not found in collection.</param>
        /// <param name="avoidInjection">If true, the resolving of the macros should avoid SQL injection (escapes the apostrophes in output).</param>
        public object GetKeyValue(String key, bool useQueryStringFallBack, bool avoidInjection)
        {
            object val = base[key];
            if ((val is String) && ResolveMacros)
            {
                MacroSettings ms = new MacroSettings();
                ms.AvoidInjection = avoidInjection;

                val = ContextResolver.ResolveMacros(val as String, ms);
            }

            // First try get value from RaiseGetValue event. If not found, use query string to get value (if flag use.. is set)
            val = (val ?? 
                   RaiseGetValue(key) ?? 
                   (useQueryStringFallBack ? HTMLHelper.HTMLEncode(QueryHelper.GetString(key, null)) : null));

            return val;
        }


        /// <summary>
        /// Loads value of the column. Returns true if the operation was successful (the value was present)
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        public override bool TryGetValue(string columnName, out object value)
        {
            if (mData == null)
            {
                value = null;
                return false;
            }

            // Get the value
            value = this[columnName];
            if (value == DBNull.Value)
            {
                value = null;
            }

            return true;
        }


        /// <summary>
        /// Checks for registered events
        /// </summary>
        /// <param name="columnName">Column name</param>
        private object RaiseGetValue(String columnName)
        {
            if (OnGetValue != null)
            {
                var args = new UIContextEventArgs()
                {
                    ColumnName = columnName
                };

                OnGetValue(this, args);

                // If any value found, store it and return it
                if (args.Result != null)
                {
                    this[columnName] = args.Result;
                    return args.Result;
                }
            }

            return null;
        }

        #endregion
    }
}