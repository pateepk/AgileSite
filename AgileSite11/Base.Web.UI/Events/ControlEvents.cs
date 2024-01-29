using System;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Before event handler.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    /// <returns>Returns true if default action should be performed</returns>
    public delegate bool BeforeEventHandler(object sender, EventArgs e);


    /// <summary>
    /// Before event handler.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="writer">HTML writer</param>
    /// <returns>Returns true if default action should be performed</returns>
    public delegate bool BeforeRenderEventHandler(object sender, HtmlTextWriter writer);


    /// <summary>
    /// Before event handler.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="writer">HTML writer</param>
    public delegate void RenderEventHandler(object sender, HtmlTextWriter writer);
}