using CMS.Helpers;

namespace CMS.WorkflowEngine.Definitions
{
    /// <summary>
    /// Class handling initial values for timeout source point.
    /// </summary>
    public class TimeoutSourcePoint : SourcePoint
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public TimeoutSourcePoint()
        {
            Label = ResHelper.GetString("workflowsourcepoint.timeout");
            Type = SourcePointTypeEnum.Timeout;
        }

        #endregion
    }
}
