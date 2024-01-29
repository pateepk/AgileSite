using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Defines the base class for the advanced export control
    /// </summary>
    public abstract class AdvancedExport : CMSUserControl
    {
        #region "Constants"

        /// <summary>
        /// Close dialog function prefix
        /// </summary>
        protected const string CLOSE_DIALOG = "closedialog";


        /// <summary>
        /// Short link to help page.
        /// </summary>
        protected const string HELP_TOPIC_LINK = "uidata_advanced_export";

        #endregion


        #region "Variables"

        private UniGridExportHelper mUniGridExportHelper;
        private bool mAlertAdded;

        #endregion


        #region "Enumerations"

        /// <summary>
        /// CSV delimiter.
        /// </summary>
        protected enum Delimiter
        {
            /// <summary>
            /// Comma delimiter
            /// </summary>
            Comma,

            /// <summary>
            /// Semicolor delimiter
            /// </summary>
            Semicolon
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Holds an instance of the UniGrid control.
        /// </summary>
        public UniGrid UniGrid
        {
            get;
            set;
        }


        /// <summary>
        /// Gets export helper for UniGrid.
        /// </summary>
        protected UniGridExportHelper UniGridExportHelper
        {
            get
            {
                if (mUniGridExportHelper == null)
                {
                    mUniGridExportHelper = new UniGridExportHelper(UniGrid);
                    mUniGridExportHelper.Error += mUniGridExportHelper_Error;
                    mUniGridExportHelper.MacroResolver = MacroContext.CurrentResolver;
                }

                return mUniGridExportHelper;
            }
        }


        /// <summary>
        /// Gets or sets identifier of current dialog (in case of more dialogs).
        /// </summary>
        protected string CurrentDialogID
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CurrentDialogID"], string.Empty);
            }
            set
            {
                ViewState["CurrentDialogID"] = value;
            }
        }


        /// <summary>
        /// Gets or sets current modal dialog ID (in case of more popups).
        /// </summary>
        protected string CurrentModalID
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CurrentModalID"], string.Empty);
            }
            set
            {
                ViewState["CurrentModalID"] = value;
            }
        }


        /// <summary>
        /// Gets current dialog control (in case of more dialogs).
        /// </summary>
        protected Panel CurrentDialog
        {
            get
            {
                return (Panel)FindControl(CurrentDialogID);
            }
        }


        /// <summary>
        /// Gets current modal control (in case of more popups).
        /// </summary>
        protected ModalPopupDialog CurrentModal
        {
            get
            {
                return (ModalPopupDialog)FindControl(CurrentModalID);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the control should close the dialog
        /// </summary>
        protected bool ShouldCloseDialog()
        {
            return Request.Params.Get("__EVENTARGUMENT") == CLOSE_DIALOG;
        }


        /// <summary>
        /// Handles possible errors during export
        /// </summary>
        /// <param name="customMessage">Message set when error occurs</param>
        /// <param name="exception">Original exception</param>
        protected void mUniGridExportHelper_Error(string customMessage, Exception exception)
        {
            AddAlert(customMessage);
        }


        /// <summary>
        /// Trims values returned by extended text area.
        /// </summary>
        /// <param name="text">Text to normalize</param>
        /// <returns>Text without line breaks at the end</returns>
        protected string TrimExtendedTextAreaValue(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                while (text.EndsWithCSafe("\n") || text.EndsWithCSafe("\r"))
                {
                    text = text.TrimEnd('\n').TrimEnd('\r');
                }
            }
            return text;
        }


        /// <summary>
        /// Determines whether UniGrid's info object contains given column.
        /// </summary>
        /// <param name="field">Column to check</param>
        /// <returns>TRUE if column is available in info object (or object type is not used)</returns>
        protected bool IsColumnAvailable(string field)
        {
            if (!string.IsNullOrEmpty(UniGrid.ObjectType))
            {
                // When loading using object type
                if (UniGrid.InfoObject != null)
                {
                    return UniGrid.InfoObject.ColumnNames.Contains(field);
                }
            }
            else
            {
                // For other types of loading (cannot determine whether column is available)
                return true;
            }
            return false;
        }


        /// <summary>
        /// Performs actions necessary to show the popup dialog.
        /// </summary>
        /// <param name="dialogControl">New dialog control</param>
        /// <param name="modalPopup">Modal control</param>
        protected void ShowPopup(Control dialogControl, ModalPopupDialog modalPopup)
        {
            // Set new identifiers
            CurrentModalID = modalPopup.ID;
            CurrentDialogID = dialogControl.ID;

            if ((CurrentModal != null) && (CurrentDialog != null))
            {
                // Enable dialog control's viewstate and visibility
                CurrentDialog.EnableViewState = true;
                CurrentDialog.Visible = true;

                // Show modal popup
                CurrentModal.Show();
            }
        }


        /// <summary>
        /// Performs actions necessary to hide popup dialog.
        /// </summary>
        protected void HideCurrentPopup()
        {
            if ((CurrentModal != null) && (CurrentDialog != null))
            {
                // Hide modal dialog
                CurrentModal.Hide();

                // Reset dialog control's viewstate and visibility
                CurrentDialog.EnableViewState = false;
                CurrentDialog.Visible = false;
            }

            // Reset identifiers
            CurrentModalID = null;
            CurrentDialogID = null;
        }


        /// <summary>
        /// Adds alert script to the page.
        /// </summary>
        /// <param name="message">Message to show</param>
        private void AddAlert(string message)
        {
            if (!mAlertAdded)
            {
                AddScript(ScriptHelper.GetScript("setTimeout(function() {" + ScriptHelper.GetAlertScript(message, false) + "}, 50);"));
                mAlertAdded = true;
            }
        }


        /// <summary>
        /// Adds script to the page.
        /// </summary>
        /// <param name="script">Script to add</param>
        private void AddScript(string script)
        {
            ScriptHelper.RegisterStartupScript(this, typeof(string), script.GetHashCode().ToString(), script);
        }

        #endregion
    }
}
