using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

using Facebook;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Provider allows you to get information about Facebook user.
    /// </summary>
    public class FacebookUserProvider
    {
        #region "Public methods"

        /// <summary>
        /// Gets public information about Facebook user. 
        /// </summary>
        /// <param name="facebookUserId">Facebook user's ID.</param>
        /// <returns>Facebook user data class</returns>
        public FacebookUserProfile GetFacebookUser(string facebookUserId)
        {
            return GetFacebookUser(facebookUserId, null, null);
        }


        /// <summary>
        /// Gets information about Facebook user. Access token define what information can be retrieved.
        /// </summary>
        /// <param name="facebookUserId">Facebook user's ID.</param>
        /// <param name="accessToken">User's access token.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook user data class.</returns>
        public FacebookUserProfile GetFacebookUser(string facebookUserId, string accessToken, string appSecret)
        {
            var client = new FacebookClient();
            if (!String.IsNullOrEmpty(accessToken))
            {
                client.AccessToken = accessToken;
            }

            var facebookUser = new FacebookUserProfile();
            IDictionary<PropertyInfo, FacebookAttribute> propertyInfoAttributes = typeof (FacebookUserProfile).GetProperties()
                .ToDictionary(x => x, x => x.GetCustomAttributes(typeof (FacebookAttribute), false).Cast<FacebookAttribute>().FirstOrDefault());

            var response = (IDictionary<string, object>)client.Get(facebookUserId, new
            {
                fields = String.Join(",", propertyInfoAttributes.Select(x => x.Value.ResponsePropertyName)),
                appsecret_proof = SecurityHelper.GetHMACSHA2Hash(accessToken, Encoding.UTF8.GetBytes(appSecret))
            });
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var propertyInfoAttribute in propertyInfoAttributes)
            {
                var propertyInfo = propertyInfoAttribute.Key;
                var attribute = propertyInfoAttribute.Value;
                if ((attribute == null) || !propertyInfo.CanWrite || !response.ContainsKey(attribute.ResponsePropertyName))
                {
                    continue;
                }
                
                object responseValue = response[attribute.ResponsePropertyName];
                if (responseValue == null)
                {
                    continue;
                }

                if (IsType<String>(propertyInfo))
                {
                    string value;
                    switch (attribute.ResponsePropertyName)
                    {
                        case "location":
                            value = ((IDictionary<string, object>) responseValue)["name"] as string;
                            break;
                                
                        default:
                            value = responseValue.ToString();
                            break;
                    }
                    propertyInfo.SetValue(facebookUser, value, null);
                }
                else if (IsType<DateTime>(propertyInfo))
                {
                    DateTime date = DateTime.MinValue;
                    bool parsed = false;
                    switch (attribute.ResponsePropertyName)
                    {
                        case "updated_time":
                            parsed = DateTime.TryParse(responseValue as string, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                            break;

                        case "birthday":
                            parsed = DateTime.TryParseExact(responseValue as string, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                            break;

                        default:
                            stringBuilder.AppendFormat("Facebook response property '{0}' can not be converted to DateTime.", attribute.ResponsePropertyName);
                            break;
                    }

                    if (parsed)
                    {
                        propertyInfo.SetValue(facebookUser, date, null);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("Facebook response property '{0}' with value '{1}' could not be converted to DateTime.", attribute.ResponsePropertyName, responseValue);
                    }
                }
                else if (IsType<bool>(propertyInfo))
                {
                    if (attribute.ResponsePropertyName == "verified")
                    {
                        propertyInfo.SetValue(facebookUser, responseValue, null);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("Facebook response property '{0}' can not be converted to Boolean.", attribute.ResponsePropertyName);
                    }
                }
                else if (IsType<UserGenderEnum>(propertyInfo))
                {
                    if (attribute.ResponsePropertyName == "gender")
                    {
                        UserGenderEnum gender = UserGenderEnum.Unknown;
                        switch (responseValue.ToString())
                        {
                            case "male":
                                gender = UserGenderEnum.Male;
                                break;

                            case "female":
                                gender = UserGenderEnum.Female;
                                break;
                        }
                        propertyInfo.SetValue(facebookUser, gender, null);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("Facebook response property '{0}' can not be converted to UserGenderEnum.", attribute.ResponsePropertyName);
                    }
                }
            }

            if (stringBuilder.Length > 0)
            {
                EventLogProvider.LogWarning("SocialMedia.FacebookUserProvider", "GETFACEBOOKUSER", null, SiteContext.CurrentSiteID, stringBuilder.ToString());
            }

            return facebookUser;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Indicates whether the given property is type T or nullable type T.
        /// </summary>
        /// <typeparam name="T">Type to test.</typeparam>
        /// <param name="propertyInfo">Property to test.</param>
        private bool IsType<T>(PropertyInfo propertyInfo)
        {
            return (propertyInfo.PropertyType == typeof(T) || Nullable.GetUnderlyingType(propertyInfo.PropertyType) == typeof(T));
        }

        #endregion
    }
}
