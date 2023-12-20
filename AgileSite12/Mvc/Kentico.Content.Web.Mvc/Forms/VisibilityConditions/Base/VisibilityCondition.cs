using System;
using System.Web.Mvc;

using Kentico.Web.Mvc;
using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a base class for a form component visibility condition.
    /// </summary>
    /// <seealso cref="VisibilityConditionEditingComponentOrder"/>
    [Serializable]
    public abstract class VisibilityCondition : IModelMetadataModifier
    {
        /// <summary>
        /// Gets a value indicating whether a form component is visible.
        /// </summary>
        /// <returns>Returns true if the component is visible, otherwise false.</returns>
        public abstract bool IsVisible();


        /// <summary>
        /// Modifies <paramref name="modelMetadata"/> according to the current object.
        /// </summary>
        /// <remarks>Sets display names for properties handled by editing component. Editing component's label is used.</remarks>
        /// <param name="modelMetadata">Metadata to modify.</param>
        public void ModifyMetadata(ModelMetadata modelMetadata)
        {
            ModelMetadataHelper.ModifyModelMetadata(modelMetadata, this);
        }
    }
}
