using System;
using System.Linq;

using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Extension methods for a type actions on tree node.
    /// </summary>
    public static class TreeNodeTypeExtensions
    {
        #region "Public methods"

        /// <summary>
        /// Returns true if the given document is a root node
        /// </summary>
        /// <param name="node">Document node</param>
        public static bool IsRoot(this ITreeNode node)
        {
            return TreeNodeMethods.IsRoot(node);
        }

        
        /// <summary>
        /// Returns true if the given document is a file node
        /// </summary>
        /// <param name="node">Document node</param>
        public static bool IsFile(this ITreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            return node.ClassName.EqualsCSafe(SystemDocumentTypes.File, true);
        }


        /// <summary>
        /// Returns true if the given document is a folder node
        /// </summary>
        /// <param name="node">Document node</param>
        public static bool IsFolder(this ITreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            return node.ClassName.EqualsCSafe(SystemDocumentTypes.Folder, true);
        }
        
        #endregion
    }

}