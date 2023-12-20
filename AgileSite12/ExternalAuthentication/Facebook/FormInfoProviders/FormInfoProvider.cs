using System.Collections.Generic;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Membership;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Provides form info objects suitable for mapping.
    /// </summary>
    public sealed class FormInfoProvider : IFormInfoProvider
    {

        #region "Private members"

        /// <summary>
        /// A dictionary of user field captions with field names as keys.
        /// </summary>
        private Dictionary<string, string> mUserFieldCaptions = new Dictionary<string, string> {
            { "FirstName", "{$general.firstname$}" },
            { "MiddleName", "{$general.middlename$}" },
            { "LastName", "{$general.lastname$}" },
            { "Email", "{$general.email$}" },
            { "PreferredCultureCode", "{$mydesk.myprofile.culture$}" }
        };


        /// <summary>
        /// A set of user system field names suitable for mapping.
        /// </summary>
        private HashSet<string> mUserSystemFieldNames = new HashSet<string> {
            "FirstName",
            "MiddleName",
            "LastName",
            "Email",
            "PreferredCultureCode"
        };


        /// <summary>
        /// A dictionary of user settings field captions with field names as keys.
        /// </summary>
        private Dictionary<string, string> mUserSettingsFieldCaptions = new Dictionary<string, string> {
            { "UserNickName", "{$general.nickname$}" },
            { "UserGender", "{$general.gender$}" },
            { "UserDateOfBirth", "{$general.dateofbirth$}" },
            { "UserDescription", "{$general.description$}" },
            { "UserSignature", "{$general.signature$}" }
        };


        /// <summary>
        /// A set of user settings system field names suitable for mapping.
        /// </summary>
        private HashSet<string> mUserSettingsSystemFieldNames = new HashSet<string> {
            "UserNickName",
            "UserGender",
            "UserDateOfBirth",
            "UserDescription",
            "UserSignature"
        };

        #endregion


        #region "IFormInfoProvider members"

        /// <summary>
        /// Creates a new instance of the form info suitable for mapping, and returns it.
        /// </summary>
        /// <param name="info">The CMS object to create the form info for.</param>
        /// <returns>A new instance of the form info suitable for mapping, if applicable; otherwise, null.</returns>
        public FormInfo GetFormInfo(BaseInfo info)
        {
            return GetFormInfo(info.TypeInfo);
        }


        /// <summary>
        /// Creates a new instance of the form info suitable for mapping, and returns it.
        /// </summary>
        /// <param name="typeInfo">The CMS object type info to create the form info for.</param>
        /// <returns>A new instance of the form info suitable for mapping, if applicable; otherwise, null.</returns>
        public FormInfo GetFormInfo(ObjectTypeInfo typeInfo)
        {
            switch (typeInfo.ObjectClassName.ToLowerInvariant())
            {
                case "cms.user":
                    return GetFormInfoInternal(UserInfo.TYPEINFO, mUserSystemFieldNames, mUserFieldCaptions);
                case "cms.usersettings":
                    return GetFormInfoInternal(UserSettingsInfo.TYPEINFO, mUserSettingsSystemFieldNames, mUserSettingsFieldCaptions);
            }

            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates a new instance of the form info suitable for mapping, and returns it.
        /// </summary>
        /// <param name="typeInfo">The type info to retrieve the original form info from.</param>
        /// <param name="systemFieldNames">A set of system field names suitable for mapping.</param>
        /// <param name="fieldCaptions">A dictionary of field captions with field names as keys.</param>
        /// <returns>A new instance of the form info suitable for mapping.</returns>
        private FormInfo GetFormInfoInternal(ObjectTypeInfo typeInfo, HashSet<string> systemFieldNames, Dictionary<string, string> fieldCaptions)
        {
            FormInfo form = FormHelper.GetFormInfo(typeInfo.ObjectClassName, true);
            List<IDataDefinitionItem> fields = new List<IDataDefinitionItem>();
            foreach (FormFieldInfo field in form.GetFields(true, false))
            {
                if (!field.System || systemFieldNames.Contains(field.Name))
                {
                    string caption = null;
                    if (fieldCaptions.TryGetValue(field.Name, out caption))
                    {
                        field.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, caption);
                    }
                    fields.Add(field);
                }
            }
            form.ItemsList.Clear();
            form.ItemsList.AddRange(fields);

            return form;
        }

        #endregion

    }

}