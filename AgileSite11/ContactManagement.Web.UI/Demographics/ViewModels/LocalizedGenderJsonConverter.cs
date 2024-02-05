using System;

using CMS.Core;
using CMS.Membership;

using Newtonsoft.Json;

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// JSON converter for localizing string representation of <see cref="UserGenderEnum"/>.
    /// </summary>
    public class LocalizedGenderJsonConverter : JsonConverter
    {
        private readonly ILocalizationService mLocalizationService;

        /// <summary>
        /// Instantiates new instance of <see cref="LocalizedGenderJsonConverter"/>.
        /// </summary>
        public LocalizedGenderJsonConverter() : this(Service.Resolve<ILocalizationService>())
        { }


        internal LocalizedGenderJsonConverter(ILocalizationService localizationService)
        {
            mLocalizationService = localizationService;
        }


        /// <summary>
        /// Writes localized string representation of given <paramref name="value"/> to the <paramref name="writer"/>.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var gender = (UserGenderEnum)value;
            string localizedGender;
            switch (gender)
            {
                case UserGenderEnum.Female:
                    localizedGender = mLocalizationService.GetString("general.female");
                    break;
                case UserGenderEnum.Male:
                    localizedGender = mLocalizationService.GetString("general.male");
                    break;
                default:
                    localizedGender = mLocalizationService.GetString("general.unknown");
                    break;
            }

            writer.WriteValue(localizedGender);
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns <c>true</c> if given <paramref name="objectType"/> is of type <see cref="UserGenderEnum"/>; otherwise, <c>false</c>.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UserGenderEnum);
        }
    }
}