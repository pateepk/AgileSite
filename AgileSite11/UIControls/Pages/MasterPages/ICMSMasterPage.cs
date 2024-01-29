using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.PortalEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Interface for CMS master pages.
    /// </summary>
    public interface ICMSMasterPage
    {
        #region "Properties"

        /// <summary>
        /// Page.
        /// </summary>
        Page Page
        {
            get;
        }


        /// <summary>
        /// Body class.
        /// </summary>
        string BodyClass
        {
            get;
            set;
        }


        /// <summary>
        /// Tabs control.
        /// </summary>
        UITabs Tabs
        {
            get;
        }


        /// <summary>
        /// PageTitle control.
        /// </summary>
        PageTitle Title
        {
            get;
        }


        /// <summary>
        /// Container with header actions menu
        /// </summary>
        ObjectEditMenu ObjectEditMenu
        {
            get;
        }


        /// <summary>
        /// HeaderActions control.
        /// </summary>
        HeaderActions HeaderActions
        {
            get;
            set;
        }


        /// <summary>
        /// HeaderActionsPermissions place holder.
        /// </summary>
        UIPlaceHolder HeaderActionsPlaceHolder
        {
            get;
        }


        /// <summary>
        /// Left tabs panel.
        /// </summary>
        Panel PanelLeft
        {
            get;
        }


        /// <summary>
        /// Right tabs panel.
        /// </summary>
        Panel PanelRight
        {
            get;
        }


        /// <summary>
        /// Panel containig title.
        /// </summary>
        Panel PanelTitle
        {
            get;
        }


        /// <summary>
        /// Tabs panel container panel.
        /// </summary>
        Panel PanelContainer
        {
            get;
        }


        /// <summary>
        /// Panel containing title actions disaplyed above scrolling content.
        /// </summary>
        Panel PanelTitleActions
        {
            get;
        }


        /// <summary>
        /// Body panel.
        /// </summary>
        Panel PanelBody
        {
            get;
        }


        /// <summary>
        /// Info panel.
        /// </summary>
        Panel PanelInfo
        {
            get;
        }


        /// <summary>
        /// Content panel.
        /// </summary>
        Panel PanelContent
        {
            get;
        }


        /// <summary>
        /// Header panel.
        /// </summary>
        Panel PanelHeader
        {
            get;
        }


        /// <summary>
        /// Header container
        /// </summary>
        Panel HeaderContainer
        {
            get;
        }


        /// <summary>
        /// Footer panel.
        /// </summary>
        Panel PanelFooter
        {
            get;
        }


        /// <summary>
        /// Body element.
        /// </summary>
        HtmlGenericControl Body
        {
            get;
        }


        /// <summary>
        /// Page status container.
        /// </summary>
        Control PageStatusContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        MessagesPlaceHolder MessagesPlaceHolder
        {
            get;
            set;
        }


        /// <summary>
        /// Information label.
        /// </summary>
        Label InfoLabel
        {
            get;
            set;
        }


        /// <summary>
        /// Warning label.
        /// </summary>
        Label WarningLabel
        {
            get;
            set;
        }


        /// <summary>
        /// Error label.
        /// </summary>
        Label ErrorLabel
        {
            get;
            set;
        }


        /// <summary>
        /// Indicated whether the panel for additional controls should be displayed.
        /// </summary>
        bool DisplayControlsPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Indicated whether the panel for site selector should be displayed
        /// </summary>
        bool DisplaySiteSelectorPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Indicated whether the panel for actions should be displayed.
        /// </summary>
        bool DisplayActionsPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for specifying the additional HEAD elements.
        /// </summary>
        Literal HeadElements
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for switching to next record (info/details etc.)
        /// </summary>
        CMSButton ButtonNext
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for switching to previous record (info/details etc.)
        /// </summary>
        CMSButton ButtonPrevious
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for closing modal window.
        /// </summary>
        CMSButton ButtonClose
        {
            get;
            set;
        }


        /// <summary>
        /// Prepared for exporting.
        /// </summary>
        HyperLink LinkExport
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if header actions panel should have ViewState enabled or disabled.
        /// </summary>
        bool ActionsViewstateEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Separator panel.
        /// </summary>
        Panel PanelSeparator
        {
            get;
        }


        /// <summary>
        /// Indicates if page is displayed in development mode.
        /// </summary>
        bool DevelopmentMode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the labels container.
        /// </summary>
        PlaceHolder PlaceholderLabels
        {
            get;
        }


        /// <summary>
        /// Gets placeholder located after form element.
        /// </summary>
        PlaceHolder AfterFormPlaceholder
        {
            get;
        }


        /// <summary>
        /// Footer container.
        /// </summary>
        Panel FooterContainer
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the RTL culture to the body class if RTL language.
        /// </summary>
        void SetRTL();


        /// <summary>
        /// Sets the browser class to the body class.
        /// </summary>
        void SetBrowserClass();

        #endregion
    }
}