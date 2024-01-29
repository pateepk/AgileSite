using System;

using CMS.DataEngine;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// TreeNode methods applicable to both PageInfo and TreeNode objects
    /// </summary>
    internal class TreeNodeMethods
    {
        /// <summary>
        /// Returns true if the given document is a root node
        /// </summary>
        /// <param name="node">Document node</param>
        public static bool IsRoot(ITreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            return IsRootClassName(node.ClassName);
        }


        /// <summary>
        /// Returns true if the given class name is a root node class name
        /// </summary>
        /// <param name="className">Class name</param>
        public static bool IsRootClassName(string className)
        {
            return className.EqualsCSafe(SystemDocumentTypes.Root, true);
        }


        /// <summary>
        /// Returns true if the document type stands for the product section
        /// </summary>
        /// <param name="node">Document node</param>
        public static bool IsProductSection(ITreeNode node)
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(node.ClassName);
            if (dci != null)
            {
                return dci.ClassIsProductSection;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the document's document type represents a product.
        /// </summary>
        /// <param name="node">Document node</param>
        public static bool IsProduct(ITreeNode node)
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(node.ClassName);
            if (dci != null)
            {
                return dci.ClassIsProduct;
            }

            return false;
        }
        

        /// <summary>
        /// Gets the page template id used by this document
        /// </summary>
        /// <param name="node">Document node</param>
        public static string GetUsedPageTemplateIdColumn(ITreeNode node)
        {
            return (node.NodeTemplateForAllCultures ? "NodeTemplateID" : "DocumentPageTemplateID");
        }
    }
}
