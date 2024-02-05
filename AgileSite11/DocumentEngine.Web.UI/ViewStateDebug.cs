using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// ViewState debug methods
    /// </summary>
    public static class ViewStateDebug
    {
        #region "Variables"

        private static readonly CMSLazy<DebugSettings> mSettings = new CMSLazy<DebugSettings>(GetDebugSettings);


        // Depth of the control context parent.
        private const int mViewStateParentDepth = 2;


        private const string COLUMN_ID = "ID";
        private const string COLUMN_VIEW_STATE = "ViewState";
        private const string COLUMN_HAS_DIRTY = "HasDirty";
        private const string COLUMN_IS_DIRTY = "IsDirty";
        private const string COLUMN_VIEW_STATE_SIZE = "ViewStateSize";

        #endregion


        #region "Properties"

        /// <summary>
        /// Debug settings
        /// </summary>
        public static DebugSettings Settings
        {
            get
            {
                return mSettings.Value;
            }
        }


        /// <summary>
        /// Debug current request view state.
        /// </summary>
        public static bool DebugCurrentRequest
        {
            get
            {
                if (Settings.LogOperations)
                {
                    return DebugContext.CurrentRequestSettings[Settings];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                DebugContext.CurrentRequestSettings[Settings] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the debug settings
        /// </summary>
        private static DebugSettings GetDebugSettings()
        {
            return new DebugSettings("ViewState")
            {
                LogControl = "~/CMSAdminControls/Debug/ViewState.ascx"
            };
        }

        
        /// <summary>
        /// Gets the view states for the given page.
        /// </summary>
        public static RequestLog GetViewStates(Page page)
        {
            var dt = NewLogTable();

            try
            {
                AddViewStates(page, dt);
            }
            catch (Exception ex)
            {
                // Add error message
                DataRow dr = dt.NewRow();
                dr[COLUMN_ID] = "ViewStateLog";
                dr[COLUMN_VIEW_STATE] = "Unable to get the controls ViewState: " + ex.Message;
                dt.Rows.Add(dr);

                EventLogProvider.LogException("Debug", "GETVIEWSTATE", ex);
            }

            // Add to the global log
            if (!DataHelper.DataSourceIsEmpty(dt))
            {
                var logs = DebugContext.CurrentRequestLogs;

                var newLog = logs.CreateLog(dt, Settings);
                logs[Settings] = newLog;

                return newLog;
            }

            return null;
        }


        /// <summary>
        /// Creates a new log table
        /// </summary>
        private static DataTable NewLogTable()
        {
            DataTable dt = new DataTable();

            var cols = dt.Columns;
            cols.Add(new DataColumn(COLUMN_ID, typeof(string)));
            cols.Add(new DataColumn(COLUMN_VIEW_STATE, typeof(string)));
            cols.Add(new DataColumn(COLUMN_HAS_DIRTY, typeof(bool)));
            cols.Add(new DataColumn(COLUMN_IS_DIRTY, typeof(string)));
            cols.Add(new DataColumn(COLUMN_VIEW_STATE_SIZE, typeof(int)));

            return dt;
        }


        /// <summary>
        /// Adds the viewstates to the specified DataSet.
        /// </summary>
        /// <param name="dt">Table with the viewstates</param>
        /// <param name="page">Processed page</param>
        private static void AddViewStates(Page page, DataTable dt)
        {
            if (page.EnableViewState)
            {
                var viewStateMode = page.ViewStateMode;
                if (viewStateMode == ViewStateMode.Inherit)
                {
                    // We're on top of the control tree
                    // If the ViewStateMode is set to Inherit we consider it as Enabled as this is default value.
                    viewStateMode = ViewStateMode.Enabled;
                }

                // Add all page controls
                foreach (Control c in page.Controls)
                {
                    AddViewStates(dt, c, viewStateMode);
                }
            }
        }


        /// <summary>
        /// Adds the viewstates to the specified DataSet.
        /// </summary>
        /// <param name="dt">Table with the viewstates</param>
        /// <param name="control">Control to process</param>
        /// <param name="inheritedViewStateMode">Inherited <see cref="ViewStateMode"/>. Must be Enabled or Disabled.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="inheritedViewStateMode"/> is <see cref="ViewStateMode.Inherit"/></exception>
        private static void AddViewStates(DataTable dt, Control control, ViewStateMode inheritedViewStateMode)
        {
            if (inheritedViewStateMode == ViewStateMode.Inherit)
            {
                throw new ArgumentException("Inherited ViewStateMode must define if the ViewStateMode is Enabled or Disabled", "inheritedViewStateMode");
            }

            if (!control.EnableViewState || !control.Visible)
            {
                //Parent has disabled ViewState or is not Visible
                //There is no point to evaluate ViewState for the control or its children
                return;
            }

            var viewStateMode = (control.ViewStateMode == ViewStateMode.Inherit) ? inheritedViewStateMode : control.ViewStateMode;
            if (viewStateMode == ViewStateMode.Enabled)
            {
                StateBag viewState = GetControlViewState(control);
                if ((viewState != null) && (viewState.Count > 0))
                {
                    DataRow dr = dt.NewRow();
                    dr[COLUMN_ID] = GetControlIdentifier(control); ;

                    AddViewStateDetails(viewState, dr);

                    dt.Rows.Add(dr);

                    DebugEvents.ViewStateDebugItemLogged.StartEvent(dr);
                }
            }

            // Add child controls
            foreach (Control c in control.Controls)
            {
                AddViewStates(dt, c, viewStateMode);
            }
        }


        /// <summary>
        /// Gets the control viewstate.
        /// </summary>
        /// <param name="control">Control</param>
        public static StateBag GetControlViewState(Control control)
        {
            if (control == null)
            {
                return null;
            }

            Type controlType = control.GetType();
            PropertyInfo viewStateProperty = controlType.GetProperty("ViewState", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic);
            if (viewStateProperty != null)
            {
                return (StateBag)viewStateProperty.GetValue(control, null);
            }

            return null;
        }


        /// <summary>
        /// Gets identifier for the control from its Id and from Ids of its parents for the debug info.
        /// The identifier is reflecting its parent to child path.
        /// </summary>
        /// <param name="control">Control</param>
        /// <returns>Composed identifier of the control.</returns>
        private static string GetControlIdentifier(Control control)
        {
            string id = control.ID;
            if (String.IsNullOrEmpty(id))
            {
                id = "N/A";
            }

            // Add parent context
            Control parent = control.Parent;
            int index = 1;
            while ((parent != null) && (index <= mViewStateParentDepth))
            {
                if ((parent is HtmlForm) || (parent is Page))
                {
                    break;
                }

                // Add parent ID
                string parentId = parent.ID;
                if (!String.IsNullOrEmpty(parentId))
                {
                    id = parentId + "\n" + id;
                }

                parent = parent.Parent;
                index++;
            }

            return id;
        }


        /// <summary>
        /// Adds <paramref name="viewState"/> details into the <paramref name="dataRow"/>.
        /// </summary>
        /// <param name="viewState">viewstate</param>
        /// <param name="dataRow">data row</param>
        private static void AddViewStateDetails(StateBag viewState, DataRow dataRow)
        {
            string state = null;
            string isDirty = null;

            bool hasDirty = false;
            int totalSize = 0;

            // Write all items
            foreach (DictionaryEntry key in viewState)
            {
                StateItem item = (StateItem)key.Value;
                object value = item.Value;
                int size;

                // Get the object string
                string stringValue = DataHelper.GetObjectString(value, 500, out size);

                state += "<strong>" + key.Key + " = </strong>" + HTMLHelper.HTMLEncode(stringValue) + "\n";

                if (item.IsDirty)
                {
                    hasDirty = true;
                    isDirty += "true\n";
                }
                else
                {
                    isDirty += "false\n";
                }

                totalSize += size;
            }

            dataRow[COLUMN_VIEW_STATE] = state;
            dataRow[COLUMN_HAS_DIRTY] = hasDirty;
            dataRow[COLUMN_IS_DIRTY] = isDirty;
            dataRow[COLUMN_VIEW_STATE_SIZE] = totalSize;
        }

        #endregion
    }
}
