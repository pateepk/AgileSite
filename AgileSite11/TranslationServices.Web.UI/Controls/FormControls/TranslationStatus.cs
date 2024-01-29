using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.TranslationServices.Web.UI
{
    /// <summary>
    /// Form control for translation status selection.
    /// </summary>
    [ToolboxData("<{0}:TranslationStatus runat=server></{0}:TranslationStatus>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class TranslationStatus : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TranslationStatus()
        {
            FormControlName = "TranslationStatus";
        }
    }
}