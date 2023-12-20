using CMS;
using CMS.Core;
using CMS.OnlineMarketing.Internal;

[assembly: RegisterImplementation(typeof(IABTestConfigurationSerializer), typeof(ABTestConfigurationSerializer), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Defines serializer of <see cref="ABTestConfiguration"/>.
    /// </summary>
    public interface IABTestConfigurationSerializer
    {
        /// <summary>
        /// Serializes an AB test configuration to JSON.
        /// </summary>
        /// <param name="testConfiguration">Configuration to be serialized to JSON.</param>
        /// <returns>Returns serialized configuration.</returns>
        string Serialize(ABTestConfiguration testConfiguration);


        /// <summary>
        /// Deserializes configuration of an AB test instance from JSON.
        /// </summary>
        /// <param name="configurationJson">Configuration in JSON to be deserialized.</param>
        /// <returns>Returns deserialized configuration.</returns>
        ABTestConfiguration Deserialize(string configurationJson);
    }
}
