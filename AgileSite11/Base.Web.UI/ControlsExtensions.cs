using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Base;
using CMS.EventLog;
using CMS.IO;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ControlsExtensions
    {
        #region "Short ID support for controls"

        private static bool? mRenderShortIDs;


        /// <summary>
        /// If true, the short IDs of the controls are rendered where possible.
        /// </summary>
        public static bool RenderShortIDs
        {
            get
            {
                if (mRenderShortIDs == null)
                {
                    mRenderShortIDs = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRenderShortIDs"], true);
                }

                return mRenderShortIDs.Value;
            }
            set
            {
                mRenderShortIDs = value;
            }
        }


        /// <summary>
        /// Sets the short ID to the ID if available.
        /// </summary>
        public static void SetShortID(this IShortID ctrl)
        {
            if ((ctrl.ShortID != null) && RenderShortIDs && (ctrl.ID != ctrl.ShortID) && (HttpContext.Current != null))
            {
                ctrl.ID = ctrl.ShortID;
            }
        }

        #endregion


        #region "Handling WebControls' CSS classes"

        /// <summary>
        /// Adds CSS class to control. If class is already present, than adding is skipped. Class names are case sensitive.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="cssClass">CSS class to add to the control</param>
        public static void AddCssClass(this WebControl control, string cssClass)
        {
            if (String.IsNullOrEmpty(cssClass))
            {
                return;
            }

            // new HashSet because HashSet removes all recurrences
            HashSet<string> classes = new HashSet<string>(control.GetCssClasses());

            if (!classes.Contains(cssClass))
            {
                classes.Add(cssClass);

                control.SetCssClasses(classes);
            }
        }


        /// <summary>
        /// Removes all occurrences. Class names are case sensitive.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="cssClass">CSS class to remove from the control</param>
        public static void RemoveCssClass(this WebControl control, string cssClass)
        {
            if (String.IsNullOrEmpty(cssClass))
            {
                return;
            }

            // new HashSet because HashSet removes all recurrences
            HashSet<string> classes = new HashSet<string>(control.GetCssClasses());
            classes.Remove(cssClass);

            control.SetCssClasses(classes);
        }


        /// <summary>
        /// Checks whether the control has given CSS class. Class names are case sensitive.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="cssClass">CSS class to check for in the control</param>
        public static bool HasCssClass(this WebControl control, string cssClass)
        {
            if (String.IsNullOrEmpty(cssClass))
            {
                return false;
            }

            return control.GetCssClasses().Contains(cssClass);
        }


        /// <summary>
        /// Returns all CSS classes of control. 
        /// </summary>
        /// <param name="control">Control</param>
        /// <returns>Control's classes</returns>
        public static IEnumerable<string> GetCssClasses(this WebControl control)
        {
            return control.CssClass.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }


        /// <summary>
        /// Sets CSS classes to the control. Current classes will be overridden.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="classes">CSS classes to set to control</param>
        public static void SetCssClasses(this WebControl control, IEnumerable<string> classes)
        {
            control.CssClass = string.Join(" ", classes.Where(c => !String.IsNullOrEmpty(c)));
        }

        #endregion


        #region "Load control"

        /// <summary>
        /// Loads the user control based on the given path
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="control">Control to load, can be either control path, or Type</param>
        public static Control LoadUserControl(this Page page, ControlDefinition control)
        {
            // Page is not available
            if ((page == null) && (HttpContext.Current != null))
            {
                // Try to use handler as a page
                var p = HttpContext.Current.Handler as Page;
                if (p != null)
                {
                    return control.Load(p);
                }
            }
            else if (page != null)
            {
                return control.Load(page);
            }

            return null;
        }

        #endregion


        #region "Exception handling"

        /// <summary>
        /// Calls the method handled by the exception handler. If the call doesn't succeed, the problem is reported through parent IExceptionHandler control instead of throwing unhandled exception for the entire page. 
        /// Returns true, if the call succeeded, otherwise returns false.
        /// </summary>
        /// <param name="ctrl">Control that calls the method</param>
        /// <param name="func">Method to execute</param>
        public static bool CallHandled(this Control ctrl, Action func)
        {
            try
            {
                // Call the method
                func();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception to the event log
                EventLogProvider.LogException("Control", "LOAD", ex);

                // Get the parent exception handler
                var handler = ControlsHelper.GetParentControl(ctrl, typeof(IExceptionHandler));

                // Re-throw exception when exception cannot be reported. Don't consume it.
                if ((handler == null) || (handler.Parent == null))
                {
                    throw;
                }

                // Stop processing of the control if available
                var stpCtrl = ctrl as IStopProcessing;
                if (stpCtrl != null)
                {
                    stpCtrl.StopProcessing = true;
                }

                ((IExceptionHandler)handler).ReportException(ex);

                return false;
            }
        }


        /// <summary>
        /// Reports the exception within the control hierarchy by placing an exception report next to the given control.
        /// </summary>
        /// <param name="handler">Handler control</param>
        /// <param name="ex">Exception to report</param>
        public static void ReportException(this IExceptionHandler handler, Exception ex)
        {
            var ctrl = (Control)handler;

            ctrl.Page.SaveStateComplete += ((sender, e) =>
                {
                    // Add the exception after the control
                    var par = ctrl.Parent;
                    int index = par.Controls.IndexOf(ctrl);

                    string title = String.Format("[Error loading the control '{0}', check event log for more details]", ctrl.ID);

                    par.Controls.AddAt(index + 1, new ExceptionReport(ex, title));
                }
            );
        }

        #endregion


        #region "Miscellaneous methods"

        /// <summary>
        /// Gets the string by the specified resource key
        /// </summary>
        /// <param name="ctrl">Object</param>
        /// <param name="resourceKey">Resource key</param>
        public static string GetString(this Control ctrl, string resourceKey)
        {
            return ControlsLocalization.GetString(ctrl, resourceKey);
        }


        /// <summary>
        /// Adds a localized item to the list control
        /// </summary>
        /// <param name="listControl">List control</param>
        /// <param name="resourceKey">Resource string</param>
        /// <param name="value">Item value</param>
        internal static void AddLocalizedItem(this ListControl listControl, string resourceKey, string value)
        {
            var item = listControl.CreateLocalizedItem(resourceKey, value);

            listControl.Items.Add(item);
        }


        /// <summary>
        /// Creates a localized item for the given list control
        /// </summary>
        /// <param name="listControl">List control</param>
        /// <param name="resourceKey">Resource string</param>
        /// <param name="value">Item value</param>
        internal static ListItem CreateLocalizedItem(this ListControl listControl, string resourceKey, string value)
        {
            var text = listControl.GetString(resourceKey);
            var item = new ListItem(text, value);

            return item;
        }


        /// <summary>
        /// Returns HTML which would be rendered by control to page.
        /// </summary>
        /// <param name="ctrl">Control to obtain its HTML</param>
        public static string GetRenderedHTML(this WebControl ctrl)
        {
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                // Get controls HTML
                using (var writer = new HtmlTextWriter(sw))
                {
                    ctrl.RenderControl(writer);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Sets the grid headers to the given resource strings
        /// </summary>
        /// <param name="grid">Grid to set</param>
        /// <param name="resourceStrings">Resouce strings</param>
        public static void SetHeaders(this GridView grid, params string[] resourceStrings)
        {
            var i = 0;

            foreach (var rs in resourceStrings)
            {
                grid.Columns[i].HeaderText = ResHelper.GetString(rs);
                i++;
            }
        }

        #endregion
    }
}