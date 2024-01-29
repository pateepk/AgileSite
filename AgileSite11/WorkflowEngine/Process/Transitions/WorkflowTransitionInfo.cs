using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WorkflowEngine.Definitions;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowTransitionInfo), WorkflowTransitionInfo.OBJECT_TYPE)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// WorkflowTransitionInfo data container class.
    /// </summary>
    public class WorkflowTransitionInfo : AbstractInfo<WorkflowTransitionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflowtransition";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowTransitionInfoProvider), OBJECT_TYPE, "CMS.WorkflowTransition", "TransitionID", "TransitionLastModified", null, null, null, null, null, "TransitionStartStepID", WorkflowStepInfo.OBJECT_TYPE)
        {
            IsBinding = true,
            DependsOn = new List<ObjectDependency>()
                {
                    new ObjectDependency("TransitionEndStepID", WorkflowStepInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding), 
                    new ObjectDependency("TransitionWorkflowID", WorkflowInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
                },
            LogEvents = true,
            TouchCacheDependencies = true,
            RegisterAsBindingToObjectTypes = new List<string>() { WorkflowStepInfo.OBJECT_TYPE, WorkflowStepInfo.OBJECT_TYPE_AUTOMATION },
            RegisterAsOtherBindingToObjectTypes = new List<string>() { WorkflowStepInfo.OBJECT_TYPE, WorkflowStepInfo.OBJECT_TYPE_AUTOMATION },
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Transition ID.
        /// </summary>
        public virtual int TransitionID
        {
            get
            {
                return GetIntegerValue("TransitionID", 0);
            }
            set
            {
                SetValue("TransitionID", value);
            }
        }


        /// <summary>
        /// Start step of the transition.
        /// </summary>
        public virtual int TransitionStartStepID
        {
            get
            {
                return GetIntegerValue("TransitionStartStepID", 0);
            }
            set
            {
                SetValue("TransitionStartStepID", value);
            }
        }


        /// <summary>
        /// End step of the transition.
        /// </summary>
        public virtual int TransitionEndStepID
        {
            get
            {
                return GetIntegerValue("TransitionEndStepID", 0);
            }
            set
            {
                SetValue("TransitionEndStepID", value);
            }
        }


        /// <summary>
        /// Transition GUID.
        /// </summary>
        public virtual WorkflowTransitionTypeEnum TransitionType
        {
            get
            {
                return (WorkflowTransitionTypeEnum)GetIntegerValue("TransitionType", 0);
            }
            set
            {
                SetValue("TransitionType", (int)value);
            }
        }


        /// <summary>
        /// Transition time stamp.
        /// </summary>
        public virtual DateTime TransitionLastModified
        {
            get
            {
                return GetDateTimeValue("TransitionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TransitionLastModified", value);
            }
        }


        /// <summary>
        /// Transition source point GUID.
        /// </summary>
        public virtual Guid TransitionSourcePointGUID
        {
            get
            {
                return GetGuidValue("TransitionSourcePointGUID", Guid.Empty);
            }
            set
            {
                SetValue("TransitionSourcePointGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Transition workflow ID.
        /// </summary>
        public virtual int TransitionWorkflowID
        {
            get
            {
                return GetIntegerValue("TransitionWorkflowID", 0);
            }
            set
            {
                SetValue("TransitionWorkflowID", value);
            }
        }

        #endregion


        #region "Special properties"

        /// <summary>
        /// Start point definition of the transition (dynamically loaded)
        /// </summary>
        public SourcePoint TransitionSourcePoint
        {
            get
            {
                // Get start point from step
                if ((TransitionStartStepID > 0) && (TransitionSourcePointGUID != Guid.Empty))
                {
                    WorkflowStepInfo step = WorkflowStepInfoProvider.GetWorkflowStepInfo(TransitionStartStepID);
                    if (step != null)
                    {
                        var point = from p in step.StepDefinition.SourcePoints where p.Guid == TransitionSourcePointGUID select p;
                        return point.FirstOrDefault();
                    }
                }

                return null;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WorkflowTransitionInfoProvider.DeleteWorkflowTransitionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(this);
        }


        /// <summary>
        /// Returns the existing object based on current object data.
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            var where = new WhereCondition()
                .Where("TransitionStartStepID", QueryOperator.Equals, TransitionStartStepID)
                .Where("TransitionEndStepID", QueryOperator.Equals, TransitionEndStepID);

            if (TransitionSourcePointGUID != Guid.Empty)
            {
                where.Where("TransitionSourcePointGUID", QueryOperator.Equals, TransitionSourcePointGUID);
            }
            else
            {
                where.WhereNull("TransitionSourcePointGUID");
            }

            return where;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowTransitionInfo object.
        /// </summary>
        public WorkflowTransitionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowTransitionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public WorkflowTransitionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
