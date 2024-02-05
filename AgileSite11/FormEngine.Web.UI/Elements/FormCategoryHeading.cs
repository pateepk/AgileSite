using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Heading control with localized text string and anchor support. Renders as H1 - H6 element.
    /// </summary>
    [ToolboxData("<{0}:FormCategoryHeading runat=server></{0}:FormCategoryHeading>"), Serializable]
    public class FormCategoryHeading : LocalizedHeading
    {
        /// <summary>
        /// Indicates if this heading serves as anchor. Heading has 'anchor' css class assigned besides classes defined in CssClass property.
        /// </summary>
        public bool IsAnchor
        {
            get;
            set;
        }


        /// <summary>
        /// Register anchor to context and handle css class
        /// </summary>       
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (IsAnchor)
            {
                this.AddCssClass("anchor");
                SetAnchorToUIContext();
            }
        }


        /// <summary>
        /// Sets anchor link to UIContext.
        /// </summary>
        protected void SetAnchorToUIContext()
        {
            // Get UIContext
            UIContext context = UIContextHelper.GetUIContext();
            if (context == null)
            {
                return;
            }

            string text = String.IsNullOrEmpty(Text) ? ResHelper.GetString(ResourceString) : Text;

            // Add link to this label tu UIContext
            if (text != null)
            {
                context.AnchorLinks.Add(new KeyValuePair<string, string>(text, ClientID));
            }
        }
    }
}