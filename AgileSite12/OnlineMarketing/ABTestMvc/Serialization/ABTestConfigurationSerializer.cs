using System;

using CMS.EventLog;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Serializer of <see cref="ABTestConfiguration"/>.
    /// </summary>
    public class ABTestConfigurationSerializer : IABTestConfigurationSerializer
    {
        /// <summary>
        /// Serializes an AB test configuration to JSON.
        /// </summary>
        /// <param name="testConfiguration">Configuration to be serialized to JSON.</param>
        /// <returns>Returns serialized configuration.</returns>
        public string Serialize(ABTestConfiguration testConfiguration)
        {
            if (testConfiguration == null)
            {
                throw new ArgumentNullException(nameof(testConfiguration));
            }

            return JsonConvert.SerializeObject(testConfiguration, Formatting.None, GetSettings());
        }


        /// <summary>
        /// Deserializes configuration of an AB test instance from JSON.
        /// </summary>
        /// <param name="configurationJson">Configuration in JSON to be deserialized.</param>
        /// <returns>Returns deserialized configuration.</returns>
        public ABTestConfiguration Deserialize(string configurationJson)
        {
            if (configurationJson == null)
            {
                throw new ArgumentNullException(nameof(configurationJson));
            }

            try
            {
                return JsonConvert.DeserializeObject<ABTestConfiguration>(configurationJson, GetSettings());
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(nameof(ABTestConfigurationSerializer), "DESERIALIZATION", ex, additionalMessage: $"Deserialization of AB test configuration failed. The original JSON string follows.{Environment.NewLine}{Environment.NewLine}{configurationJson}");

                throw new InvalidOperationException($"Deserialization of AB test configuration failed. See event log exception for details.", ex);
            }
        }


        /// <summary>
        /// Gets serializer settings used to serialize and deserialize <see cref="ABTestConfiguration"/> objects.
        /// </summary>
        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new ABTestConfigurationTypesBinder()
            };
        }
    }
}
