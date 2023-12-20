using System;
using System.Data;
using System.Linq;

using CMS;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("SKUSectionSelectionUniSelectorExtender", typeof(SKUSectionSelectionUniSelectorExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for SKU tab of product document detail
    /// </summary>
    public class SKUSectionSelectionUniSelectorExtender : ControlExtender<UniSelector>
    {
        private const string COLUMN_NAME = "NodeAliasPath";


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.UniGrid.OnExternalDataBound += UniGridDataBound;
            Control.PreRender += OnPreRender;
        }


        private object UniGridDataBound(object sender, string sourceName, object parameter)
        {
            var view = parameter as DataRowView;
            if (view != null)
            {
                var colName = Control.ReturnColumnName;
                var query = DocumentHelper.GetDocuments()
                                          .OnCurrentSite()
                                          .TopN(1)
                                          .Column("DocumentNamePath")
                                          .Culture(CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName))
                                          .CombineWithAnyCulture();

                if (colName == "NodeGUID")
                {
                    var nodeGUID = ValidationHelper.GetGuid(view["NodeGUID"], Guid.Empty);
                    query = query.WhereEquals("NodeGUID", nodeGUID);
                }
                else
                {
                    var nodeID = ValidationHelper.GetInteger(view["NodeID"], 0);
                    query = query.WhereEquals("NodeID", nodeID);
                }

                var doc = query.FirstOrDefault();

                if (doc != null)
                {
                    view[COLUMN_NAME] = HTMLHelper.HTMLEncode(doc.DocumentNamePath.TrimStart('/').Replace("/", " > "));
                }
            }

            return parameter;
        }


        private void OnPreRender(object sender, EventArgs e)
        {
            var headerRow = Control.UniGrid.GridView.HeaderRow;
            if (headerRow != null && headerRow.Cells.Count >= 1)
            {
                headerRow.Cells[1].Text = ResHelper.GetString("com.productsection");
            }
        }
    }
}