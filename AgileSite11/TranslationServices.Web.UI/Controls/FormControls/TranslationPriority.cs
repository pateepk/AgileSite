using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.TranslationServices.Web.UI
{
    /// <summary>
    /// Form control for translation priority selection.
    /// </summary>
    [ToolboxData("<{0}:TranslationPriority runat=server></{0}:TranslationPriority>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class TranslationPriority : FormControl
    {
        #region "Public properties"

        /// <summary>
        /// Returns the instructions of the submission.
        /// </summary>
        public int Priority
        {
            get
            {
                return ValidationHelper.GetInteger(Value, 1);
            }
            set
            {
                Value = value;
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public TranslationPriority()
        {
            FormControlName = "TranslationPrioritySelector";
        }
    }
}