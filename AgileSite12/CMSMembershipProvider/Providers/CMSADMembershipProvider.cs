using System;
using System.Collections.Specialized;
using System.Web.Security;

using CMS.Membership;

namespace CMS.MembershipProvider
{
    /// <summary>
    /// Active directory membership provider.
    /// </summary>
    public class CMSADMembershipProvider : ActiveDirectoryMembershipProvider
    {
        /// <summary>
        /// Initializes the membership provider.
        /// </summary>
        /// <param name="name">Provider name</param>
        /// <param name="config">Configuration</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config != null)
            {
                if ((String.IsNullOrEmpty(config["attributeMapUsername"]) && (CMSMembershipHelper.ADDefaultMapUserNameInternal != string.Empty)))
                {
                    config["attributeMapUsername"] = CMSMembershipHelper.ADDefaultMapUserNameInternal;
                }

                AuthenticationHelper.ADConnectionStringName = config["connectionStringName"];
                AuthenticationHelper.ADUsername = config["connectionUsername"];
                AuthenticationHelper.ADPassword = config["connectionPassword"];

                base.Initialize(name, config);
            }
        }
    }
}