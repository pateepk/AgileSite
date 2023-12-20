using System;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Button that generates appropriate HTML code for its use in actions container.
    /// </summary>
    public class CMSAccessibleButtonBase : CMSButton
    {
        /// <summary>
        /// When true button is render as type="submit". Otherwise type is type="button".
        /// </summary>
        public override bool UseSubmitBehavior
        {
            get;
            set;
        }


        /// <summary>
        /// HTML tag for this control.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Button;
            }
        }


        /// <summary>
        /// When true input tag is rendered instead of button tag.
        /// </summary>
        public override bool RenderInputTag
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException("CMSAccessibleButton must always render button tag. This property cannot be changed.");
            }
        }
    }
}
