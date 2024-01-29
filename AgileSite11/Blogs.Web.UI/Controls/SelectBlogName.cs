using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Form control for the blog selection.
    /// </summary>
    [ToolboxData("<{0}:SelectBlogName runat=server></{0}:SelectBlogName>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectBlogName : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectBlogName()
        {
            FormControlName = "BlogNameSelector";
        }
    }
}
