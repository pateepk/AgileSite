using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Class for listing categories.
    /// </summary>
    [ToolboxData("<{0}:CategoryListPanel runat=server></{0}:CategoryListPanel>")]
    public class CategoryListPanel : PlaceHolder
    {
        #region "Methods"

        /// <summary>
        /// Output routine.
        /// </summary>
        /// <param name="writer">Text writer for rendering</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CategoryListPanel: " + ID + "]");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div class=\"{0}\"><ul>", "ReportFormCategoryList");

            // Take collection of categories from memory       
            var panels = ControlsHelper.GetControlsOfType<CategoryPanel>(this.Parent);
            if (panels != null)
            {
                foreach (CategoryPanel cp in panels)
                {
                    sb.AppendFormat("<li><a href=\"#{0}\">{1}</a></li>", cp.Identifier, cp.Text);
                }
            }

            sb.Append("</ul></div>");

            writer.Write(sb.ToString());

            base.Render(writer);
        }

        #endregion
    }
}