using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class provides Continuous integration extensions for <see cref="ObjectTypeInfo"/> and is meant for this single purpose.
    /// </summary>
    public static class ObjectTypeInfoExtensions
    {
        /// <summary>
        /// Returns true if given <paramref name="typeInfo"/> supports continuous integration.
        /// </summary>
        public static bool SupportsContinuousIntegration(this ObjectTypeInfo typeInfo)
        {
            return typeInfo.ContinuousIntegrationSettings.Enabled;
        }
    }
}
