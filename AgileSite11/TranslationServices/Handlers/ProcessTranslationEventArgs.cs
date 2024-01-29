using CMS.Base;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Process translation event arguments
    /// </summary>
    public class ProcessTranslationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Source object to be translated
        /// </summary>
        public ICMSObject SourceObject
        {
            get;
            internal set;
        }


        /// <summary>
        /// Target object to be used as a translation (Typically same as the soure object. Documents have separate culture versions.)
        /// </summary>
        public ICMSObject TargetObject
        {
            get;
            internal set;
        }


        /// <summary>
        /// Translation submission for which the translation is processed
        /// </summary>
        public TranslationSubmissionInfo Submission
        {
            get;
            internal set;
        }


        /// <summary>
        /// Indicates if the update of the target object should be forced
        /// </summary>
        public bool ForceTargetObjectUpdate
        {
            get;
            set;
        }
    }
}