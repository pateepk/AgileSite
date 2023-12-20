using System.Web.Mvc;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Defines an interface for objects to customize model metadata when they are being created.
    /// </summary>
    public interface IModelMetadataModifier
    {
        /// <summary>
        /// Allows to customize passed model metadata by object for which the metadata are being created.
        /// </summary>
        /// <param name="modelMetadata">Metadata to modify.</param>
        void ModifyMetadata(ModelMetadata modelMetadata);
    }
}
