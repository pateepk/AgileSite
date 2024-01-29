using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document mark event arguments.
    /// </summary>
    public class DocumentMarkEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Content of the mark to be modified.
        /// </summary>
        public string MarkContent
        {
            get;
            set;
        }


        /// <summary>
        /// Site name.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Preferred culture code.
        /// </summary>
        public string PreferredCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Current workflow step.
        /// </summary>
        public WorkflowStepTypeEnum StepType
        {
            get;
            set;
        }


        /// <summary>
        /// Container with Document and Node data.
        /// </summary>
        public ISimpleDataContainer Container
        {
            get;
            set;
        }
    }
}
