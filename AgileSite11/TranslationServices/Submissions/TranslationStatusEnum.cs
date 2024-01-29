using System;
using CMS.Helpers;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation process statuses.
    /// </summary>
    public enum TranslationStatusEnum
    {
        /// <summary>
        /// Horizontal.
        /// </summary>
        [EnumStringRepresentation("WaitingForTranslation")]
        WaitingForTranslation = 0,


        /// <summary>
        /// Translation is ready to process
        /// </summary>
        [EnumStringRepresentation("TranslationReady")]
        TranslationReady = 1,


        /// <summary>
        /// Translation was already processed.
        /// </summary>
        [EnumStringRepresentation("TranslationCompleted")]
        TranslationCompleted = 2,


        /// <summary>
        /// Error while submitting occured.
        /// </summary>
        [EnumStringRepresentation("SubmissionError")]
        SubmissionError = 3,


        /// <summary>
        /// Error while processing occured.
        /// </summary>
        [EnumStringRepresentation("ProcessingError")]
        ProcessingError = 4,


        /// <summary>
        /// Translation was canceled.
        /// </summary>
        [EnumStringRepresentation("TranslationCanceled")]
        TranslationCanceled = 5,


        /// <summary>
        /// Submission is being resubmitted
        /// </summary>
        [EnumStringRepresentation("ResubmittingSubmission")]
        ResubmittingSubmission = 6,


        /// <summary>
        /// Submission is being processed
        /// </summary>
        [EnumStringRepresentation("ProcessingSubmission")]
        ProcessingSubmission = 7,
    }
}