using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Provides helper methods for mapping between Facebook API entities and CMS objects.
    /// </summary>
    public class FacebookMappingHelper : AbstractHelper<FacebookMappingHelper>
    {

        #region "Private members"

        /// <summary>
        /// Stores the instance of the Facebook user profile mapper for caching purposes.
        /// </summary>
        private IEntityMapper<FacebookUserProfile, UserInfo> mUserProfileMapper;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Retrieves the current mapping between Facebook user profile and CMS user, and returns it.
        /// </summary>
        /// <returns>The current mapping between Facebook user profile and CMS user.</returns>
        public static EntityMapping GetUserProfileMapping()
        {
            return HelperObject.GetUserProfileMappingInternal();
        }


        /// <summary>
        /// Retrieves the current condition when the Facebook user profile mapping occurs, and returns it.
        /// </summary>
        /// <param name="siteName">The site name.</param>
        /// <returns>The current condition when the Facebook user profile mapping occurs.</returns>
        public static FacebookUserProfileMappingTriggerEnum GetUserProfileMappingTrigger(string siteName = null)
        {
            return HelperObject.GetUserProfileMappingTriggerInternal(siteName);
        }


        /// <summary>
        /// Retrieves a model of the Facebook user profile, and returns it.
        /// </summary>
        /// <returns>A model of the Facebook user profile.</returns>
        public static EntityModel GetUserProfileModel()
        {
            return HelperObject.GetUserProfileModelInternal();
        }


        /// <summary>
        /// Retrieves an object that provides mapping between Facebook user profiles to CMS users, and returns it.
        /// </summary>
        /// <returns>An object that provides mapping between Facebook user profiles to CMS users.</returns>
        public static IEntityMapper<FacebookUserProfile, UserInfo> GetUserProfileMapper()
        {
            return HelperObject.GetUserProfileMapperInternal();
        }


        /// <summary>
        /// Maps the values of Facebook user profile attributes to the values of CMS user fields depending on the current mapping.
        /// </summary>
        /// <param name="userProfile">The Facebook user profile.</param>
        /// <param name="user">The CMS user.</param>
        public static void MapUserProfile(FacebookUserProfile userProfile, UserInfo user)
        {
            HelperObject.MapUserProfileInternal(userProfile, user);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Retrieves the current mapping between Facebook user profile and CMS user, and returns it.
        /// </summary>
        /// <returns>The current mapping between Facebook user profile and CMS user.</returns>
        protected virtual EntityMapping GetUserProfileMappingInternal()
        {
            string content = SettingsKeyInfoProvider.GetValue("CMSFacebookUserMapping");
            if (String.IsNullOrEmpty(content))
            {
                return new EntityMapping();
            }
            EntityMappingSerializer serializer = new EntityMappingSerializer();
            return serializer.UnserializeEntityMapping(content);
        }


        /// <summary>
        /// Retrieves the current condition when the Facebook user profile mapping occurs, and returns it.
        /// </summary>
        /// <param name="siteName">The site name.</param>
        /// <returns>The current condition when the Facebook user profile mapping occurs.</returns>
        protected virtual FacebookUserProfileMappingTriggerEnum GetUserProfileMappingTriggerInternal(string siteName = null)
        {
            string name = "CMSFacebookMapUserProfile";
            if (!String.IsNullOrEmpty(siteName))
            {
                name = String.Format("{0}.{1}", siteName, name);
            }

            return (FacebookUserProfileMappingTriggerEnum)SettingsKeyInfoProvider.GetIntValue(name);
        }


        /// <summary>
        /// Retrieves a model of the Facebook user profile, and returns it.
        /// </summary>
        /// <returns>A model of the Facebook user profile.</returns>
        protected virtual EntityModel GetUserProfileModelInternal()
        {
            EntityModelProvider provider = new EntityModelProvider();

            return provider.GetEntityModel<FacebookUserProfile>();
        }


        /// <summary>
        /// Retrieves an object that provides mapping between Facebook user profiles to CMS users, and returns it.
        /// </summary>
        /// <returns>An object that provides mapping between Facebook user profiles to CMS users.</returns>
        protected virtual IEntityMapper<FacebookUserProfile, UserInfo> GetUserProfileMapperInternal()
        {
            if (mUserProfileMapper == null)
            {
                IEntityAttributeValueConverterFactory attributeValueConverterFactory = new EntityAttributeValueConverterFactory();
                IEntityAttributeValueFormatter attributeValueFormatter = new EntityAttributeValueFormatter();
                IFormInfoProvider formInfoProvider = new FormInfoProvider();
                EntityModel userProfileModel = GetUserProfileModelInternal();
                mUserProfileMapper = new EntityMapper<FacebookUserProfile, UserInfo>(userProfileModel, formInfoProvider, attributeValueConverterFactory, attributeValueFormatter);
            }

            return mUserProfileMapper;
        }


        /// <summary>
        /// Maps the values of Facebook user profile attributes to the values of CMS user fields depending on the current mapping.
        /// </summary>
        /// <param name="userProfile">The Facebook user profile.</param>
        /// <param name="user">The CMS user.</param>
        public virtual void MapUserProfileInternal(FacebookUserProfile userProfile, UserInfo user)
        {
            IEntityMapper<FacebookUserProfile, UserInfo> mapper = GetUserProfileMapperInternal();
            EntityMapping mapping = GetUserProfileMappingInternal();
            mapper.Map(userProfile, user, mapping);
        }

        #endregion
    }
}