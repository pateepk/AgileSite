using System;
using System.Data;
using System.Linq;

using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Provides handlers for controls backward compatibility
    /// </summary>
    static internal class BackwardCompatibilityHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            DocumentEngineWebUIEvents.TransformationEval.Execute += TransformationEval_Execute;
        }


        private static void TransformationEval_Execute(object sender, TransformationEvalEventArgs e)
        {
            // Backward compatibility for transformations
            // SiteName column was removed from the main view for documents
            if (e.ColumnName.EqualsCSafe("sitename", true))
            {
                var data = e.Data as DataRowView;
                if (data == null)
                {
                    return;
                }

                var siteId = DataHelper.GetDataRowViewValue(data, "NodeSiteID").ToInteger(0);
                if (siteId <= 0)
                {
                    return;
                }

                e.HasValue = true;
                e.Value = SiteInfoProvider.GetSiteName(siteId);
            }
        }
    }
}