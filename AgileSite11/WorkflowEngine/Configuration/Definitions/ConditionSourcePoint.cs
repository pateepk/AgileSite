using CMS.Helpers;

namespace CMS.WorkflowEngine.Definitions
{
    /// <summary>
    /// Class handling initial values for condition source point.
    /// </summary>
    public class ConditionSourcePoint : SourcePoint
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConditionSourcePoint()
        {
            Label = ResHelper.GetString("workflowsourcepoint.defaultconditionlabel");
            Type = SourcePointTypeEnum.SwitchCase;
        }

        #endregion
    }
}
