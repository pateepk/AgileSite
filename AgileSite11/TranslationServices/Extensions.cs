using CMS.DocumentEngine;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class TranslationExtensions
    {

        /// <summary>
        /// Submits the document to a translation.
        /// </summary>
        /// <param name="node">Source culture document (if you want to translate EN to CZ, pass EN version of the document as this parameter)</param>
        /// <param name="settings">Translation settings</param>
        /// <returns>Null if operation was successful, error message to display otherwise</returns>
        public static string SubmitToTranslation(this TreeNode node, TranslationSettings settings)
        {
            TranslationSubmissionInfo submissionInfo;
            return TranslationServiceHelper.SubmitToTranslation(settings, node, out submissionInfo);
        }
    }
}