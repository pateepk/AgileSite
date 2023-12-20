using System;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IVisibilityConditionConfigurationXmlSerializer), typeof(VisibilityConditionConfigurationXmlSerializer), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// XML serializer for <see cref="VisibilityConditionConfiguration"/>.
    /// </summary>
    public interface IVisibilityConditionConfigurationXmlSerializer
    {
        /// <summary>
        /// Serializes a visibility condition configuration to an XML string.
        /// </summary>
        /// <param name="visibilityConditionConfiguration">Visibility condition configuration to be serialized.</param>
        /// <returns>Returns an XML representation of the condition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="visibilityConditionConfiguration"/> is null.</exception>
        string Serialize(VisibilityConditionConfiguration visibilityConditionConfiguration);


        /// <summary>
        /// Deserializes a visibility condition configuration from an XML string.
        /// </summary>
        /// <param name="visibilityConditionConfigurationXml">XML representation of the condition configuration to be deserialized.</param>
        /// <returns>Returns a visibility condition configuration.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="visibilityConditionConfigurationXml"/> is null or an empty string.</exception>
        VisibilityConditionConfiguration Deserialize(string visibilityConditionConfigurationXml);
    }
}
