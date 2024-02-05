using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Search
{
    /// <summary>
    /// Wrappes values required to create a new search task.
    /// </summary>
    public class SearchTaskCreationParameters
    {
        /// <summary>
        /// Type of the search task
        /// </summary>
        public SearchTaskTypeEnum TaskType
        {
            get;
            set;
        }


        /// <summary>
        /// Object type of the indexed object. Not required for rebuild and optimize tasks.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Is used for indexer to decide how to process the task and to understand the information stored in task value. 
        /// </summary>
        public string ObjectField
        {
            get;
            set;
        }


        /// <summary>
        /// Value that indexer requires to correctly process the task. 
        /// </summary>
        public string TaskValue
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the related object. With this ID we can pinpoint the object, which change caused the creation of the search task. 
        /// </summary>
        public int RelatedObjectID
        {
            get;
            set;
        }
    }
}
