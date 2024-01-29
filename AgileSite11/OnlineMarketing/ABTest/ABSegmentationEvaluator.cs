using System;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class that evaluates AB segmentation conditions.
    /// </summary>
    internal static class ABSegmentationEvaluator
    {
        /// <summary>
        /// Returns true if visitor is editor.
        /// </summary>
        internal static bool CheckUserIsEditor()
        {
            return MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns false if visitor did not pass traffic condition.
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> is null</exception>
        internal static bool CheckIncludedTrafficCondition(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            // If 100%, do not check and return selected variant
            if (abTest.ABTestIncludedTraffic == 100)
            {
                return true;
            }

            // Generate random number to decide whether include visitor in test or not
            int includeTraffic = StaticRandom.Next(1, 101);
            if (abTest.ABTestIncludedTraffic < includeTraffic)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Returns false if visitor did not pass targeting macro condition.
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> is null</exception>
        internal static bool CheckVisitorTargetingMacro(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            if (!String.IsNullOrEmpty(abTest.ABTestVisitorTargeting))
            {
                MacroResolver resolver = MacroContext.GlobalResolver.CreateChild();
                resolver.SetNamedSourceData("Contact", ContactManagementContext.CurrentContact);
                resolver.SetNamedSourceData("CurrentUser", MembershipContext.AuthenticatedUser);

                // Check macro condition to decide whether include visitor in test or not
                if (!ValidationHelper.GetBoolean(resolver.ResolveMacros(abTest.ABTestVisitorTargeting), true))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
