using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for validation of serialized <see cref="EditableAreasConfiguration"/>.
    /// </summary>
    [Obsolete("This interface is obsolete. It was not intended for public use. The configuration validation is handled a different way.")]
    public interface IEditableAreasConfigurationValidator
    {
        /// <summary>
        /// Validates serialized <see cref="EditableAreasConfiguration"/>.
        /// </summary>
        /// <param name="configuration">Configuration to validate.</param>
        /// <exception cref="InvalidOperationException"><paramref name="configuration"/> is in incorrect format.</exception>
        void Validate(string configuration);
    }
}
