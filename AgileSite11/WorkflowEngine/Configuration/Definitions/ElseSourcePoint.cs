using CMS.Helpers;

namespace CMS.WorkflowEngine.Definitions
{
    /// <summary>
    /// Class handling initial values for else source point.
    /// </summary>
    public class ElseSourcePoint : SourcePoint
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ElseSourcePoint()
        {
            Label = ResHelper.GetString("workflowsourcepoint.else");
            Type = SourcePointTypeEnum.SwitchDefault;
        }

        #endregion
    }
}
