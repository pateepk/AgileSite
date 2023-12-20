using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CMS.DataEngine
{
    /// <summary>
    /// Holds system view names and underlying tables.
    /// </summary>
    internal class SystemViews
    {
        /// <summary>
        /// Key: table name
        /// Value: list of views, where the table is used
        /// </summary>
        private readonly Dictionary<string, List<string>> tableViews = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        private static SystemViews mInstance;


        /// <summary>
        /// Instance of <see cref="SystemViews"/>
        /// </summary>
        public static SystemViews Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Interlocked.CompareExchange(ref mInstance, new SystemViews(), null);
                }

                return mInstance;
            }
        }


        private SystemViews()
        {
            AddViewTables("View_CMS_User", "cms_user", "cms_usersettings");
            AddViewTables("View_CMS_ObjectVersionHistoryUser_Joined", "cms_user", "cms_objectversionhistory");
            AddViewTables("View_Community_Member", "cms_user", "cms_usersettings");

            AddViewTables("View_Community_Group", "community_group");

            AddViewTables("View_OM_Contact_Joined", "om_contact");

            AddViewTables("View_OM_Account_Joined", "om_account");
            AddViewTables("View_OM_ContactGroupMember_AccountJoined", "om_account");
        }


        private void AddViewTables(string viewName, params string[] tableNames)
        {
            foreach(var tableName in tableNames)
            {
                AddViewTable(viewName, tableName);
            }
        }

       
        private void AddViewTable(string viewName, string tableName)
        {
            if (tableViews.ContainsKey(tableName))
            {
                tableViews[tableName].Add(viewName);
            }
            else
            {
                tableViews.Add(tableName, new List<string>() { viewName });
            }
        }


        /// <summary>
        /// Gets enumeration of system views, where given table is used.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        public IEnumerable<string> GetViewsForTable(string tableName)
        {
            if (tableViews.ContainsKey(tableName))
            {
                return tableViews[tableName];
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }


        /// <summary>
        /// Gets enumeration of all system views.
        /// </summary>
        public IEnumerable<string> GetAllViews()
        {
            return tableViews.Values.SelectMany(views => views).Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
