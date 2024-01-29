using CMS.Base;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Create submission item event arguments
    /// </summary>
    public class CreateSubmissionItemEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Translation settings
        /// </summary>
        public TranslationSettings Settings
        {
            get;
            internal set;
        }


        /// <summary>
        /// Target object ID
        /// </summary>
        public int TargetObjectID
        {
            get;
            internal set;
        }


        /// <summary>
        /// Target culture code
        /// </summary>
        public string TargetCultureCode
        {
            get;
            internal set;
        }


        /// <summary>
        /// Source object to be translated
        /// </summary>
        public ICMSObject SourceObject
        {
            get;
            internal set;
        }


        /// <summary>
        /// Translation submission for which the item is created
        /// </summary>
        public TranslationSubmissionInfo Submission
        {
            get;
            internal set;
        }


        /// <summary>
        /// Created submission item
        /// </summary>
        public TranslationSubmissionItemInfo SubmissionItem
        {
            get;
            internal set;
        }
    }
}