using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for CMS master pages.
    /// </summary>
    public abstract class AbstractMasterPage : MasterPage
    {
        #region "Variables"

        /// <summary>
        /// Body CSS class.
        /// </summary>
        protected string mBodyClass = "";


        /// <summary>
        /// Hashtable with the short ID mappings.
        /// </summary>
        protected static Hashtable mShortIDs = null;


        /// <summary>
        /// Placeholder for messages.
        /// </summary>
        protected MessagesPlaceHolder plcMessages = null;


        /// <summary>
        /// Info label control.
        /// </summary>
        protected Label lblInfo = null;


        /// <summary>
        /// Warning label control.
        /// </summary>
        protected Label lblWarning = null;


        /// <summary>
        /// Error label control.
        /// </summary>
        protected Label lblError = null;


        private bool? mDevelopmentMode = null;

        #endregion


        #region "Inner controls properties"

        /// <summary>
        /// Tabs control.
        /// </summary>
        public virtual UITabs Tabs
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// PageTitle control.
        /// </summary>
        public virtual PageTitle Title
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// HeaderActions control.
        /// </summary>
        public virtual HeaderActions HeaderActions
        {
            get;
            set;
        }


        /// <summary>
        /// Container with header actions menu
        /// </summary>
        public virtual ObjectEditMenu ObjectEditMenu
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// HeaderActionsPermissions place holder.
        /// </summary>
        public virtual UIPlaceHolder HeaderActionsPlaceHolder
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Left tabs panel.
        /// </summary>
        public virtual Panel PanelLeft
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Right tabs panel.
        /// </summary>
        public virtual Panel PanelRight
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Panel containing title.
        /// </summary>
        public virtual Panel PanelTitle
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Panel containing title actions displayed above scrolling content.
        /// </summary>
        public virtual Panel PanelTitleActions
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Body panel.
        /// </summary>
        public virtual Panel PanelBody
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Info panel.
        /// </summary>
        public virtual Panel PanelInfo
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the content panel.
        /// </summary>
        public virtual Panel PanelContent
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the labels container.
        /// </summary>
        public virtual PlaceHolder PlaceholderLabels
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets placeholder located after form element.
        /// </summary>
        public virtual PlaceHolder AfterFormPlaceholder
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Footer panel.
        /// </summary>
        public virtual Panel PanelFooter
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Header panel.
        /// </summary>
        public virtual Panel PanelHeader
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Header container.
        /// </summary>
        public virtual Panel HeaderContainer
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Separator panel.
        /// </summary>
        public virtual Panel PanelSeparator
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Body object.
        /// </summary>
        public virtual HtmlGenericControl Body
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Tabs panel.
        /// </summary>
        public virtual Panel PanelTabs
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Tabs container panel.
        /// </summary>
        public virtual Panel PanelContainer
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Page status container.
        /// </summary>
        public Control PageStatusContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for specifying the additional HEAD elements.
        /// </summary>
        public virtual Literal HeadElements
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for switching to next record (info/details etc.)
        /// </summary>
        public virtual CMSButton ButtonNext
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for switching to previous record (info/details etc.)
        /// </summary>
        public virtual CMSButton ButtonPrevious
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for closing modal window.
        /// </summary>
        public virtual CMSButton ButtonClose
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for exporting.
        /// </summary>
        public virtual HyperLink LinkExport
        {
            get;
            set;
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        public virtual MessagesPlaceHolder MessagesPlaceHolder
        {
            get
            {
                return EnsureMessagesPlaceHolder();
            }
            set
            {
                plcMessages = value;
            }
        }


        /// <summary>
        /// Info label control.
        /// </summary>
        public virtual Label InfoLabel
        {
            get
            {
                return MessagesPlaceHolder.InfoLabel;
            }
            set
            {
                MessagesPlaceHolder.InfoLabel = value;
            }
        }


        /// <summary>
        /// Warning label control.
        /// </summary>
        public virtual Label WarningLabel
        {
            get
            {
                return MessagesPlaceHolder.WarningLabel;
            }
            set
            {
                MessagesPlaceHolder.WarningLabel = value;
            }
        }


        /// <summary>
        /// Error label control.
        /// </summary>
        public virtual Label ErrorLabel
        {
            get
            {
                return MessagesPlaceHolder.ErrorLabel;
            }
            set
            {
                MessagesPlaceHolder.ErrorLabel = value;
            }
        }


        /// <summary>
        /// Container with footer panel.
        /// </summary>
        public virtual Panel FooterContainer
        {
            get
            {
                return null;
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Table of the short IDs [ID -> ShortID]
        /// </summary>
        protected static Hashtable ShortIDs
        {
            get
            {
                if (mShortIDs == null)
                {
                    // Prepare the table with IDs
                    Hashtable ids = new Hashtable();

                    ids.Add("plcContent", "c");
                    ids.Add("plcBeforeBody", "bb");
                    ids.Add("plcControls", "ct");
                    ids.Add("plcActions", "pa");
                    ids.Add("plcBeforeContent", "bc");
                    ids.Add("plcFooter", "f");

                    mShortIDs = ids;
                }

                return mShortIDs;
            }
        }


        /// <summary>
        /// Body class.
        /// </summary>
        public string BodyClass
        {
            get
            {
                return mBodyClass;
            }
            set
            {
                mBodyClass = value;
            }
        }


        /// <summary>
        /// Indicates whether the panel for additional controls should be displayed.
        /// </summary>
        public bool DisplayControlsPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the panel for site selector should be displayed
        /// </summary>
        public bool DisplaySiteSelectorPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the panel for actions should be displayed.
        /// </summary>
        public bool DisplayActionsPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether sparator should be displayed
        /// </summary>
        public bool DisplaySeparatorPanel
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if header actions panel should have ViewState enabled or disabled.
        /// </summary>
        public virtual bool ActionsViewstateEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the page is in tab mode.
        /// </summary>
        public virtual bool TabMode
        {
            get
            {
                return QueryHelper.GetBoolean("tabmode", false);
            }
        }


        /// <summary>
        /// Indicates if page is displayed in development mode.
        /// </summary>
        public virtual bool DevelopmentMode
        {
            get
            {
                if (mDevelopmentMode == null)
                {
                    return SystemContext.DevelopmentMode;
                }
                else
                {
                    return mDevelopmentMode.Value;
                }
            }
            set
            {
                mDevelopmentMode = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Ensures message labels on the page.
        /// </summary>
        protected virtual MessagesPlaceHolder EnsureMessagesPlaceHolder()
        {
            if (plcMessages == null)
            {
                if (PlaceholderLabels == null)
                {
                    throw new NullReferenceException("[AbstractMasterPage.EnsureMessagesPlaceHolder]: PlaceholderLabels property must be overridden to ensure Messages placeholder.");
                }

                plcMessages = new MessagesPlaceHolder { ID = "plcMessages", ShortID = "pM", IsLiveSite = false };
                PlaceholderLabels.Controls.AddAt(0, plcMessages);
            }

            return plcMessages;
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (ControlsExtensions.RenderShortIDs)
            {
                // Shorten the IDs automatically
                SetShortIDs(this);

                // Shorten the ID of the master page
                if ((MasterPageFile == null) && (HttpContext.Current != null) && !DesignMode)
                {
                    ID = "m";
                }
            }

            // Ensure messages placeholder because of update panel
            if (PlaceholderLabels != null)
            {
                EnsureMessagesPlaceHolder();
            }

            base.OnInit(e);
        }


        /// <summary>
        /// Sets the short IDs to the controls.
        /// </summary>
        /// <param name="parent">Parent controls</param>
        protected void SetShortIDs(Control parent)
        {
            // Process all controls
            foreach (Control c in parent.Controls)
            {
                if (c is ContentPlaceHolder)
                {
                    // Set shorter IDs to content placeholders
                    string newId = (string)ShortIDs[c.ID];
                    if (newId != null)
                    {
                        c.ID = newId;
                    }
                }
                else
                {
                    // Recursively set
                    SetShortIDs(c);
                }
            }
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Set visibility of the labels
            if (lblInfo != null)
            {
                lblInfo.Visible = !String.IsNullOrEmpty(lblInfo.Text);
            }
            if (lblWarning != null)
            {
                lblWarning.Visible = !String.IsNullOrEmpty(lblWarning.Text);
            }
            if (lblError != null)
            {
                lblError.Visible = !String.IsNullOrEmpty(lblError.Text);
            }
        }

        #endregion
    }
}