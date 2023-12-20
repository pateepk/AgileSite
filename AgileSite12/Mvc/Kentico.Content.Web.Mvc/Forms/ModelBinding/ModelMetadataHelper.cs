using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using CMS.Base;
using CMS.Helpers;

using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.AnnotationExtensions;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Helps to work with <see cref="ModelMetadata"/>.
    /// </summary>
    internal class ModelMetadataHelper : AbstractHelper<ModelMetadataHelper>
    {
        /// <summary>
        /// Modifies <paramref name="modelMetadata"/> of the given <paramref name="validationRule"/>.
        /// </summary>
        public static void ModifyModelMetadata(ModelMetadata modelMetadata, ValidationRule validationRule)
        {
            HelperObject.ModifyModelMetadataInternal(modelMetadata, validationRule);
        }


        /// <summary>
        /// Modifies <paramref name="modelMetadata"/> of the given <paramref name="visibilityCondition"/>.
        /// </summary>
        public static void ModifyModelMetadata(ModelMetadata modelMetadata, VisibilityCondition visibilityCondition)
        {
            HelperObject.ModifyModelMetadataInternal(modelMetadata, visibilityCondition);
        }


        /// <summary>
        /// Modifies <paramref name="modelMetadata"/> of the given <paramref name="validationRule"/>.
        /// </summary>
        protected virtual void ModifyModelMetadataInternal(ModelMetadata modelMetadata, ValidationRule validationRule)
        {
            ModifyModelMetadataCore(modelMetadata, validationRule);
        }


        /// <summary>
        /// Modifies <paramref name="modelMetadata"/> of the given <paramref name="visibilityCondition"/>.
        /// </summary>
        protected virtual void ModifyModelMetadataInternal(ModelMetadata modelMetadata, VisibilityCondition visibilityCondition)
        {
            ModifyModelMetadataCore(modelMetadata, visibilityCondition);
        }


        private void ModifyModelMetadataCore(ModelMetadata modelMetadata, object component)
        {
            var editableProperties = component.GetAnnotatedProperties<EditingComponentAttribute>(false).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var propertyModelMetadata in modelMetadata.Properties.Where(p => editableProperties.ContainsKey(p.PropertyName) && String.IsNullOrEmpty(p.DisplayName)))
            {
                var property = editableProperties[propertyModelMetadata.PropertyName];
                var attribute = property.GetCustomAttribute<EditingComponentAttribute>(false);
                if (attribute == null)
                {
                    continue;
                }

                if (!String.IsNullOrEmpty(attribute.Label))
                {
                    propertyModelMetadata.DisplayName = ResHelper.LocalizeString(attribute.Label);
                }
            }
        }
    }
}
