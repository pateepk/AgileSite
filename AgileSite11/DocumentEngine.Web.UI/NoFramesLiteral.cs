using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Renders "noframes" tags with default message.
    /// </summary>
    [ToolboxItem(false)]
    public class NoFramesLiteral : Literal
    {
        #region "Private variables"

        private static Dictionary<string, string> ResStrings;

        #endregion


        /// <summary>
        /// Returns entire "noframes" tags with default message.
        /// </summary>
        private string GetString()
        {
            // Create hashtable if not created
            if (ResStrings == null)
            {
                ResStrings = new Dictionary<string, string>();
            }

            string culture = Thread.CurrentThread.CurrentUICulture.ToString();
            if (ResStrings.ContainsKey(culture))
            {
                return ResStrings[culture];
            }
            else
            {
                return ResStrings[culture] = "<noframes><body><p>" + ResHelper.GetAPIString("general.noframescontent", culture, "This HTML frameset displays multiple Web pages. To view this frameset, use a Web browser that supports HTML 4.0 and later.") + "</p></body></noframes>";
            }
        }


        /// <summary>
        /// Renders tags.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            EnableViewState = false;
            Text = GetString();
        }
    }
}