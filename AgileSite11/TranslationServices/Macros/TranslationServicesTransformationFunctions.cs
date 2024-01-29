using System;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation services functions.
    /// </summary>
    public class TranslationServicesTransformationFunctions
    {
        /// <summary>
        /// Returns true if there is at least one translation submission item with target XLIFF for given page and submission is marked as ready.
        /// </summary>
        /// <param name="document">Document to check</param>
        public static bool IsTranslationReady(object document)
        {
            var doc = document as TreeNode;
            if (doc == null)
            {
                return false;
            }

            return TranslationServiceHelper.IsTranslationReady(doc);
        }
    }
}