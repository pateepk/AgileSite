using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the CMS Event manager pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSEventManagerPage : CMSDeskPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckDocPermissions = false;

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.EventManager);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.EventManager", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.EventManager");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools -> EventManager
            if (!user.IsAuthorizedPerUIElement("CMS.EventManager", "EventManager"))
            {
                RedirectToUIElementAccessDenied("CMS.EventManager", "EventManager");
            }

            // Check existence of CMS.BookingEvent dataclass
            if (DataClassInfoProvider.GetDataClassInfo("CMS.BookingEvent") != null)
            {
                // Check that any eventID is specified
                int eventNodeId = QueryHelper.GetInteger("eventId", 0);
                if (eventNodeId > 0)
                {
                    // Check that event is placed on current site
                    DataSet ds = ModuleCommands.EventsGetSiteEvent(eventNodeId, SiteContext.CurrentSiteName, "NodeID");
                    if (DataHelper.DataSourceIsEmpty(ds))
                    {
                        RedirectToInformation(ResHelper.GetString("events_edit.notfound"));
                    }
                }

                // Check READ permission
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.EventManager", "Read"))
                {
                    RedirectToAccessDenied("CMS.EventManager", "Read");
                }
            }
            // Document type Event doesn't exist
            else
            {
                RedirectToInformation(ResHelper.GetString("events_edit.classnotfound"));
            }
        }


        /// <summary>
        /// Check permission for current event specified in query string and current user.
        /// </summary>
        /// <param name="permissionName">Permission name to check</param>
        protected void CheckPermission(string permissionName)
        {
            // Check existence of CMS.BookingEvent dataclass
            if (DataClassInfoProvider.GetDataClassInfo("CMS.BookingEvent") != null)
            {
                // Check that any eventID is specified
                int eventNodeId = QueryHelper.GetInteger("eventId", 0);
                if (eventNodeId > 0)
                {
                    // Check that event is placed on current iste
                    DataSet ds = ModuleCommands.EventsGetSiteEvent(eventNodeId, SiteContext.CurrentSiteName, "NodeID");
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Check permission
                        if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.EventManager", permissionName))
                        {
                            RedirectToAccessDenied("CMS.EventManager", permissionName);
                        }
                    }
                    // Event is not placed on current site or not found
                    else
                    {
                        RedirectToInformation(ResHelper.GetString("events_edit.notfound"));
                    }
                }
                // Event ID not found - check permission
                else
                {
                    if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.EventManager", permissionName))
                    {
                        RedirectToAccessDenied("CMS.EventManager", permissionName);
                    }
                }
            }
            // Document type Event doesn't exist
            else
            {
                RedirectToInformation(ResHelper.GetString("events_edit.classnotfound"));
            }
        }
    }
}