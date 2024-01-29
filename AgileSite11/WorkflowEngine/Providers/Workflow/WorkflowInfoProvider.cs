using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;
using CMS.Synchronization;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class to provide the workflow info management.
    /// </summary>
    public class WorkflowInfoProvider : AbstractInfoProvider<WorkflowInfo, WorkflowInfoProvider>, IFullNameInfoProvider
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowInfoProvider()
            : base(WorkflowInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    FullName = true
                })
        {
        }

        #endregion


        #region "Workflow management"

        /// <summary>
        /// Returns all workflows.
        /// </summary>
        public static ObjectQuery<WorkflowInfo> GetWorkflows()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets the specified workflow data.
        /// </summary>
        /// <param name="infoObj">Workflow data object</param>
        public static void SetWorkflowInfo(WorkflowInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Returns the WorkflowInfo structure for the specified workflow name. (Except the automation processes.)
        /// </summary>
        /// <param name="workflowName">Workflow name to use for retrieving the data</param>
        public static WorkflowInfo GetWorkflowInfo(string workflowName)
        {
            return ProviderObject.GetWorkflowInfoInternal(workflowName, WorkflowTypeEnum.Basic);
        }


        /// <summary>
        /// Returns the WorkflowInfo structure of specified type for the specified workflow name.
        /// </summary>
        /// <param name="workflowName">Workflow name to use for retrieving the data</param>
        /// <param name="type">Workflow type</param>
        public static WorkflowInfo GetWorkflowInfo(string workflowName, WorkflowTypeEnum type)
        {
            return ProviderObject.GetWorkflowInfoInternal(workflowName, type);
        }


        /// <summary>
        /// Returns the WorkflowInfo structure for the specified workflow ID.
        /// </summary>
        /// <param name="workflowId">ID of the workflow to retrieve</param>
        public static WorkflowInfo GetWorkflowInfo(int workflowId)
        {
            return ProviderObject.GetInfoById(workflowId);
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static WorkflowInfo GetWorkflowInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Deletes the specified workflow.
        /// </summary>
        /// <param name="wi">Workflow object to delete</param>
        public static void DeleteWorkflowInfo(WorkflowInfo wi)
        {
            ProviderObject.DeleteInfo(wi);
        }


        /// <summary>
        /// Deletes the specified workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID to delete</param>
        public static void DeleteWorkflowInfo(int workflowId)
        {
            WorkflowInfo wi = GetWorkflowInfo(workflowId);
            DeleteWorkflowInfo(wi);
        }


        /// <summary>
        /// Check dependencies, return true if is depend.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        public static bool CheckDependencies(int workflowId)
        {
            List<string> documentNames = new List<string>();
            return CheckDependencies(workflowId, ref documentNames);
        }


        /// <summary>
        /// Check dependencies, return true if is depend.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <param name="documentNames">List of document names which use given workflow</param>
        public static bool CheckDependencies(int workflowId, ref List<string> documentNames)
        {
            var infoObj = ProviderObject.GetInfoById(workflowId);
            if (infoObj != null)
            {
                documentNames = infoObj.Generalized.GetDependenciesNames();
                return (documentNames != null) && (documentNames.Count > 0);
            }
            return false;
        }


        /// <summary>
        /// Determines whether custom workflow steps are allowed by license for current domain.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if custom steps are allowed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCustomStepAllowed()
        {
            return (LicenseKeyInfoProvider.VersionLimitations(RequestContext.CurrentDomain, FeatureEnum.WorkflowVersioning, false) == LicenseHelper.LIMITATIONS_UNLIMITED);
        }


        /// <summary>
        /// Determines whether advanced workflow is available in license for current domain.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if advanced workflow is allowed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAdvancedWorkflowAllowed()
        {
            // Check license
            return LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.AdvancedWorkflow);
        }


        /// <summary>
        /// Determines whether marketing automation is available in license for current domain.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if marketing automation is allowed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMarketingAutomationAllowed()
        {
            // Check license
            return LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.MarketingAutomation);
        }


        /// <summary>
        /// Converts basic workflow to advanced workflow.
        /// </summary>
        /// <param name="workflowId">workflow ID</param>
        public static void ConvertToAdvancedWorkflow(int workflowId)
        {
            ProviderObject.ConvertToAdvancedWorkflowInternal(workflowId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WorkflowInfo structure of specified type for the specified workflow name.
        /// </summary>
        /// <param name="workflowName">Workflow name to use for retrieving the data</param>
        /// <param name="type">Workflow type</param>
        protected virtual WorkflowInfo GetWorkflowInfoInternal(string workflowName, WorkflowTypeEnum type)
        {
            string fullName = string.Format("{0}.{1}", workflowName, (int)type);
            return GetInfoByFullName(fullName, true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WorkflowInfo info)
        {
            base.DeleteInfo(info);

            // Delete the workflow steps tables
            ProviderHelper.ClearHashtables(WorkflowStepInfo.OBJECT_TYPE, true);
        }


        /// <summary>
        /// Converts basic workflow to advanced workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        protected virtual void ConvertToAdvancedWorkflowInternal(int workflowId)
        {
            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.AdvancedWorkflow);
            }

            WorkflowInfo wi = GetWorkflowInfo(workflowId);
            if (wi != null)
            {
                // Only basic workflows can be converted to advanced
                if (wi.IsBasic)
                {
                    // Automatic publishing must be turned off
                    if (wi.WorkflowAutoPublishChanges)
                    {
                        throw new Exception("[WorkflowInfoProvider.ConvertToAdvancedWorkflowInternal]: Workflow with automatic publishing cannot be converted.");
                    }

                    using (var tr = BeginTransaction())
                    {
                        // Destroy workflow history
                        ObjectVersionManager.DestroyObjectHistory(WorkflowInfo.OBJECT_TYPE, wi.WorkflowID);

                        using (CMSActionContext context = new CMSActionContext())
                        {
                            // Do not create version
                            context.CreateVersion = false;

                            // Make changes to workflow
                            wi.WorkflowType = WorkflowTypeEnum.Approval;
                            wi.WorkflowAllowedObjects = ";" + PredefinedObjectType.GROUP_DOCUMENTS + ";";
                            SetWorkflowInfo(wi);

                            // Process all steps in given workflow
                            var steps = WorkflowStepInfoProvider.GetWorkflowSteps(wi.WorkflowID)
                                .OrderBy("StepOrder");

                            var enumerator = steps.GetEnumerator();
                            if (enumerator.MoveNext())
                            {
                                WorkflowStepInfo step = enumerator.Current;
                                while (enumerator.MoveNext())
                                {
                                    // Ensure correct default definition
                                    step.EnsureDefaultDefinition();
                                    WorkflowStepInfoProvider.SetWorkflowStepInfo(step);

                                    // Create transition between subsequent steps
                                    var nextStep = enumerator.Current;
                                    Guid sourcePointGuid = step.StepDefinition.SourcePoints.First().Guid;
                                    WorkflowTransitionInfo connection = new WorkflowTransitionInfo()
                                    {
                                        TransitionStartStepID = step.StepID,
                                        TransitionSourcePointGUID = sourcePointGuid,
                                        TransitionEndStepID = nextStep.StepID,
                                        TransitionWorkflowID = wi.WorkflowID,
                                        TransitionType = step.GetStepConnectionType(sourcePointGuid)
                                    };
                                    WorkflowTransitionInfoProvider.SetWorkflowTransitionInfo(connection);

                                    // Move to next step
                                    step = nextStep;
                                }

                                // Ensure correct default definition for last step
                                step.EnsureDefaultDefinition();
                                WorkflowStepInfoProvider.SetWorkflowStepInfo(step);
                            }
                        }

                        // Ensure version
                        ObjectVersionManager.EnsureVersion(wi);

                        tr.Commit();
                    }
                }
                else
                {
                    throw new Exception("[WorkflowInfoProvider.ConvertToAdvancedWorkflowInternal]: Only basic workflow can be converted.");
                }
            }
        }

        #endregion


        #region "Full name methods"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(TypeInfo.ObjectType, "WorkflowName;WorkflowType");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string workflowName;
            string typeString;
            int workflowType;

            // Parse the full name
            if (ObjectHelper.ParseFullName(fullName, out workflowName, out typeString) && Int32.TryParse(typeString, out workflowType))
            {
                // Build the where condition
                string where = string.Format("WorkflowName = N'{0}'", SqlHelper.GetSafeQueryString(workflowName, false));
                string typeWhere = "WorkflowType = " + workflowType;

                WorkflowTypeEnum type = (WorkflowTypeEnum)workflowType;
                if (type == WorkflowTypeEnum.Basic)
                {
                    typeWhere = SqlHelper.AddWhereCondition(typeWhere, "WorkflowType IS NULL", "OR");
                }

                return SqlHelper.AddWhereCondition(where, typeWhere);
            }

            return null;
        }

        #endregion
    }
}