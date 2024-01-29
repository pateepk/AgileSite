using System.Configuration;
using System.Web.Configuration;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterModuleUsageDataSource(typeof(MembershipUsageDataSource))]

namespace CMS.Membership
{
    /// <summary>
    /// Module usage data for membership.
    /// </summary>
    internal class MembershipUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Membership usage data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Membership";
            }
        }


        /// <summary>
        /// Get membership usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            // Get current authentication mode
            string mode = null;
         
            if (RequestHelper.IsMixedAuthentication())
            {
                mode = "Mixed";
            }
            else
            {
                var section = ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection;
                if (section != null)
                {
                    mode = section.Mode.ToString();
                }
            }

            result.Add("AuthenticationMode", mode);

            return result;
        }
    }
}
