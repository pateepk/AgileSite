using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Button control with localized text string.
    /// </summary>
    [ToolboxData("<{0}:LocalizedDropDownList runat=server />"), Serializable()]
    public class LocalizedDropDownList : CMSDropDownList
    {
    }
}