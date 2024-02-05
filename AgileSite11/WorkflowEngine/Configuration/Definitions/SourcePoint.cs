using System;
using System.Xml.Serialization;

using CMS.Helpers;

namespace CMS.WorkflowEngine.Definitions
{
    /// <summary>
    /// Class to hold the workflow step source point definition.
    /// </summary>
    [XmlInclude(typeof(ConditionSourcePoint)), XmlInclude(typeof(ElseSourcePoint)), XmlInclude(typeof(CaseSourcePoint)), XmlInclude(typeof(ChoiceSourcePoint)), XmlInclude(typeof(TimeoutSourcePoint))]
    public class SourcePoint
    {
        #region "Properties"

        /// <summary>
        /// Point GUID
        /// </summary>
        [XmlAttribute()]
        public Guid Guid
        {
            get;
            set;
        }


        /// <summary>
        /// Point label
        /// </summary>
        public string Label
        {
            get;
            set;
        }


        /// <summary>
        /// Point action text
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Point action tooltip
        /// </summary>
        public string Tooltip
        {
            get;
            set;
        }        


        /// <summary>
        /// Point condition
        /// </summary>
        public string Condition
        {
            get;
            set;
        }


        /// <summary>
        /// Point type
        /// </summary>
        public SourcePointTypeEnum Type
        {
            get;
            set;
        }


        /// <summary>
        /// Security settings for roles
        /// </summary>
        public WorkflowStepSecurityEnum StepRolesSecurity
        {
            get;
            set;
        }


        /// <summary>
        /// Security settings for users
        /// </summary>
        public WorkflowStepSecurityEnum StepUsersSecurity
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if users settings are inherited
        /// </summary>
        public bool InheritsUsersSettings
        { 
            get
            {
                return (StepUsersSecurity == WorkflowStepSecurityEnum.Default);
            }
        }


        /// <summary>
        /// Indicates if roles settings are inherited
        /// </summary>
        public bool InheritsRolesSettings
        { 
            get
            {
                return (StepRolesSecurity == WorkflowStepSecurityEnum.Default);
            }
        }
        

        /// <summary>
        /// Indicates if all step security settings are inherited
        /// </summary>
        public bool InheritsStepSettings
        { 
            get
            {
                return InheritsRolesSettings && InheritsUsersSettings;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public SourcePoint()
        {
            Guid = Guid.NewGuid();
            Label = ResHelper.GetString("workflowsourcepoint.standard");
            Type = SourcePointTypeEnum.Standard;
        }

        #endregion
    }
}