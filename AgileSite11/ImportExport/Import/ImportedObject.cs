using CMS.DataEngine;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Imported object info
    /// </summary>
    public class ImportedObject
    {
        /// <summary>
        /// Info object
        /// </summary>
        public GeneralizedInfo Object
        {
            get;
            set;
        }


        /// <summary>
        /// Task type
        /// </summary>
        public TaskTypeEnum TaskType
        {
            get;
            set;
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="obj">Info object</param>
        /// <param name="taskType">Task type</param>
        public ImportedObject(GeneralizedInfo obj, TaskTypeEnum taskType)
        {
            Object = obj;
            TaskType = taskType;
        }
    }
}