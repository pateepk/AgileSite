using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.UIControls;
using CMS.WorkflowEngine.Web.UI;


[assembly: RegisterCustomClass("WorkflowScopeListControlExtender", typeof(WorkflowScopeListControlExtender))]

namespace CMS.WorkflowEngine.Web.UI
{
    /// <summary>
    /// Permission edit control extender
    /// </summary>
    public class WorkflowScopeListControlExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// OnInit event handler
        /// </summary>
        public override void OnInit()
        {
            Control.OnExternalDataBound += OnExternalDataBound;
            Control.OnBeforeDataReload += () => Control.NamedColumns["culture"].Visible = LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Multilingual);
        }


        /// <summary>
        /// OnExternalDataBound event handler
        /// </summary>
        protected object OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName.ToLowerCSafe())
            {
                case "aliaspath":
                    return TreePathUtils.EnsureSingleNodePath((string)parameter);

                case "classdisplayname":
                    string docType = ValidationHelper.GetString(parameter, "");
                    if (docType == "")
                    {
                        return Control.GetString("general.selectall");
                    }
                    return HTMLHelper.HTMLEncode(docType);

                case "scopecultureid":
                    int cultureId = ValidationHelper.GetInteger(parameter, 0);
                    if (cultureId > 0)
                    {
                        return CultureInfoProvider.GetCultureInfo(cultureId).CultureName;
                    }
                    else
                    {
                        return Control.GetString("general.selectall");
                    }

                case "scopeexcluded":
                {
                    bool allowed = !ValidationHelper.GetBoolean(parameter, false);
                    return UniGridFunctions.ColoredSpanAllowedExcluded(allowed);
                }

                case "coverage":
                {
                    DataRowView drv = (DataRowView)parameter;
                    string alias = ValidationHelper.GetString(drv.Row["ScopeStartingPath"], "");
                    bool children = !ValidationHelper.GetBoolean(drv.Row["ScopeExcludeChildren"], false);

                    // Only child documents
                    if (alias.EndsWithCSafe("/%"))
                    {
                        return Control.GetString("workflowscope.children");
                    }
                    // Only document
                    if (!children)
                    {
                        return Control.GetString("workflowscope.doc");
                    }
                    // Document including children
                    return Control.GetString("workflowscope.docandchildren");
                }

                default:
                    return parameter;
            }
        }
    }
}