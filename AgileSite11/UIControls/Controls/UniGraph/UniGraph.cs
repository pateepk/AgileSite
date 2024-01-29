using System;
using System.ComponentModel;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;

namespace CMS.UIControls
{
    /// <summary>
    /// UniGraph base class.
    /// </summary>
    [DefaultProperty("ID")]
    [ToolboxData("<{0}:UniGraph runat=server></{0}:UniGraph>")]
    public class UniGraph : CMSPanel
    {
        #region "Variables"

        /// <summary>
        /// Panel used to print the graph.
        /// </summary>
        private UniGraphPane mGraphPrintingPane = new UniGraphPane();

        /// <summary>
        /// Name of JavaScript object representing the graph.
        /// </summary>
        private string mJsObjectName = null;

        /// <summary>
        /// ID of selected node.
        /// </summary>
        private string mSelectedNodeID = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of JavaScript object representing the graph.
        /// </summary>
        public string JsObjectName
        {
            get
            {
                if (mJsObjectName == null)
                {
                    mJsObjectName = "GraphObject_" + ClientID;
                }
                return mJsObjectName;
            }
        }

        /// <summary>
        /// Contains definition of graph and it's components.
        /// </summary>
        public Graph GraphConfiguration
        {
            get
            {
                return mGraphPrintingPane.GraphConfiguration;
            }
            set
            {
                mGraphPrintingPane.GraphConfiguration = value;
            }
        }

        /// <summary>
        /// Node that will be in the middle of wrapped area.
        /// </summary>
        public string SelectedNodeID
        {
            get
            {
                return mSelectedNodeID;
            }
            set
            {
                mSelectedNodeID = value;
            }
        }


        /// <summary>
        /// Defines whether or not is graph read only.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return mGraphPrintingPane.ReadOnly;
            }
            set
            {
                mGraphPrintingPane.ReadOnly = value;
            }
        }


        /// <summary>
        /// Name of JavaScript object of service used for saving graph.
        /// </summary>
        public string JsSavingServiceName
        {
            get
            {
                return mGraphPrintingPane.JsSavingServiceName;
            }
            set
            {
                mGraphPrintingPane.JsSavingServiceName = value;
            }
        }

        #endregion


        #region "Handlers"

        /// <summary>
        /// Event for graph initialization.
        /// </summary>
        /// <param name="e">Event</param>
        protected override void OnInit(EventArgs e)
        {
            Controls.Add(mGraphPrintingPane);
        }


        /// <summary>
        /// Handler that prints the graph and it's components.
        /// </summary>
        /// <param name="e">Event</param>
        protected override void OnPreRender(EventArgs e)
        {
            mGraphPrintingPane.Print();
            SetHtmlProperties();
            WrapperInitialization();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers service used for saving graph state.
        /// </summary>
        public void RegisterService(string url)
        {
            CMSPage page = Page as CMSPage;
            if (page == null)
            {
                throw new Exception("[WorkflowDesigner]: Parent page must be CMSPage.");
            }

            // Register service reference
            ServiceReference service = new ServiceReference(url);
            page.ScriptManagerControl.Services.Add(service);
            JsSavingServiceName = "IWorkflowDesignerService";

            // Save session token from current request
            string token = CookieHelper.GetValue(CookieName.SessionToken);
            if (String.IsNullOrEmpty(token))
            {
                token = Guid.NewGuid().ToString();
                SessionHelper.SetValue(CookieName.SessionToken, token);
            }

            CookieHelper.SetValue(CookieName.SessionToken, token, DateTime.Now.AddYears(1));

            ScriptHelper.FixSSLForWCFServices(page, "IWorkflowDesignerService");
        }


        /// <summary>
        /// Sets default properties of this object.
        /// </summary>
        protected void SetHtmlProperties()
        {
            if (String.IsNullOrEmpty(CssClass))
            {
                CssClass = "GraphContainer";
            }
            if (TabIndex == 0)
            {
                TabIndex = 1;
            }
        }


        /// <summary>
        /// Registers events handling movement of graph pane.
        /// </summary>
        protected void WrapperInitialization()
        {
            StringBuilder initScript = new StringBuilder();

            string size = String.Format("{{ width:{0}, height:{1} }}", Width.IsEmpty.ToString().ToLowerCSafe(), Height.IsEmpty.ToString().ToLowerCSafe());
            initScript.Append(String.Format("var {0} = new JsPlumbGraphWrapper('{1}', '{2}', '{3}', {4}, {5});", JsObjectName, ClientID, JsObjectName, SelectedNodeID, mGraphPrintingPane.JsObjectName, size));

            ScriptHelper.RegisterLoader(Page);
            ScriptHelper.RegisterStartupScript(this, typeof(string), ClientID, ScriptHelper.GetScript(initScript.ToString()));
        }

        #endregion
    }
}

