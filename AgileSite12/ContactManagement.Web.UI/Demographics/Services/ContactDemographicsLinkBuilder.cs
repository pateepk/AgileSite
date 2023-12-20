using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;

namespace CMS.ContactManagement.Web.UI
{
    internal class ContactDemographicsLinkBuilder : IContactDemographicsLinkBuilder
    {
        private readonly IUILinkProvider mUiLinkProvider;

        public ContactDemographicsLinkBuilder(IUILinkProvider uiLinkProvider)
        {
            mUiLinkProvider = uiLinkProvider;
        }


        public string GetDemographicsLink(string retrieverIdentifier, NameValueCollection additionalParameters = null)
        {
            if (retrieverIdentifier == null)
            {
                throw new ArgumentNullException(nameof(retrieverIdentifier));
            }

            var link = mUiLinkProvider.GetSingleObjectLink(ModuleName.CONTACTMANAGEMENT, "ContactDemographics", new ObjectDetailLinkParameters
            {
                Persistent = true
            });
            
            link += $"&{GetQueryString(retrieverIdentifier, additionalParameters)}";
            return link;
        }


        private string GetQueryString(string retrieverIdentifier, NameValueCollection additionalParameters)
        {
            additionalParameters = additionalParameters ?? new NameValueCollection();
            additionalParameters.Add("retrieverIdentifier", retrieverIdentifier);

            return string.Join("&", additionalParameters.AllKeys.Select(a => a + "=" + HttpUtility.UrlEncode(additionalParameters[a])));
        }
    }
}