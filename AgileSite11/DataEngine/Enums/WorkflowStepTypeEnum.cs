using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Workflow step types enumeration.
    /// </summary>
    public enum WorkflowStepTypeEnum
    {
        /// <summary>
        /// Undefined workflow step.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// General start step.
        /// </summary>
        [EnumStringRepresentation("workflow.startStep")]
        Start = 1,

        /// <summary>
        /// Special type of step for document edit step.
        /// </summary>
        [EnumStringRepresentation("workflow.documentEditStep")]
        DocumentEdit = 2,
        
        /// <summary>
        /// Standard workflow step.
        /// </summary>
        [EnumStringRepresentation("workflow.standardStep")]
        Standard = 3,

        /// <summary>
        /// Conditional step with else branch.
        /// </summary>
        [EnumStringRepresentation("workflow.conditionStep")]
        Condition = 6,

        /// <summary>
        /// Step with several conditional choices. (If multiple conditions match, user must choose one.)
        /// </summary>
        [EnumStringRepresentation("workflow.multichoiceStep")]
        Multichoice = 7,

        /// <summary>
        /// Step with several conditional choices. (If multiple conditions match, first is used.)
        /// </summary>
        [EnumStringRepresentation("workflow.multichoiceFirstWinStep")]
        MultichoiceFirstWin = 8,

        /// <summary>
        /// Step with several conditional choices. (User must always choose.)
        /// </summary>
        [EnumStringRepresentation("workflow.userchoiceStep")]
        Userchoice = 9,

        /// <summary>
        /// Wait step.
        /// </summary>
        [EnumStringRepresentation("workflow.waitStep")]
        Wait = 10,

        /// <summary>
        /// Step with custom action.
        /// </summary>
        Action = 11,

        /// <summary>
        /// Step with custom action.
        /// </summary>
        [EnumStringRepresentation("workflow.finishedStep")]
        Finished = 99,

        /// <summary>
        /// Special type of step for document published step.
        /// </summary>
        [EnumStringRepresentation("workflow.documentPublishedStep")]
        DocumentPublished = 100,

        /// <summary>
        /// Special type of step for document archived step.
        /// </summary>
        [EnumStringRepresentation("workflow.documentArchivedStep")]
        DocumentArchived = 101,
    }
}