using System;

using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Loads the data for <see cref="TreeNode"/> components.
    /// </summary>
    internal class ComponentsDataLoader
    {
        /// <summary>
        /// Document instance.
        /// </summary>
        protected readonly TreeNode Document;


        /// <summary>
        /// Creates instance of <see cref="ComponentsDataLoader"/> instance.
        /// </summary>
        /// <param name="document">Document instance.</param>
        public ComponentsDataLoader(TreeNode document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Missing document instance.");
            }

            Document = document;
        }


        /// <summary>
        /// Loads the culture data of given document.
        /// </summary>
        public virtual DocumentCultureDataInfo LoadCultureData()
        {
            return DocumentCultureDataInfo.New();
        }


        /// <summary>
        /// Loads coupled data for given document.
        /// </summary>
        public virtual DocumentFieldsInfo LoadCoupledData()
        {
            if (!Document.IsCoupled)
            {
                return null;
            }

            var className = Document.DataClassInfo.ClassName;
            return DocumentFieldsInfo.New(className);
        }
    }
}