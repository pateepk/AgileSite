using System;

using CMS.Helpers;

namespace CMS.WorkflowEngine.Definitions
{
    /// <summary>
    /// Class handling initial values for user choice source point.
    /// </summary>
    public class ChoiceSourcePoint : SourcePoint
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChoiceSourcePoint()
        {
            Label = ResHelper.GetString("workflowsourcepoint.newchoice");
            Type = SourcePointTypeEnum.SwitchCase;
        }


        /// <summary>
        /// Constructor ensuring numbered default labels
        /// </summary>
        /// <param name="order">Number to be added to label</param>
        public ChoiceSourcePoint(int order)
        {
            Label = String.Format("{0} {1}", ResHelper.GetString("workflowsourcepoint.defaultchoicelabel"), order);
            Type = SourcePointTypeEnum.SwitchCase;
        }

        #endregion
    }
}