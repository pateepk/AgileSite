using System;

namespace CMS
{
    /// <summary>
    /// Marks a method in class implementing this interface and deriving from <see cref="Attribute"/> as suitable to be executed during application initialization.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public interface IInitAttribute
    {
        /// <summary>
        /// Type marked with this attribute.
        /// </summary>
        Type MarkedType
        {
            get;
        }

        /// <summary>
        /// Method to be executed during application pre-initialization.
        /// </summary>
        void Init();
    }
}