using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.WorkflowEngine.Factories
{
    /// <summary>
    /// Factory for creating setting objects based on workflow step type.
    /// </summary>
    /// <typeparam name="ContainerType">Type of settings object</typeparam>
    public class StepTypeFactory<ContainerType>
    {
        #region "Private variables"

        /// <summary>
        /// Local variable containing dependency injector.
        /// </summary>
        private StepTypeDependencyInjector<ContainerType> mInjector = null;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Sets dependency injector.
        /// </summary>
        /// <param name="injector">Dependency injector used in factory methods</param>
        public StepTypeFactory(StepTypeDependencyInjector<ContainerType> injector)
        {
            mInjector = injector;
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Creates menu items based on workflow step types.
        /// </summary>
        /// <param name="stepTypes">List of workflow step types</param>
        /// <returns>List of settings objects</returns>
        public List<ContainerType> GetSettingsObject(List<WorkflowStepTypeEnum> stepTypes)
        {
            List<ContainerType> menuItems = new List<ContainerType>();
            foreach (WorkflowStepTypeEnum type in stepTypes)
            {
                menuItems.Add(GetSettingsObject(type));
            }
            return menuItems;
        }


        /// <summary>
        /// Creates menu items based on workflow step type.
        /// </summary>
        /// <param name="nodeType">Workflow step types</param>
        /// <returns>Settings object</returns>
        public ContainerType GetSettingsObject(WorkflowStepTypeEnum nodeType)
        {
            switch (nodeType)
            {
                case WorkflowStepTypeEnum.Action:
                    return mInjector.GetActionSettings();

                case WorkflowStepTypeEnum.Condition:
                    return mInjector.GetConditionSettings();

                case WorkflowStepTypeEnum.DocumentArchived:
                    return mInjector.GetDocumentArchivedSettings();

                case WorkflowStepTypeEnum.DocumentEdit:
                    return mInjector.GetDocumentEditSettings();

                case WorkflowStepTypeEnum.DocumentPublished:
                    return mInjector.GetDocumentPublishedSettings();

                case WorkflowStepTypeEnum.Multichoice:
                    return mInjector.GetMultichoiceSettings();

                case WorkflowStepTypeEnum.MultichoiceFirstWin:
                    return mInjector.GetMultichoiceFirstWinSettings();

                case WorkflowStepTypeEnum.Standard:
                    return mInjector.GetStandardSettings();

                case WorkflowStepTypeEnum.Start:
                    return mInjector.GetStartSettings();

                case WorkflowStepTypeEnum.Finished:
                    return mInjector.GetFinishedSettings();

                case WorkflowStepTypeEnum.Undefined:
                    return mInjector.GetUndefinedSettings();

                case WorkflowStepTypeEnum.Userchoice:
                    return mInjector.GetUserchoiceSettings();

                case WorkflowStepTypeEnum.Wait:
                    return mInjector.GetWaitSettings();

                default:
                    return mInjector.GetDefaultSettings();
            }
        }

        #endregion
    }
}
