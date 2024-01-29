using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Pager mode enum.
    /// </summary>
    public enum UniPagerMode
    {
        /// <summary>
        /// Querystring pager.
        /// </summary>
        Querystring,

        /// <summary>
        /// Postback pager.
        /// </summary>
        PostBack
    }


    /// <summary>
    /// HTML envelope rendering options.
    /// </summary>
    public enum HtmlEnvelopeRenderingMode
    {
        /// <summary>
        /// HTML envelope tag is rendered only if uni pager is in UpdatePanel (default).
        /// </summary>
        OnlyForUpdatePanel,

        /// <summary>
        /// HTML envelope tag is always rendered.
        /// </summary>
        Always,

        /// <summary>
        /// HTML envelope tag is never rendered.
        /// </summary>
        Never
    }
}