using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Methods for resolving macros within discussion posts.
    /// </summary>
    public class DiscussionMacroHelper : CoreMethods
    {
        #region "Variables"

        private static RequestStockValue<DiscussionMacroResolver> mCurrentDiscussionResolver = new RequestStockValue<DiscussionMacroResolver>("CurrentDiscussionResolver", () => new DiscussionMacroResolver());

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current discussion macro resolver.
        /// </summary>
        public static DiscussionMacroResolver CurrentDiscussionResolver
        {
            get
            {
                return mCurrentDiscussionResolver;
            }
            set
            {
                mCurrentDiscussionResolver.Value = value;
            }
        }
        
        #endregion
        

        #region "Static methods"
        
        /// <summary>
        /// Resolves the discussion macros.
        /// </summary>
        /// <param name="inputText">Text to resolve</param>
        public static string ResolveDiscussionMacros(string inputText)
        {
            // Get resolver and resolve macros
            var resolver = CurrentDiscussionResolver;
            if (resolver != null)
            {
                return resolver.ResolveMacros(inputText, null);
            }

            return inputText;
        }

        #endregion
    }
}