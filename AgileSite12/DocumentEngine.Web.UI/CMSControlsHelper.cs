using System;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMS controls helper methods.
    /// </summary>
    public static class CMSControlsHelper
    {
        #region "Variables"

        private static string mEditDocumentScript;

        #endregion


        #region "Properties"

        /// <summary>
        /// Script code for the dialog handling.
        /// </summary>
        public static string EditDocumentScript
        {
            get
            {
                if (mEditDocumentScript != null)
                {
                    return mEditDocumentScript;
                }

                string script = String.Format(@"
function NewDocument(parentNodeId, classId){{ 
	if (parent != window) {{ 
		parent.NewDocument(parentNodeId, classId); 
	}} else {{ 
		window.top.document.location = '{0}/Admin/CMSAdministration.aspx?action=new&nodeid=' + parentNodeId + '&classid=' + classId +'" + ApplicationUrlHelper.GetApplicationHash("cms.content", "content") + @"'; 
	}} 
}}
function NotAllowed(action){{ 
	if (parent != window) {{ 
		parent.NotAllowed('{0}', action); 
	}} else {{ 
		window.top.document.location = '{0}/Admin/CMSAdministration.aspx?action=notallowed&subaction=' + action + '" + ApplicationUrlHelper.GetApplicationHash("cms.content", "content") + @"'; 
	}} 
}}
function DeleteDocument(nodeId) {{ 
	if (parent != window) {{ 
		parent.DeleteDocument(nodeId); 
	}} else {{ 
		window.top.document.location = '{0}/Admin/CMSAdministration.aspx?action=delete&nodeid=' + nodeId + '" + ApplicationUrlHelper.GetApplicationHash("cms.content", "content") + @"'; 
	}} 
}}
function EditDocument(nodeId, tab) {{ 
	if (parent != window) {{ 
		parent.EditDocument(nodeId, tab); 
	}} else {{ 
		window.top.document.location = '{0}/Admin/CMSAdministration.aspx?action=edit&nodeid=' + nodeId + '" + ApplicationUrlHelper.GetApplicationHash("cms.content", "content") + @"'; 
	}} 
}}", SystemContext.ApplicationPath.TrimEnd('/'));

                return ScriptHelper.GetScript(script);
            }
            set
            {
                mEditDocumentScript = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns filter control from filter collection.
        /// </summary>
        /// <param name="filterName">Filter name</param>
        public static Control GetFilter(string filterName)
        {
            if (!String.IsNullOrEmpty(filterName))
            {
                return BaseControlsContext.CurrentFilters[filterName];
            }
            return null;
        }


        /// <summary>
        /// Sets filter control to filter collection
        /// </summary>
        /// <param name="filterName">Filter name</param>
        /// <param name="filterControl">Filter control</param>
        public static void SetFilter(string filterName, Control filterControl)
        {
            if (!String.IsNullOrEmpty(filterName) && (filterControl != null))
            {
                BaseControlsContext.CurrentFilters[filterName] = filterControl;
            }
        }


        /// <summary>
        /// Converts control layout string to enumeration.
        /// </summary>
        /// <param name="layout">Filter mode</param>        
        public static ControlLayoutEnum GetControlLayoutEnum(string layout)
        {
            switch (layout)
            {
                case "vertical":
                    return ControlLayoutEnum.Vertical;

                default:
                    return ControlLayoutEnum.Horizontal;
            }
        }


        /// <summary>
        /// Converts control layout enumeration to string.
        /// </summary>
        /// <param name="layout">Filter mode</param>        
        public static string GetControlLayoutString(ControlLayoutEnum layout)
        {
            switch (layout)
            {
                case ControlLayoutEnum.Vertical:
                    return "vertical";

                default:
                    return "horizontal";
            }
        }


        /// <summary>
        /// Gets the tag used for the controls envelope
        /// </summary>
        public static HtmlTextWriterTag GetControlTagKey()
        {
            if (SiteContext.CurrentSite == null)
            {
                return HtmlTextWriterTag.Span;
            }

            if (SettingsKeyInfoProvider.GetValue("CMSControlElement", SiteContext.CurrentSiteName).ToLowerInvariant().Trim() == "div")
            {
                return HtmlTextWriterTag.Div;
            }

            return HtmlTextWriterTag.Span;
        }

        #endregion
    }
}
