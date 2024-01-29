using CMS.Base;
using CMS.MacroEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Event arguments for the <see cref="ResolveMacrosHandler"/>.
    /// </summary>
    public class ResolveMacrosEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Macro resolver which will be used to resolve macros.
        /// </summary>
        public MacroResolver MacroResolver
        {
            get;
            set;
        }


        /// <summary>
        /// Text before resolving on the Before event or already resolved text on the After event.
        /// </summary>
        public string TextToResolve
        {
            get;
            set;
        }


        /// <summary>
        /// Text which is being resolved belongs to the issue in this newsletter.
        /// Can be null if there is no corresponding newsletter available.
        /// </summary>
        public NewsletterInfo Newsletter
        {
            get;
            set;
        }


        /// <summary>
        /// Text which is being resolved belongs to this issue.
        /// Can be null if there is no corresponding issue available.
        /// </summary>
        public IssueInfo IssueInfo
        {
            get;
            set;
        }
    }
}