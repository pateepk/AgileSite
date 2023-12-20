using CMS.Core;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Generator of the specific document types
    /// </summary>
    public static class DocumentGenerator
    {
        /// <summary>
        /// Object generator
        /// </summary>
        private static readonly ObjectGenerator<TreeNode> mGenerator = new ObjectGenerator<TreeNode>(new ObjectFactory<TreeNode>());


        /// <summary>
        /// Creates new instance of the given type
        /// </summary>
        /// <typeparam name="NodeType">Node type</typeparam>
        public static NodeType NewInstance<NodeType>(string className) where NodeType : TreeNode, new()
        {
            return (NodeType)mGenerator.CreateNewObject(className);
        }


        /// <summary>
        /// Registers the given document type class
        /// </summary>
        /// <param name="className">Class name to register</param>
        /// <param name="factory">Document factory</param>
        public static void RegisterDocumentType(string className, IDocumentFactory factory)
        {
            mGenerator.RegisterObjectType(className, factory);
        }


        /// <summary>
        /// Registers the given document type class
        /// </summary>
        /// <param name="className">Class name to register</param>
        public static void RegisterDocumentType<NodeType>(string className) where NodeType : TreeNode, new()
        {
            mGenerator.RegisterObjectType<NodeType>(className);
        }


        /// <summary>
        /// Registers the default factory for the documents, which overlaps the default one
        /// </summary>
        /// <param name="factory">Factory to register</param>
        public static void RegisterDefaultFactory(IDocumentFactory factory)
        {
            mGenerator.RegisterDefaultFactory(factory, true);
        }
    }
}
