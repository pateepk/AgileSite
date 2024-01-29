using System;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing information about last visited page.
    /// </summary>
    [Serializable]
    public class UserPage
    {
        /// <summary>
        /// Last request timestamp.
        /// </summary>
        public DateTime TimeStamp
        {
            get;
            set;
        }
        

        /// <summary>
        /// Gets or sets the last page document ID.
        /// </summary>
        public int LastPageDocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the last page node ID.
        /// </summary>
        public int LastPageNodeID
        {
            get;
            set;
        }
        
        
        /// <summary>
        /// Unique identifier of the last visited page.
        /// </summary>
        public Guid Identifier
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public UserPage()
        {
            Identifier = Guid.NewGuid();
        }
    }
}