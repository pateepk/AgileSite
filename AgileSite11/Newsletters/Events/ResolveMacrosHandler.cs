using CMS.Base;
using CMS.MacroEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handler for the event when macros are being resolved in the newsletter module.
    /// </summary>
    public class ResolveMacrosHandler : AdvancedHandler<ResolveMacrosHandler, ResolveMacrosEventArgs>
    {
        /// <summary>
        /// Initiates event handling.
        /// </summary>
        /// <param name="macroResolver">Macro resolver which will be used to resolve macros in the text</param>
        /// <param name="textToResolve">Text which will be resolved</param>
        /// <param name="newsletter">Text which is being resolved belongs to the issue in this newsletter</param>
        /// <param name="issue">Text which is being resolved belongs to this issue</param>
        public ResolveMacrosHandler StartEvent(MacroResolver macroResolver, string textToResolve, NewsletterInfo newsletter = null, IssueInfo issue = null)
        {
            var e = new ResolveMacrosEventArgs
            {
                IssueInfo = issue,
                Newsletter = newsletter,
                MacroResolver = macroResolver,
                TextToResolve = textToResolve
            };

            return StartEvent(e);
        }
    }
}