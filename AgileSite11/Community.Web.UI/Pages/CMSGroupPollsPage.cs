using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Base page for polls under groups pages.
    /// </summary>
    public class CMSGroupPollsPage : CMSGroupPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Polls", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Polls");
            }

            int pollId = QueryHelper.GetInteger("pollId", 0);
            int groupId = QueryHelper.GetInteger("groupId", 0);

            // Check if poll belongs to specified group
            if ((pollId > 0) && (groupId > 0) && !ModuleCommands.PollsPollBelongsToGroup(pollId, groupId))
            {
                RedirectToAccessDenied(GetString("community.group.pollnotassigned"));
            }

            // Check if is any group specified
            if (groupId <= 0)
            {
                RedirectToInformation(GetString("community.group.notspecified"));
            }
        }
    }
}