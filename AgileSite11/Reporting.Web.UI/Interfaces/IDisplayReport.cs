using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Interface for DisplayReport
    /// </summary>
    public interface IDisplayReport
    {
        #region "Properties"

        /// <summary>
        /// Colors assigned to series.
        /// </summary>
        Dictionary<string, Color> Colors
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether progress indicator should be used.
        /// </summary>
        bool UseProgressIndicator
        {
            get;
            set;
        }


        /// <summary>
        /// Content panel.
        /// </summary>
        Panel ReportPanel
        {
            get;
        }


        /// <summary>
        /// Inidcates if CSS classes should be rendered.
        /// </summary>
        bool RenderCssClasses
        {
            get;
            set;
        }


        /// <summary>
        /// Graph possible width of control.
        /// </summary>
        int AreaMaxWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Report's subscription site ID (used in automatic subscription report generation)
        /// </summary>
        int ReportSubscriptionSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, reload is not called from this control.
        /// </summary>
        bool UseExternalReload
        {
            get;
            set;
        }

        /// <summary>
        /// First width of the column
        /// </summary>
        Unit TableFirstColumnWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Special macros used for report macro resolving
        /// </summary>
        string[,] DynamicMacros
        {
            get;
            set;
        }


        /// <summary>
        /// Returns report display name
        /// </summary>
        string ReportDisplayName
        {
            get;
        }


        /// <summary>
        /// Report name.
        /// </summary>
        string ReportName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if check inner sub controls.
        /// </summary>
        bool CheckInnerControls
        {
            get;
            set;
        }


        /// <summary>
        /// ReportHTML.
        /// </summary>
        string ReportHTML
        {
            get;
            set;
        }


        /// <summary>
        /// Display filter.
        /// </summary>
        bool DisplayFilter
        {
            get;
            set;
        }


        /// <summary>
        /// Body CSS class.
        /// </summary>
        string BodyCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Form CSS class.
        /// </summary>
        string FormCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Assigned if parameters will be loaded automaticcaly.
        /// </summary>
        bool LoadFormParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Graph type selected interval
        /// </summary>        
        String SelectedInterval
        {
            get;
            set;
        }


        /// <summary>
        /// Load always default values.
        /// </summary>
        bool ForceLoadDefaultValues
        {
            get;
            set;
        }


        /// <summary>
        /// Reloads parameters even if were already inited.
        /// </summary>
        bool IgnoreWasInit
        {
            get;
            set;
        }


        /// <summary>
        /// Child report controls.
        /// </summary>
        ArrayList ReportControls
        {
            get;
        }


        /// <summary>
        /// Width of chart image in percent versus displayable zone.
        /// </summary>
        int GraphImageWidth
        {
            get;
            set;
        }


        /// <summary>
        /// If true, control is rendered for email.
        /// </summary>
        bool EmailMode
        {
            get;
            set;
        }


        /// <summary>
        /// Report parameters.
        /// </summary>
        DataRow ReportParameters
        {
            get;
            set;
        }


        /// <summary>
        /// If true, only reports with non empty datasource is sent (used in subscriptions)
        /// </summary>
        bool SendOnlyNonEmptyDataSource
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the RTL culture to the body class if RTL language.
        /// </summary>
        void ReloadData(bool forceLoad);


        /// <summary>
        /// Saves the report
        /// </summary>
        int SaveReport();


        /// <summary>
        /// Sets all default macros needed for graphs
        /// </summary>
        /// <param name="interval">Interval type</param>
        void SetDefaultDynamicMacros(int interval);


        /// <summary>
        /// Sets all default macros needed for graphs
        /// </summary>
        /// <param name="interval">Interval type</param>
        void SetDefaultDynamicMacros(String interval);


        /// <summary>
        /// Returns true if report is loaded
        /// </summary>
        bool IsReportLoaded();


        /// <summary>
        /// Renders control to String representation
        /// </summary>
        /// <param name="SiteID">This SiteID is used in report query instead of default CMSContext one</param>
        String RenderToString(int SiteID);

        #endregion
    }
}