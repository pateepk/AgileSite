using System;
using System.ComponentModel;
using System.Text;
using System.Web.Script.Serialization;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Internal class printing the graph in its own pane. 
    /// This control should be wrapped in another for proper usage.
    /// </summary>
    [DefaultProperty("ID")]
    internal class UniGraphPane : CMSPanel
    {
        #region "Variables"

        /// <summary>
        /// Name of JavaScript object representing the graph.
        /// </summary>
        private string mJsObjectName = null;


        /// <summary>
        /// Name of service - default setting
        /// </summary>
        private string mSavingServiceName = "undefined";

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of JavaScript object representing the graph.
        /// </summary>
        public string JsObjectName
        {
            get
            {
                if(mJsObjectName == null)
                {
                    mJsObjectName = "GraphObject_" + ClientID;
                }
                return mJsObjectName;
            }
        }


        /// <summary>
        /// Contains configuration of graph and it's components.
        /// </summary>
        public Graph GraphConfiguration 
        { 
            get;
            set;
        }


        /// <summary>
        /// Defines whether or not is graph read only.
        /// </summary>
        public bool ReadOnly 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Name of JavaScript object of service used for saving graph.
        /// </summary>
        public string JsSavingServiceName
        {
            get
            {
                return mSavingServiceName;
            }
            set
            {
                mSavingServiceName = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Handler that prints the graph and it's components.
        /// </summary>
        public void Print()
        {
            SetHtmlProperties();
            RegisterJsFiles();
            PrintGraph();
        }


        /// <summary>
        /// Sets default properties of this object.
        /// </summary>
        private void SetHtmlProperties()
        {
            if (String.IsNullOrEmpty(CssClass))
            {
                CssClass = "Graph";
            }
            if (ReadOnly)
            {
                CssClass += " ReadOnly";
            }
        }


        /// <summary>
        /// Registers all needed JS files
        /// </summary>
        public void RegisterJsFiles()
        {
            ScriptHelper.RegisterJQuery(Page);
            ScriptHelper.RegisterJQueryUI(Page);
            ScriptHelper.RegisterDialogScript(Page);
            ScriptHelper.RegisterJQueryTools(Page);
            ScriptHelper.RegisterApplicationConstants(Page);
            ScriptHelper.RegisterJQueryCookie(Page);

            if (GraphConfiguration != null)
            {
                GraphConfiguration.JsFiles.ForEach(f => ScriptHelper.RegisterScriptFile(Page, f, false));
            }

            CssRegistration.RegisterCssLink(Page, Page.Theme, "CMSDesk.css");
            CssRegistration.RegisterCssLink(Page, "Design", "UniGraph.css");
            CssRegistration.RegisterDesignMode(Page);
        }


        /// <summary>
        /// Method to print the graph.
        /// </summary>
        protected void PrintGraph()
        {
            if (GraphConfiguration != null)
            {
                PrepareConditionTooltips();
                JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();
                string graphJson = JsonSerializer.Serialize(GraphConfiguration);
                string sourcePointsLimits = JsonSerializer.Serialize(NodeSourcePointsLimits.GetSerializableObject());
                GraphInitialization(graphJson, sourcePointsLimits);
            }
        }


        /// <summary>
        /// Prepares tooltips of source points of steps.
        /// </summary>
        private void PrepareConditionTooltips()
        {
            string condition = ResHelper.GetString("graph.conditionmacrotooltip");
            GraphConfiguration.Nodes.ForEach
            (
                node => node.SourcePoints.ForEach
                (
                    sourcepoint =>
                    {
                        var cond = ResolveCondition(sourcepoint.Tooltip);
                        if (!string.IsNullOrEmpty(cond))
                        {
                            sourcepoint.Tooltip = condition + cond;
                        }
                    }
                )
            );
        }


        /// <summary>
        /// Resolves given conditions.
        /// </summary>
        /// <param name="condition">Condition macro</param>
        /// <returns>Resolved condition</returns>
        private string ResolveCondition(string condition)
        {
            return MacroRuleTree.GetRuleText(ValidationHelper.GetString(condition, String.Empty));
        }


        /// <summary>
        /// Registers graph initialization script.
        /// </summary>
        /// <param name="graphJson">JavaScript object defining the graph.</param>
        /// <param name="soucePointsLimits">Limits for source points of nodes.</param>
        private void GraphInitialization(string graphJson, string soucePointsLimits)
        {
            StringBuilder initScript = new StringBuilder();

            initScript.Append(String.Format("var {0} = new JsPlumbGraph({1}, {2}, '{3}', '{0}');", JsObjectName, ReadOnly.ToString().ToLowerCSafe(), JsSavingServiceName, ClientID));
            initScript.Append(String.Format("{0}.setSourcePointsLimits({1});", JsObjectName, soucePointsLimits));
            initScript.Append(String.Format("{0}.printGraph({1});", JsObjectName, graphJson));

            ScriptHelper.RegisterStartupScript(this, typeof(string), ClientID, ScriptHelper.GetScript(initScript.ToString()));
        }

        #endregion
    }
}
