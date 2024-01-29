using CMS.Base;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation handler for processing the translation
    /// </summary>
    public class ProcessTranslationHandler : AdvancedHandler<ProcessTranslationHandler, ProcessTranslationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="sourceObject">Source object ot be translated</param>
        /// <param name="targetObject">Target object ot be used as a translation</param>
        /// <param name="submission">Submission being processed</param>
        public ProcessTranslationHandler StartEvent(ICMSObject sourceObject, ICMSObject targetObject, TranslationSubmissionInfo submission)
        {
            var e = new ProcessTranslationEventArgs
            {
                SourceObject = sourceObject,
                TargetObject = targetObject,
                Submission = submission
            };

            return StartEvent(e);
        }
    }
}