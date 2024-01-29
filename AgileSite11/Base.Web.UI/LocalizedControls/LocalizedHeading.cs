using System;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Heading control with localized text string. Renders as H1 - H6 element.
    /// </summary>
    [ToolboxData("<{0}:LocalizedHeading runat=server></{0}:LocalizedHeading>"), Serializable()]
    public class LocalizedHeading : LocalizedLabel
    {
        /// <summary>
        /// Level of heading. Value must be from 1 to 6.
        /// </summary>
        public int Level
        {
            get;
            set;
        }


        /// <summary>
        /// Tag key.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                switch (Level)
                {
                    case 1: return HtmlTextWriterTag.H1;
                    case 2: return HtmlTextWriterTag.H2;
                    case 3: return HtmlTextWriterTag.H3;
                    case 4: return HtmlTextWriterTag.H4;
                    case 5: return HtmlTextWriterTag.H5;
                    case 6: return HtmlTextWriterTag.H6;

                    default: return HtmlTextWriterTag.H1;
                }
            }
        }
    }
}
