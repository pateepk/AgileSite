using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;

namespace NHG_C
{
    public partial class BlueKey_CMSWebParts_FiledUnderBlogFilter : CMSAbstractBaseFilterControl
    {
        public bool FilterByQuery = true;

        protected void Page_Load(object sender, EventArgs e)
        {
            SetFilter();
        }

        private void SetFilter()
        {

            int tagId = QueryHelper.GetInteger("tagid", -1);
            string tagName = QueryHelper.GetString("tagname", "");
            int groupId = QueryHelper.GetInteger("groupid", -1);

            string strWhere = string.Empty;

            if (tagId > 0 && tagName == "" && groupId < 0)
            {
                strWhere = @"(DocumentID IN (SELECT DocumentID FROM CMS_DocumentTag WHERE TagID = " + tagId + "))";
            }

            if (tagId < 0 && tagName != "" && groupId > 0)
            {
                strWhere = @"(DocumentID IN (SELECT DocumentID FROM CMS_DocumentTag dt INNER JOIN CMS_Tag t on dt.TagID = t.TagID WHERE (t.TagName = '" + tagName + "' AND t.TagGroupID = " + groupId + ")))";
            }
            //string strWhere = @"(DocumentID IN (SELECT DocumentID FROM CMS_DocumentTag WHERE (TagID = " + tagId + "@tagId = -1) AND (@tagName = '' AND @groupId = -1)))";
            //strWhere += @" OR (DocumentID IN (SELECT DocumentID FROM CMS_DocumentTag dt INNER JOIN CMS_Tag t on dt.TagID = t.TagID WHERE ((t.TagCodeName = @tagName AND t.TagGroupID = @groupId) OR (@tagName = '' AND @groupId = -1)) AND (@tagId = -1)))";

            WhereCondition = strWhere;

            this.RaiseOnFilterChanged();
        }
    }
}