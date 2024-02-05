using System.Collections.Generic;

using CMS.Base;

using Newtonsoft.Json;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Helps to generate JSON output for Google Tag Manager.
    /// </summary>
    public class GtmDataHelper : AbstractHelper<GtmDataHelper>
    {
        /// <summary>
        /// Serializes <paramref name="gtmData"/> to JSON.
        /// </summary>
        /// <param name="gtmData">Data to be serialized to JSON.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>JSON representation of <paramref name="gtmData"/>.</returns>
        /// <seealso cref="SerializeToJsonInternal(GtmData, string)"/>
        public static string SerializeToJson(GtmData gtmData, string purpose = null)
        {
            return HelperObject.SerializeToJsonInternal(gtmData, purpose);
        }


        /// <summary>
        /// Serializes collection of <see cref="GtmData"/> to array of JSONs.
        /// </summary>
        /// <param name="gtmDataCollection">Data to be serialized to array of JSONs.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>Array of JSONs.</returns>
        /// <seealso cref="SerializeToJsonInternal(IEnumerable{GtmData}, string)"/>
        public static string SerializeToJson(IEnumerable<GtmData> gtmDataCollection, string purpose = null)
        {
            return HelperObject.SerializeToJsonInternal(gtmDataCollection, purpose);
        }

        /// <summary>
        /// Serializes <paramref name="gtmData"/> to JSON.
        /// </summary>
        /// <param name="gtmData">Data to be serialized to JSON.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>JSON representation of <paramref name="gtmData"/>.</returns>
        /// <example>
        /// <para>To customize gtmData before serialization, override this method in similar fashion.</para>
        /// <para>gtmData.Add("key", "value");</para>
        /// <para>base.SerializeInternal(gtmObject, purpose);</para>
        /// <para>To customize the whole serialization process do not call base implementation and implement custom gtmData serialization.</para>
        /// </example>
        /// <returns>The <paramref name="gtmData"/> serialized to string.</returns>
        protected virtual string SerializeToJsonInternal(GtmData gtmData, string purpose = null)
        {
            return JsonConvert.SerializeObject(gtmData);
        }


        /// <summary>
        /// Serializes collection of <see cref="GtmData"/> to array of JSONs.
        /// </summary>
        /// <param name="gtmDataCollection">Data to be serialized to array of JSONs.</param>
        /// <param name="purpose">Contextual information fitting for customizations.</param>
        /// <returns>Array of JSONs.</returns>
        protected virtual string SerializeToJsonInternal(IEnumerable<GtmData> gtmDataCollection, string purpose = null)
        {
            return JsonConvert.SerializeObject(gtmDataCollection);
        }
    }
}
