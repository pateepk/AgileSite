using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;


namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class for displaying AJAX modal dialog.
    /// </summary>
    public class ModalPopupDialog : Panel
    {
        #region "Variables"

        private string mBackgroundCssClass = String.Empty;

        private string mCancelControlId = String.Empty;
        private string mPopupControlId = String.Empty;
        private string mOkControlId = String.Empty;

        private string mOnOkScript = String.Empty;
        private string mOnCancelScript = String.Empty;

        private bool showRequired = false;
        private bool hideRequired = false;
        private bool mCancelOnBackgroundClick = false;
        private bool mStopProcessing = false;

        private bool mShowMaximized = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the script which should be executed after OK button click.
        /// </summary>
        public string OnOkScript
        {
            get
            {
                return mOnOkScript;
            }
            set
            {
                mOnOkScript = value;
            }
        }


        /// <summary>
        /// Gets or sets the script which should be executed after Cancel button click.
        /// </summary>
        public string OnCancelScript
        {
            get
            {
                return mOnCancelScript;
            }
            set
            {
                mOnCancelScript = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether background click should cancel current popup.
        /// </summary>
        public bool CancelOnBackgroundClick
        {
            get
            {
                return mCancelOnBackgroundClick;
            }
            set
            {
                mCancelOnBackgroundClick = value;
            }
        }


        /// <summary>
        /// Gets or sets the background css class.
        /// </summary>
        public string BackgroundCssClass
        {
            get
            {
                return mBackgroundCssClass;
            }
            set
            {
                mBackgroundCssClass = value;
            }
        }


        /// <summary>
        /// Gets or sets the ID of control which should cancel modal popup.
        /// </summary>
        [DefaultValue(""), IDReferenceProperty(typeof(WebControl))]
        public string CancelControlID
        {
            get
            {
                return mCancelControlId;
            }
            set
            {
                mCancelControlId = value;
            }
        }


        /// <summary>
        /// Gets or sets the ID of control which should popup modal window.
        /// </summary>
        [DefaultValue(""), IDReferenceProperty(typeof(WebControl))]
        public string PopupControlID
        {
            get
            {
                return mPopupControlId;
            }
            set
            {
                mPopupControlId = value;
            }
        }


        /// <summary>
        /// Gets or sets the ID of control which should close modal window and continue with processing.
        /// </summary>
        [DefaultValue(""), IDReferenceProperty(typeof(WebControl))]
        public string OkControlID
        {
            get
            {
                return mOkControlId;
            }
            set
            {
                mOkControlId = value;
            }
        }


        /// <summary>
        /// Indicates if the control should perform the operations.
        /// </summary>
        public virtual bool StopProcessing
        {
            get
            {
                return mStopProcessing;
            }
            set
            {
                mStopProcessing = value;
            }
        }


        /// <summary>
        /// Indicates whether show modal dialog maximized (width and height is computed from window size).
        /// </summary>
        public bool ShowMaximized
        {
            get
            {
                return mShowMaximized;
            }
            set
            {
                mShowMaximized = value;
            }
        }


        /// <summary>
        /// Applies only if dialog is maximized. Modal dialog is shortened from all sizes with this number.
        /// </summary>
        public int MaxSizedEdgeSpace
        {
            get;
            set;
        }


        /// <summary>
        /// Javascript handler for additional script (used in maximized form).
        /// </summary>
        public String AdditionalInitHandler
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Shows dialog.
        /// </summary>
        public void Show()
        {
            showRequired = true;
            hideRequired = false;
        }


        /// <summary>
        /// Hides dialog.
        /// </summary>
        public void Hide()
        {
            hideRequired = true;
            showRequired = false;
        }


        /// <summary>
        /// Raises the OnPreRender event.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnPreRender(EventArgs e)
        {
            if (!StopProcessing)
            {
                // Register dialog script
                ScriptHelper.RegisterJQueryDialog(Page);
            }
            else
            {
                Visible = false;
            }
            base.OnPreRender(e);
        }


        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">HTML writer</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (!StopProcessing)
            {
                // Generate background id
                string backgroundId = ClientID + "_background";


                #region "CancelControlID"

                // Check whether CancelControl ID is defined
                if (!String.IsNullOrEmpty(CancelControlID))
                {
                    // Try find control
                    Control ctrl = FindControl(CancelControlID);
                    // If control was not found throw an exception
                    if (ctrl == null)
                    {
                        throw new Exception("[ModalPopupDialog] Cancel control with ID '" + CancelControlID + "' could not be found.");
                    }
                    // If control exist, register click event
                    else
                    {
                        ScriptHelper.RegisterStartupScript(Page, typeof(string), ClientID + "CancelControl", ScriptHelper.GetScript("RegisterClickHandler('" + ctrl.ClientID + "',function() { hideModalPopup('" + ClientID + "','" + backgroundId + "'); " + OnCancelScript + " });"));
                    }
                }

                #endregion


                #region "PopupControlID"

                // Check whether PopupControl ID is defined
                if (!String.IsNullOrEmpty(PopupControlID))
                {
                    // Try find control
                    Control ctrl = FindControl(PopupControlID);
                    // If control was not found throw an exception
                    if (ctrl == null)
                    {
                        throw new Exception("[ModalPopupDialog] Popup control with ID '" + PopupControlID + "' could not be found.");
                    }
                    // If control exist, register click event
                    else
                    {
                        String fceCall = "function() { showModalPopup('" + ClientID + "','" + backgroundId + "');}";
                        if (ShowMaximized)
                        {
                            fceCall = "function() { showModalMaximizedPopup('" + ClientID + "','" + backgroundId + "'," + MaxSizedEdgeSpace + (String.IsNullOrEmpty(AdditionalInitHandler) ? "" : "," + AdditionalInitHandler) + ");}";
                        }
                        ScriptHelper.RegisterStartupScript(Page, typeof(string), ClientID + "PopupControl", ScriptHelper.GetScript("RegisterClickHandler('" + ctrl.ClientID + "'," + fceCall + ");"));
                    }
                }

                #endregion


                #region "OkControlID"

                // Check whether PopupControl ID is defined
                if (!String.IsNullOrEmpty(OkControlID))
                {
                    // Try find control
                    Control ctrl = Page.FindControl(OkControlID);
                    // If control was not found throw an exception
                    if (ctrl == null)
                    {
                        throw new Exception("[ModalPopupDialog] OK control with ID '" + OkControlID + "' could not be found.");
                    }
                    // If control exist, register click event
                    else
                    {
                        ScriptHelper.RegisterStartupScript(Page, typeof(string), ClientID + "OkControl", ScriptHelper.GetScript("RegisterClickHandler('" + ctrl.ClientID + "',function() { hideModalPopup('" + ClientID + "','" + backgroundId + "'); " + OnOkScript + " });"));
                    }
                }

                #endregion


                // Check whether popup should be hidden after click on background
                if (CancelOnBackgroundClick)
                {
                    ScriptHelper.RegisterStartupScript(Page, typeof(string), ClientID + "CancelBackground", ScriptHelper.GetScript("RegisterClickHandler('" + backgroundId + "',function() { hideModalPopup('" + ClientID + "','" + backgroundId + "'); " + OnCancelScript + " });"));
                }

                // Register show script if is required
                if (showRequired)
                {
                    String fceCall = " showModalPopup('" + ClientID + "','" + backgroundId + "');";
                    if (ShowMaximized)
                    {
                        fceCall = " showModalMaximizedPopup('" + ClientID + "','" + backgroundId + "'," + MaxSizedEdgeSpace + (String.IsNullOrEmpty(AdditionalInitHandler) ? "" : "," + AdditionalInitHandler) + ");";
                    }

                    ScriptHelper.RegisterStartupScript(Page, typeof(string), ClientID + "ShowPopup", ScriptHelper.GetScript(fceCall));
                }
                // Register hide script if is required
                else if (hideRequired)
                {
                    ScriptHelper.RegisterStartupScript(Page, typeof(string), ClientID + "HidePopup", ScriptHelper.GetScript("hideModalPopup('" + ClientID + "','" + backgroundId + "');"));
                }

                // Hide current control & set default z-index
                Style.Add("display", "none");
                Style.Add("z-index", "20200");
                Style.Add("position", "fixed");


                // Set background default style
                const string backgroundStyle = "display:none; position: fixed; top: 0; left: 0; z-index: 20100;";

                // Add background control
                writer.Write(@"<div id=""" + backgroundId + @""" style=""" + backgroundStyle + @""" class=""" + BackgroundCssClass + @"""></div>");
            }
            base.Render(writer);
        }

        #endregion
    }
}
