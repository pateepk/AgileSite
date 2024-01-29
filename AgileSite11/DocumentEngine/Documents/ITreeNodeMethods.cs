namespace CMS.DocumentEngine
{
    /// <summary>
    /// TreeNodesMethods interface
    /// </summary>
    internal interface ITreeNodeMethods
    {
        /// <summary>
        /// Returns true if the document type stands for the product section
        /// </summary>
        bool IsProductSection();

        /// <summary>
        /// Returns true if the document represents a product.
        /// </summary>
        bool IsProduct();

        /// <summary>
        /// Gets the page template id used by this document
        /// </summary>
        string GetUsedPageTemplateIdColumn();

        /// <summary>
        /// Gets the page template id used by this document
        /// </summary>
        int GetUsedPageTemplateId();
    }
}
