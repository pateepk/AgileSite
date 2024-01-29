using System;

using CMS.Helpers;

namespace CMS.WorkflowEngine.Definitions
{
    /// <summary>
    /// Class handling initial values for user choice source point.
    /// </summary>
    public class CaseSourcePoint : SourcePoint
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public CaseSourcePoint()
        {
            Label = ResHelper.GetString("workflowsourcepoint.newcase");
            Type = SourcePointTypeEnum.SwitchCase;
        }


        /// <summary>
        /// Constructor ensuring numbered default labels
        /// </summary>
        /// <param name="order">Number to be added to label</param>
        public CaseSourcePoint(int order)
        {
            Label = String.Format("{0} {1}",ResHelper.GetString("workflowsourcepoint.defaultcaselabel"), order);
            Type = SourcePointTypeEnum.SwitchCase;
        }

        #endregion
    }
}
