using System.Collections.Generic;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Interface class for the controls providing content for spell checker.
    /// </summary>
    public interface ISpellCheckableControl
    {
        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        List<string> GetSpellCheckFields();
    }
}