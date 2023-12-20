using CMS.Base;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation handler for creating the submission item
    /// </summary>
    public class CreateSubmissionItemHandler : AdvancedHandler<CreateSubmissionItemHandler, CreateSubmissionItemEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Translation settings</param>
        /// <param name="submission">Submission being processed</param>
        /// <param name="source">Source object being translated</param>
        /// <param name="targetObjectId">Target object ID</param>
        /// <param name="targetCultureCode">Target culture code</param>
        public CreateSubmissionItemHandler StartEvent(TranslationSettings settings, TranslationSubmissionInfo submission, ICMSObject source, int targetObjectId, string targetCultureCode)
        {
            var e = new CreateSubmissionItemEventArgs
            {
                Settings = settings,
                SourceObject = source,
                TargetObjectID = targetObjectId,
                TargetCultureCode = targetCultureCode,
                Submission = submission
            };

            return StartEvent(e);
        }
    }
}