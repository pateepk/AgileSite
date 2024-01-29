using System;
using System.Linq;

using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.SiteProvider;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Action to publish message to Facebook.
    /// </summary>
    public class FacebookAction : DocumentWorkflowAction
    {
        #region "Properties"
        
        /// <summary>
        /// Text to be published.
        /// </summary>
        public string Text
        {
            get
            {
                return GetResolvedParameter("Text", String.Empty);
            }
        }


        /// <summary>
        /// Gets the type of the URL shortener that will be used to shorten links in this Facebook post.
        /// </summary>
        public URLShortenerTypeEnum UrlShortenerType
        {
            get
            {
                return (URLShortenerTypeEnum)GetResolvedParameter("URLShortenerType", 0);
            }
        }


        /// <summary>
        /// If true, the action will verify that there is no post related to the document yet. If there are any, no action is executed.
        /// </summary>
        public bool PostOnlyOnce
        {
            get
            {
                return GetResolvedParameter("PostOnlyOnce", false);
            }
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Executes action.
        /// </summary>
        public override void Execute()
        {
            int postSiteId = Node.OriginalNodeSiteID;
            FacebookAccountInfo accountInfo = FacebookAccountInfoProvider.GetDefaultFacebookAccount(postSiteId);
            if (accountInfo == null)
            {
                EventLogProvider.LogWarning("Social marketing - Advanced workflow action", "PUBLISHPOST", null, SiteContext.CurrentSiteID,
                    String.Format("Advanced workflow publishing to Facebook could not be processed because of missing default Facebook page."));

                return;
            }

            if (PostOnlyOnce && FacebookPostInfoProvider.GetFacebookPostInfosByDocumentGuid(Node.DocumentGUID, postSiteId).Any())
            {
                //If the option to post only one post is selected and a post related to a document(page) in the same culture already exists, do nothing

                return;
            }

            FacebookPostInfo postInfo = new FacebookPostInfo
            {
                FacebookPostFacebookAccountID = accountInfo.FacebookAccountID,
                FacebookPostDocumentGUID = Node.DocumentGUID,
                FacebookPostSiteID = postSiteId,
                FacebookPostText = Text,
                FacebookPostURLShortenerType = UrlShortenerType
            };
            FacebookPostInfoProvider.SetFacebookPostInfo(postInfo);
            FacebookPostInfoProvider.PublishFacebookPost(postInfo.FacebookPostID);
        }

        #endregion
    }
}
