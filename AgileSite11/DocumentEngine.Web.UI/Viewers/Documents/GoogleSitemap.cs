using System;
using System.ComponentModel;
using System.Web.UI;
using System.Data;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Control for rendering google sitemap specific XML.
    /// </summary>
    [ToolboxItem(false)]
    public class GoogleSitemap : CMSAbstractMenuProperties
    {
        #region "Variables"

        private string mTransformationName = String.Empty;
        private Repeater mItemsRepeater;
        private bool mHideChildrenForHiddenParent = true;

        #endregion


        #region "Google Sitemap properties"

        /// <summary>
        /// Indicates whether sitemap index transformations should be used
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether sitemap index transformations should be used")]
        public bool IsSiteMapIndex
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether exclude from search option should be ignored for sitemap
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether exclude from search option should be ignored for sitemap")]
        public bool IgnoreExcludeFromSearch
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether children should be hidden if parent is not accessible. True by default.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates whether children should be hidden if parent is not accessible.")]
        public bool HideChildrenForHiddenParent
        {
            get
            {
                return mHideChildrenForHiddenParent;
            }
            set
            {
                mHideChildrenForHiddenParent = value;
            }
        }


        /// <summary>
        /// Property to set and get name of transformation for displaying results. If none is set, default transformation is used.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Name of transformation which will be used for displaying results. If none is set, default transformation is used.")]
        public string TransformationName
        {
            get
            {
                return mTransformationName;
            }
            set
            {
                mTransformationName = value;
            }
        }


        /// <summary>
        /// Default item template for sitemap index.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate IndexItemTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Header item template for sitemap index.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate IndexHeaderTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Footer item template for sitemap index.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate IndexFooterTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Default item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate ItemTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Header item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate HeaderTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Footer item template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(RepeaterItem)), DefaultValue((string)null), Browsable(false)]
        public virtual ITemplate FooterTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Repeater that renders site map items.
        /// </summary>
        private Repeater ItemsRepeater
        {
            get
            {
                if (mItemsRepeater == null)
                {
                    mItemsRepeater = new Repeater();
                    Controls.Add(mItemsRepeater);
                }

                return mItemsRepeater;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Reloads data.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            ReloadData(false);
        }


        /// <summary>
        /// Renders output without  begin/end tags.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            foreach (Control childControl in Controls)
            {
                childControl.RenderControl(writer);
            }
        }


        /// <summary>
        /// Returns DataSet of items under specified parent node.
        /// </summary>
        /// <param name="parentNodeId">Parent Node ID</param>
        /// <param name="parentCultureCode">Culture code of parent element. Must match element's culture to add element to collection. Otherwise element is ignored.</param>
        private DataSet GetItems(int parentNodeId, string parentCultureCode)
        {
            // Return regular datasource if hiding of sub items is not required
            if (!HideChildrenForHiddenParent)
            {
                return DataSource;
            }

            // Get items
            var items = GroupedDS.GetGroupView(parentNodeId);
            if (!DataHelper.DataSourceIsEmpty(items))
            {
                // Create result dataset
                DataSet ds = new DataSet();

                // Copy source table structure to result table
                var dt = items[0].Row.Table.Clone();

                // Copy all items
                foreach (DataRowView dr in items)
                {
                    String culture = Convert.ToString(dr["DocumentCulture"]);

                    // Special handle for ##ALL## macro. Add only pages with the same culture. Different culture will be added in its own "thread".
                    // There has to be check for ##ALL## because of the setting 'Compare with default culture'. If this setting is checked (and only one culture selected), it's necessary to add different cultures in one recursion.
                    if (!CultureCode.EqualsCSafe("##all##", true) || (parentCultureCode == null) || (culture == parentCultureCode))
                    {
                        dt.Rows.Add(dr.Row.ItemArray);

                        // Copy children of items
                        AppendDataSet(dt, GetItems(Convert.ToInt32(dr["NodeID"]), Convert.ToString(dr["DocumentCulture"])));
                    }
                }

                ds.Tables.Add(dt);

                return ds;
            }

            return null;
        }


        /// <summary>
        /// Copies DataSet to the end of DataTable.
        /// </summary>
        private void AppendDataSet(DataTable baseDT, DataSet appendDS)
        {
            if ((baseDT != null) && (appendDS != null))
            {
                foreach (DataRow dr in appendDS.Tables[0].Rows)
                {
                    baseDT.Rows.Add(dr.ItemArray);
                }
            }
        }


        /// <summary>
        /// Reloads control data.
        /// </summary>
        /// <param name="forceLoad">Indicates force load</param>
        public override void ReloadData(bool forceLoad)
        {
            // Check whether processing is enabled
            if (!StopProcessing)
            {
                // Set envelope template
                ItemsRepeater.HeaderTemplate = HeaderTemplate;
                ItemsRepeater.FooterTemplate = FooterTemplate;

                // Set envelope templates for index if required
                if (IsSiteMapIndex)
                {
                    ItemsRepeater.HeaderTemplate = IndexHeaderTemplate;
                    ItemsRepeater.FooterTemplate = IndexFooterTemplate;
                }

                // If transformation name is not specified use Item template or Index item template
                if (String.IsNullOrEmpty(TransformationName))
                {
                    ItemsRepeater.ItemTemplate = ItemTemplate;
                    if (IsSiteMapIndex)
                    {
                        ItemsRepeater.ItemTemplate = IndexItemTemplate;
                    }
                }
                else
                {
                    ItemsRepeater.ItemTemplate = TransformationHelper.LoadTransformation(this, TransformationName);
                }

                // keep original where condition
                string originalWhere = WhereCondition;

                // Do not include document excluded from search if required
                if (!IgnoreExcludeFromSearch)
                {
                    WhereCondition = SqlHelper.AddWhereCondition("ISNULL(DocumentSearchExcluded , 0) != 1", WhereCondition);
                }

                // Get data
                DataSource = GetDataSource(true);

                // Restore original where condition
                WhereCondition = originalWhere;

                // Check whether data source isn't empty
                if (!DataHelper.DataSourceIsEmpty(DataSource))
                {
                    // Bind data
                    int parentId = ValidationHelper.GetInteger(DataSource.Tables[0].DefaultView[0]["NodeParentID"], 0);
                    ItemsRepeater.DataSource = GetItems(parentId, null);
                    ItemsRepeater.DataBind();
                }
            }
        }

        #endregion
    }
}