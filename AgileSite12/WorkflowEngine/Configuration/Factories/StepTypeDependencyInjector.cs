using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.WorkflowEngine.Factories
{
    /// <summary>
    /// Abstract class for creating settings containers based on step type.
    /// </summary>
    /// <typeparam name="Type">Type of returned setting object</typeparam>
    public abstract class StepTypeDependencyInjector<Type>
    {
        #region "Private variables"

        /// <summary>
        /// Private property factory.
        /// </summary>
        private StepTypeFactory<Type> mFactory = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Setting property executing the creation.
        /// </summary>
        public StepTypeFactory<Type> Factory
        {
            get
            {
                return mFactory ?? (mFactory = new StepTypeFactory<Type>(this));
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns settings for specified type.
        /// </summary>
        /// <param name="type">Step type</param>
        public virtual Type GetSettingsObject(WorkflowStepTypeEnum type)
        {
            return Factory.GetSettingsObject(type);
        }


        /// <summary>
        /// Returns settings for specified types.
        /// </summary>
        /// <param name="types">Step types</param>
        public virtual List<Type> GetSettingsObject(List<WorkflowStepTypeEnum> types)
        {
            return Factory.GetSettingsObject(types);
        }


        /// <summary>
        /// Injects dependencies for step type action. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetActionSettings();

        /// <summary>
        /// Injects dependencies for step type condition. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetConditionSettings();


        /// <summary>
        /// Injects dependencies for step type of archived document. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetDocumentArchivedSettings();


        /// <summary>
        /// Injects dependencies for step type of edited document. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetDocumentEditSettings();


        /// <summary>
        /// Injects dependencies for step type of published document. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetDocumentPublishedSettings();


        /// <summary>
        /// Injects dependencies for step type multi choice. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetMultichoiceSettings();


        /// <summary>
        /// Injects dependencies for step type multi choice first win. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetMultichoiceFirstWinSettings();


        /// <summary>
        /// Injects dependencies for step type standard. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetStandardSettings();


        /// <summary>
        /// Injects dependencies for step type start. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetStartSettings();


        /// <summary>
        /// Injects dependencies for step type start. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetFinishedSettings();


        /// <summary>
        /// Injects dependencies for step type undefined. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetUndefinedSettings();


        /// <summary>
        /// Injects dependencies for step type user choice. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetUserchoiceSettings();


        /// <summary>
        /// Injects dependencies for step type wait. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetWaitSettings();


        /// <summary>
        /// Injects dependencies for step type default. 
        /// </summary>
        /// <returns>Generic data type</returns>
        public abstract Type GetDefaultSettings();

        #endregion
    }
}
