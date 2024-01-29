namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for document property set action
    /// </summary>
    public class DocumentPropertyAction : BaseDocumentAction
    {
        #region "Parameters"

        /// <summary>
        /// Gets property name
        /// </summary>
        protected virtual string PropertyName
        {
            get
            {
                return GetResolvedParameter<string>("PropertyName", null);
            }
        }


        /// <summary>
        /// Gets property value
        /// </summary>
        protected virtual string PropertyValue
        {
            get
            {
                return GetResolvedParameter<string>("PropertyValue", null);
            }
        }

        #endregion


        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            // Update document property
            if ((SourceNode != null) && !string.IsNullOrEmpty(PropertyName))
            {
                SourceNode.SetValue(PropertyName, PropertyValue);
                SourceNode.Update();
            }
        }
    }
}
