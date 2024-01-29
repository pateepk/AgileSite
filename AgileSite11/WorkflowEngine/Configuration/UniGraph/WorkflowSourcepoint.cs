using System;
using System.Runtime.Serialization;

using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;

using WorkflowSourcePointDefinition = CMS.WorkflowEngine.Definitions.SourcePoint;

namespace CMS.WorkflowEngine.GraphConfig
{
    /// <summary>
    /// Workflow source point configuration
    /// </summary>
    [DataContract(Name="SourcePoint", Namespace="CMS.Helpers.UniGraphConfig")]
    public class WorkflowSourcePoint : SourcePoint
    {
        #region "Constructors"

        /// <summary>
        /// Creates source point configuration object by given workflow source point object.
        /// </summary>
        /// <param name="sourcePoint">Source point to be rewritten</param>
        /// <returns>Source point configuration object</returns>
        public WorkflowSourcePoint(WorkflowSourcePointDefinition sourcePoint)
        {
            if (sourcePoint == null)
            {
                throw new NullReferenceException("[WorkflowConnection] : Workflow SourcePoint is null.");
            }
            ID = sourcePoint.Guid.ToString();
            Label = ResHelper.LocalizeString(sourcePoint.Label);
            IsLabelLocalized = Label != sourcePoint.Label;
            Type = sourcePoint.Type;
            Tooltip = sourcePoint.Condition;
        }

        #endregion
    }
}
