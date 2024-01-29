using CMS.DocumentEngine;
using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Factory that provides SKUTreeNode for product document types
    /// </summary>
    public class SKUTreeNodeFactory : DocumentFactory<SKUTreeNode>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SKUTreeNodeFactory()
            : base(typeof(SKUTreeNode))
        {
        }


        /// <summary>
        /// Returns true if the factory is able to create the object based on the given parameter
        /// </summary>
        /// <param name="parameter">Object parameter</param>
        public override bool CanCreateObject(object parameter)
        {
            return CanCreateObject((string)parameter);
        }


        /// <summary>
        /// Returns true if the factory is able to create the object of the given type
        /// </summary>
        /// <param name="objectType">Object type</param>
        protected bool CanCreateObject(string objectType)
        {
            if (!base.CanCreateObject(objectType))
            {
                return false;
            }

            // Get the document type data class info
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(objectType);

            return (dci != null) && dci.ClassIsProduct && dci.ClassIsDocumentType;
        }
    }
}
