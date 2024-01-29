using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.PortalEngine.Web.UI;
using CMS.Search.Web.UI;
using CMS.SiteProvider;

[assembly: RegisterCustomClass(nameof(SearchIndexNewFormExtender), typeof(SearchIndexNewFormExtender))]

namespace CMS.Search.Web.UI
{
    /// <summary>
    /// Extender class for new Smart search index form (for both Azure and local).
    /// </summary>
    public class SearchIndexNewFormExtender : ControlExtender<UIForm>
    {        
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.OnAfterSave += AddDefaultCulture;
        }


        /// <summary>
        /// Sets culture based on default culture of the site.
        /// </summary>
        private void AddDefaultCulture(object sender, EventArgs e)
        {
            var indexInfo = Control.EditedObject as SearchIndexInfo;
            if ((indexInfo == null) || ((indexInfo.IndexType != SearchHelper.DOCUMENTS_CRAWLER_INDEX) && (indexInfo.IndexType != TreeNode.OBJECT_TYPE)))
            {
                return;
            }

            var assignToSiteControl = Control.FieldControls["AssignToSite"];
            if (assignToSiteControl != null && ValidationHelper.GetBoolean(assignToSiteControl.Value, false))
            {
                int siteId = assignToSiteControl.GetResolvedValue<int>("TargetObjectID", 0);
                string siteName = SiteInfoProvider.GetSiteName(siteId);
                if (!String.IsNullOrEmpty(siteName))
                {
                    var cultureCode = CultureHelper.GetDefaultCultureCode(siteName);
                    var cultureId = CultureInfoProvider.GetCultureID(cultureCode);
                    SearchIndexCultureInfoProvider.AddSearchIndexCulture(indexInfo.IndexID, cultureId);
                }
            }
        }
    }
}
