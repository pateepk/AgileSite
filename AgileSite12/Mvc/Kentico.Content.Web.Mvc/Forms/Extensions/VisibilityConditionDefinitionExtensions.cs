using System;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Extension methods for <see cref="VisibilityConditionDefinition"/> class.
    /// </summary>
    internal static class VisibilityConditionDefinitionExtensions
    {
        /// <summary>
        /// Returns true, if <see cref="VisibilityCondition"/> for given <paramref name="visibilityConditionDefinition"/> is a default visibility condition.
        /// </summary>
        public static bool IsDefaultVisibilityCondition(this VisibilityConditionDefinition visibilityConditionDefinition)
        {
            if (visibilityConditionDefinition == null)
            {
                throw new ArgumentNullException(nameof(visibilityConditionDefinition));
            }

            return typeof(IDefaultVisibilityCondition).IsAssignableFrom(visibilityConditionDefinition.VisibilityConditionType);
        }


        /// <summary>
        /// Returns true, if <see cref="VisibilityCondition"/> for given <paramref name="visibilityConditionDefinition"/> is depending on another field.
        /// </summary>
        public static bool IsDependingOnAnotherField(this VisibilityConditionDefinition visibilityConditionDefinition)
        {
            if (visibilityConditionDefinition == null)
            {
                throw new ArgumentNullException(nameof(visibilityConditionDefinition));
            }

            return visibilityConditionDefinition.VisibilityConditionType.FindTypeByGenericDefinition(typeof(AnotherFieldVisibilityCondition<>)) != null;
        }


        /// <summary>
        /// Returns title for the <see cref="VisibilityCondition"/> defined by <paramref name="visibilityConditionDefinition"/>.
        /// </summary>
        /// <param name="visibilityConditionDefinition">Visibility condition definition.</param>
        /// <param name="formComponent">If a visibility condition depends on another field, then title is created out of the given form component handling that field.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="visibilityConditionDefinition"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="VisibilityCondition"/> defined by <paramref name="visibilityConditionDefinition"/> depends on another field, is not default and <paramref name="formComponent"/> is null.</exception>
        public static string GetVisibilityConditionTitle(this VisibilityConditionDefinition visibilityConditionDefinition, FormComponent formComponent = null)
        {
            if (visibilityConditionDefinition == null)
            {
                throw new ArgumentNullException(nameof(visibilityConditionDefinition));
            }

            bool isDepending = visibilityConditionDefinition.IsDependingOnAnotherField();
            if (formComponent == null && isDepending)
            {
                throw new InvalidOperationException($"Visibility condition definition with identifier '{visibilityConditionDefinition.Identifier}' depends on another field, however, now form component was passed. ");
            }

            if (isDepending)
            {
                return visibilityConditionDefinition.IsDefaultVisibilityCondition() ? ResHelper.LocalizeString(formComponent.GetDisplayName()) : $"{ResHelper.LocalizeString(visibilityConditionDefinition.Name)}: {ResHelper.LocalizeString(formComponent.GetDisplayName())}";
            }

            return ResHelper.GetString(visibilityConditionDefinition.Name);
        }
    }
}
