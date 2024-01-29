using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Issue status enumeration
    /// </summary>
    public enum IssueStatusEnum
    {
        /// <summary>
        /// New issue that has not been sent yet
        /// </summary>
        [EnumStringRepresentation("Idle")]
        [EnumDefaultValue]
        Idle = 0,

        /// <summary>
        /// Issue is ready for sending (i.e. scheduled task for sending is enabled)
        /// </summary>
        [EnumStringRepresentation("ReadyForSending")]
        ReadyForSending = 1,

        /// <summary>
        /// Newsletter queue is being filled
        /// </summary>
        [EnumStringRepresentation("PreparingData")]
        PreparingData = 2,

        /// <summary>
        /// For A/B test when testing is in progress
        /// </summary>
        [EnumStringRepresentation("TestPhase")]
        TestPhase = 3,
        
        /// <summary>
        /// Issue is being sent
        /// </summary>
        [EnumStringRepresentation("Sending")]
        Sending = 4,

        /// <summary>
        /// Issue has been sent
        /// </summary>
        [EnumStringRepresentation("Finished")]
        Finished = 5
    }
}
