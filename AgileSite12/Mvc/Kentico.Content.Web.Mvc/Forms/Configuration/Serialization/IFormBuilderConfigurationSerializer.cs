using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;

[assembly: RegisterImplementation(typeof(IFormBuilderConfigurationSerializer), typeof(FormBuilderConfigurationSerializer), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Defines serializer of <see cref="FormBuilderConfiguration"/>.
    /// </summary>
    public interface IFormBuilderConfigurationSerializer
    {
        /// <summary>
        /// Serializes Form builder configuration to JSON.
        /// </summary>
        /// <param name="formBuilderConfiguration">Configuration to be serialized to JSON.</param>
        /// <param name="clearFormComponents">If <c>true</c> serialized 'formComponent' tokens will contain only 'identifier' property</param>
        /// <returns>Returns serialized configuration.</returns>
        string Serialize(FormBuilderConfiguration formBuilderConfiguration, bool clearFormComponents = false);


        /// <summary>
        /// Deserializes configuration of a Form builder instance from JSON.
        /// </summary>
        /// <param name="configurationJson">Configuration in JSON to be deserialized.</param>
        /// <returns>Returns deserialzied configuration.</returns>
        FormBuilderConfiguration Deserialize(string configurationJson);


        /// <summary>
        /// Deserializes form component configuration from JSON.
        /// </summary>
        /// <param name="formComponentJson">Configuration in JSON to be deserialized.</param>
        /// <returns>Returns deserialzied configuration.</returns>
        FormComponentConfiguration DeserializeFormComponentConfiguration(string formComponentJson);
    }
}
