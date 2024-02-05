using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page info interface
    /// </summary>
    public interface IPageInfo
    {
        /// <summary>
        /// Page template instance
        /// </summary>
        PageTemplateInstance TemplateInstance
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Document template instance
        /// </summary>
        PageTemplateInstance DocumentTemplateInstance
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Group template instance
        /// </summary>
        PageTemplateInstance GroupTemplateInstance 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Document ID
        /// </summary>
        int DocumentID 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Node ID
        /// </summary>
        int NodeID
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Document culture.
        /// </summary>
        string DocumentCulture
        {
            get;
            set;
        }

        /// <summary>
        /// Node alias path.
        /// </summary>
        string NodeAliasPath
        {
            get;
            set;
        }
    }
}
