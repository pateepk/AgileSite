using System;

using CMS.Core;
using CMS.Helpers;

using Newtonsoft.Json;

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// JSON converter for localizing string representation of <see cref="AgeCategoryEnum"/>.
    /// </summary>
    public class LocalizedAgeCategoryJsonConverter : JsonConverter
    {
        private readonly ILocalizationService mLocalizationService;
        
        /// <summary>
        /// Instantiates new instance of <see cref="LocalizedAgeCategoryJsonConverter"/>.
        /// </summary>
        public LocalizedAgeCategoryJsonConverter() : this(Service.Resolve<ILocalizationService>())
        {}


        internal LocalizedAgeCategoryJsonConverter(ILocalizationService localizationService)
        {
            mLocalizationService = localizationService;
        }


        /// <summary>
        /// Writes localized string representation of given <paramref name="value"/> to the <paramref name="writer"/>.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var age = (AgeCategoryEnum)value;
            writer.WriteValue(mLocalizationService.GetString(age.ToStringRepresentation()));
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns <c>true</c> if given <paramref name="objectType"/> is of type <see cref="AgeCategoryEnum"/>; otherwise, <c>false</c>.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AgeCategoryEnum);
        }
    }
}