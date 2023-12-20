using System;

using CMS.Base;
using CMS.Helpers;
using CMS.Helpers.Internal;

namespace CMS.Membership
{
    /// <summary>
    /// Virtual context user parser.
    /// </summary>
    internal static class VirtualContextUserParser
    {
        /// <summary>
        /// Initializes the <see cref="VirtualContextUserParser" />.
        /// </summary>
        public static void Init()
        {
            VirtualContext.UserParsing.Execute += OnUserParsing_Execute;
        }


        /// <summary>
        /// Parses user's userGuid/userName from <paramref name="args"/> parameter.
        /// </summary>
        private static void OnUserParsing_Execute(object sender, CMSEventArgs<VirtualContextUserArgs> args)
        {
            var userNameOrGuid = args.Parameter.UserNameOrGuid;

            if (!string.IsNullOrEmpty(userNameOrGuid))
            {
                // Try to search the user by userName first then by userGuid if provided value is a valid GUID
                var user = UserInfoProvider.GetUserInfo(userNameOrGuid)
                    ?? (Guid.TryParse(userNameOrGuid, out Guid userGuid)
                        ? UserInfoProvider.GetUserInfoByGUID(userGuid)
                        : null);

                args.Parameter.UserName = user?.UserName;
                args.Parameter.UserGuid = user?.UserGUID ?? Guid.Empty;
            }
        }
    }
}
