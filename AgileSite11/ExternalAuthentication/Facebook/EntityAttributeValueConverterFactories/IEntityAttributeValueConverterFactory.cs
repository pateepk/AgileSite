namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Creates entity attribute value converters corresponding to entity attribute models.
    /// </summary>
    public interface IEntityAttributeValueConverterFactory
    {
        #region "Methods"

        /// <summary>
        /// Creates an entity attribute value converter that corresponds to the specified entity attribute model.
        /// </summary>
        /// <param name="attributeModel">The entity attribute model.</param>
        /// <returns>An entity attribute value converter that corresponds to the specified entity attribute model, if applicable; otherwise, null.</returns>
        EntityAttributeValueConverterBase CreateConverter(EntityAttributeModel attributeModel);

        #endregion
    }

}