using System;

using CMS.Core;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Factory that provides document objects
    /// </summary>
    public class DocumentFactory<NodeType> : ObjectFactory<NodeType>, IDocumentFactory
         where NodeType : TreeNode, new()
    {
        /// <summary>
        /// Document type
        /// </summary>
        public Type Type 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of the document class</param>
        public DocumentFactory(Type type)
        {
            Type = type;
        }


        /// <summary>
        /// Creates new document type object
        /// </summary>
        public override object CreateNewObject()
        {
            return Activator.CreateInstance(Type);
        }
    }
}
