using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Panel encapsulates the content with table cells to provide the possibility of rich graphical container.
    /// </summary>
    [ToolboxItem(false)]
    public class LayoutPanel : Panel
    {
        private string mLayoutCssClass = "DefaultLayout";

        ///  <summary>
        ///  Base layout CSS class
        ///  </summary>
        public string LayoutCssClass
        {
            get
            {
                return mLayoutCssClass;
            }
            set
            {
                mLayoutCssClass = value;
            }
        }


        ///  <summary>
        ///  Add the layout
        ///  </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }


        ///  <summary>
        ///  Control render action
        ///  </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // Render the layout start
            output.Write(@"<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""" + mLayoutCssClass + @""">" + "\r\n");
            output.Write(@"  <tr class=""Top"">" + "\r\n" + @"<td class=""Left"">&nbsp;</td>" + "\r\n" + @"<td class=""Center"">&nbsp;</td>" + "\r\n" + @"<td class=""Right"">&nbsp;</td>" + "\r\n" + "</tr>" + "\r\n");
            output.Write(@"  <tr class=""Middle"">" + "\r\n" + @"<td class=""Left"">&nbsp;</td>" + "\r\n" + @"<td class=""Center"">");

            // Render the content
            RenderChildren(output);

            // Render the layout end
            output.Write("</td>" + "\r\n" + @"<td class=""Right""></td>" + "\r\n" + "</tr>");
            output.Write(@"  <tr class=""Bottom"">" + "\r\n" + @"<td class=""Left"">&nbsp;</td>" + "\r\n" + @"<td class=""Center"">&nbsp;</td>" + "\r\n" + @"<td class=""Right"">&nbsp;</td>" + "\r\n" + "</tr>" + "\r\n");
            output.Write("</table>");
        }
    }
}