using System;

using CMS.Activities;
using CMS.ApplicationDashboard.Web.UI;
using CMS.ContactManagement.Web.UI;
using CMS.Core;
using CMS.Helpers;

[assembly: RegisterLiveTileModelProvider(ModuleName.ONLINEMARKETING, "ContactsFrameset", typeof(ContactManagementLiveTileModelProvider))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides live model for the Contact management dashboard tile.
    /// </summary>
    internal class ContactManagementLiveTileModelProvider : ILiveTileModelProvider
    {
        /// <summary>
        /// Loads model for the dashboard live tile.
        /// </summary>
        /// <param name="liveTileContext">Context of the live tile. Contains information about the user and the site the model is requested for</param>
        /// <exception cref="ArgumentNullException"><paramref name="liveTileContext"/> is null</exception>
        /// <returns>Live tile model</returns>
        public LiveTileModel GetModel(LiveTileContext liveTileContext)
        {
            if (liveTileContext == null)
            {
                throw new ArgumentNullException("liveTileContext");
            }
            
            return CacheHelper.Cache(() =>
            {
                if (!ActivitySettingsHelper.OnlineMarketingEnabled(liveTileContext.SiteInfo.SiteName))
                {
                    return null;
                }

                var newContactsCount = GetNewContactsCount();

                return new LiveTileModel
                {
                    Value = newContactsCount,
                    Description = ResHelper.GetString("contactmanagement.livetiledescription"),
                };
            }, new CacheSettings(2, "ContactManagementLiveTileModelProvider", liveTileContext.SiteInfo.SiteID));
        }


        /// <summary>
        /// Gets total number of new contacts from the last week.
        /// </summary>
        /// <returns>Number of new contacts</returns>
        private static int GetNewContactsCount()
        {
            return ContactInfoProvider.GetContacts()
                                      .CreatedAfter(DateTime.Now.AddDays(-7).Date)
                                      .Count;
        }
    }
}