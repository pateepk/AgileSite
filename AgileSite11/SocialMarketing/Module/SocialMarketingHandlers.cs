using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.LicenseProvider;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides handlers for social marketing.
    /// </summary>
    internal static class SocialMarketingHandlers
    {

        #region "Public methods"

        /// <summary>
        /// Initializes the social marketing handlers.
        /// </summary>
        public static void Init()
        {
            WorkflowEvents.Publish.After += Workflow_Publish_After_Facebook;
            WorkflowEvents.Publish.After += Workflow_Publish_After_LinkedIn;
            WorkflowEvents.Publish.After += Workflow_Publish_After_Twitter;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Event handler will post to Facebook after publishing the document.
        /// </summary>
        private static void Workflow_Publish_After_Facebook(object sender, WorkflowEventArgs e)
        {
            if ((e.Document == null) || !LicenseKeyInfoProvider.IsFeatureAvailable(FeatureEnum.SocialMarketing))
            {
                return;
            }

            var document = e.Document;
            List<FacebookPostInfo> posts = FacebookPostInfoProvider.GetFacebookPostInfosByDocumentGuid(document.DocumentGUID, document.NodeSiteID).ToList();
            posts.ForEach(post =>
            {
                if (post.FacebookPostPostAfterDocumentPublish && !post.IsPublished && !post.IsFaulty)
                {
                    try
                    {
                        FacebookPostInfoProvider.PublishFacebookPostToFacebook(post.FacebookPostID);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogWarning("Social marketing - Facebook post", "PUBLISHPOST", ex, post.FacebookPostSiteID,
                            String.Format("An error occurred while publishing the Facebook post with ID {0} after publishing the page with ID {1}.", post.FacebookPostID, document.DocumentID));
                    }
                }
            });
        }


        /// <summary>
        /// Event handler will post to LinkedIn after publishing the document.
        /// </summary>
        private static void Workflow_Publish_After_LinkedIn(object sender, WorkflowEventArgs e)
        {
            if ((e.Document == null) || !LicenseKeyInfoProvider.IsFeatureAvailable(FeatureEnum.SocialMarketing))
            {
                return;
            }

            var document = e.Document;
            List<LinkedInPostInfo> posts = LinkedInPostInfoProvider.GetLinkedInPostInfosByDocumentGuid(document.DocumentGUID, document.NodeSiteID).ToList();
            posts.ForEach(post =>
            {
                if (post.LinkedInPostPostAfterDocumentPublish && !post.IsPublished && !post.IsFaulty)
                {
                    try
                    {
                        LinkedInPostInfoProvider.PublishLinkedInPostToLinkedIn(post.LinkedInPostID);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogWarning("Social marketing - LinkedIn post", "PUBLISHPOST", ex, post.LinkedInPostSiteID,
                            String.Format("An error occurred while publishing the LinkedIn post with ID {0} after publishing the page with ID {1}.", post.LinkedInPostID, document.DocumentID));
                    }
                }
            });
        }


        /// <summary>
        /// Event handler will post to Twitter after publishing the document.
        /// </summary>
        private static void Workflow_Publish_After_Twitter(object sender, WorkflowEventArgs e)
        {
            if ((e.Document == null) || !LicenseKeyInfoProvider.IsFeatureAvailable(FeatureEnum.SocialMarketing))
            {
                return;
            }

            var document = e.Document;
            List<TwitterPostInfo> posts = TwitterPostInfoProvider.GetTwitterPostInfosByDocumentGuid(document.DocumentGUID, document.NodeSiteID).ToList();
            posts.ForEach(post =>
            {
                if (post.TwitterPostPostAfterDocumentPublish && !post.IsPublished && !post.IsFaulty)
                {
                    try
                    {
                        TwitterPostInfoProvider.PublishTwitterPostToTwitter(post.TwitterPostID);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogWarning("Social marketing - Twitter post", "PUBLISHPOST", ex, post.TwitterPostSiteID,
                            String.Format("An error occurred while publishing the Twitter post with ID {0} after publishing the page with ID {1}.", post.TwitterPostID, document.DocumentID));
                    }
                }
            });
        }

        #endregion

    }
}
