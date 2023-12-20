using System;

namespace CMS.Base
{
    /// <summary>
    /// Objects containing TreeNode properties.
    /// </summary>
    public interface ITreeNode : IDataContainer
    {
        /// <summary>
        /// Gets the node ID.
        /// </summary>
        int NodeID { get; }


        /// <summary>
        /// Gets the node GUID.
        /// </summary>
        Guid NodeGUID { get; }


        /// <summary>
        /// Gets the document ID.
        /// </summary>
        int DocumentID { get; }


        /// <summary>
        /// Gets the document culture.
        /// </summary>
        string DocumentCulture { get; }


        /// <summary>
        /// Gets the node alias path.
        /// </summary>
        string NodeAliasPath { get; }


        /// <summary>
        /// Indicates whether the document is link to another document.
        /// </summary>
        bool IsLink { get; }


        /// <summary>
        /// Gets the node site ID.
        /// </summary>
        int NodeSiteID { get; }


        /// <summary>
        /// Gets the node site name.
        /// </summary>
        string NodeSiteName { get; }


        /// <summary>
        /// Gets the node class name.
        /// </summary>
        string ClassName { get; }


        /// <summary>
        /// If true, the document uses the same template (NodeTemplateID) for all culture versions.
        /// </summary>
        bool NodeTemplateForAllCultures { get; }


        /// <summary>
        /// Gets the original node site ID. Returns NodeSiteID for standard document, LinkedNodeSiteID for linked document.
        /// </summary>
        int OriginalNodeSiteID { get; }


        /// <summary>
        /// Gets the document URL path.
        /// </summary>
        string DocumentUrlPath { get; }


        /// <summary>
        /// Gets the document extensions.
        /// </summary>
        string DocumentExtensions { get; }


        /// <summary>
        /// Gets the page template id used by this document.
        /// </summary>
        int GetUsedPageTemplateId();
    }
}