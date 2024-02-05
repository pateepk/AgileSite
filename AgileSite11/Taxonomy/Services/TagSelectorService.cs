using System.Web;
using System.Data;
using System.Web.Script.Services;
using System.Web.Services;

using CMS.Helpers;
using CMS.DataEngine;

namespace CMS.Taxonomy
{
    /// <summary>
    /// Summary description for TagSelectorService.
    /// </summary>
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [WebService(Namespace = "CMS.WebService")]
    [ScriptService]
    public class TagSelectorService : WebService
    {
        /// <summary>
        /// Gets results for tag auto-complete
        /// </summary>
        /// <param name="prefixText">Prefix to be searched</param>
        /// <param name="count">Number of tags to be returned</param>
        /// <param name="contextKey">Tag group ID</param>
        [WebMethod]
        [ScriptMethod]
        public string[] TagsAutoComplete(string prefixText, int count, string contextKey)
        {
            WhereCondition condition = new WhereCondition().WhereStartsWith("TagName", prefixText);
            if (contextKey != null)
            {
                condition.WhereEquals("TagGroupID", contextKey);
            }

            DataSet ds = TagInfoProvider.GetTags()
                                        .TopN(20)
                                        .Column("TagName")
                                        .Where(condition)
                                        .OrderBy("TagName");
            
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                string[] output = new string[ds.Tables[0].Rows.Count];

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i]["TagName"].ToString().Contains(" "))
                    {
                        output[i] = "\"" + HttpUtility.HtmlDecode(ds.Tables[0].Rows[i]["TagName"].ToString()) + "\"";
                    }
                    else
                    {
                        output[i] = HttpUtility.HtmlDecode(ds.Tables[0].Rows[i]["TagName"].ToString());
                    }
                }

                return output;
            }

            return null;
        }
    }
}