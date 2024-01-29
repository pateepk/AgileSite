using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing LinkInfoProvider management.
    /// </summary>
    public class LinkInfoProvider : AbstractInfoProvider<LinkInfo, LinkInfoProvider>, IFullNameInfoProvider
    {
        /// <summary>
        /// Creates a new instance of LinkInfoProvider.
        /// </summary>
        public LinkInfoProvider()
            : base(LinkInfo.TYPEINFO, new HashtableSettings
            {
                FullName = true,
                Load = LoadHashtableEnum.None,
                UseWeakReferences = true
            })
        {
        }


        /// <summary>
        /// Gets a LinkInfo object with specified ID.
        /// </summary>
        /// <param name="linkId">ID of the link</param>        
        public static LinkInfo GetLinkInfo(int linkId)
        {
            return ProviderObject.GetInfoById(linkId);
        }


        /// <summary>
        /// Gets a LinkInfo object with specified GUID.
        /// </summary>
        /// <param name="linkGuid">GUID of the link</param>                
        public static LinkInfo GetLinkInfo(Guid linkGuid)
        {
            return ProviderObject.GetInfoByGuid(linkGuid);
        }


        /// <summary>
        /// Gets a LinkInfo object with specified full name.
        /// </summary>
        /// <param name="linkFullName">Full name of the link</param>
        internal static LinkInfo GetLinkInfo(string linkFullName)
        {
            return ProviderObject.GetInfoByFullName(linkFullName);
        }


        /// <summary>
        /// Gets an object query with all LinkInfo objects.
        /// </summary>
        public static ObjectQuery<LinkInfo> GetLinks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets all links in the specified issue.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <returns>A Dictionary with links from the specified issue, using link URL as the key</returns>
        internal static Dictionary<string, LinkInfo> GetLinks(int issueId)
        {
            var links = GetLinks().WhereEquals("LinkIssueID", issueId).ToList();
            
            if (!links.Any())
            {
                return new Dictionary<string, LinkInfo>();
            }
            
            // Grouping is used in order to ensure only one target per issue is retrieved from DB
            // Otherwise the ToDictionary() method could not be used
            var grouped = links.GroupBy(link => link.LinkTarget);
            return grouped.ToDictionary(group => group.Key, group => group.First());
        }


        /// <summary>
        /// Sets (updates or inserts) a specified LinkInfo object.
        /// </summary>
        /// <param name="linkObj">LinkInfo object to set</param>
        public static void SetLinkInfo(LinkInfo linkObj)
        {
            ProviderObject.SetInfo(linkObj);
        }


        /// <summary>
        /// Deletes a specified LinkInfo object.
        /// </summary>
        /// <param name="linkObj">LinkInfo object to delete</param>
        public static void DeleteLinkInfo(LinkInfo linkObj)
        {
            ProviderObject.DeleteInfo(linkObj);
        }


        /// <summary>
        /// Deletes a LinkInfo object specified by ID.
        /// </summary>
        /// <param name="linkId">ID of the LinkInfo object to delete</param>
        public static void DeleteLinkInfo(int linkId)
        {
            LinkInfo li = GetLinkInfo(linkId);
            if (li != null)
            {
                DeleteLinkInfo(li);
            }
        }


        /// <summary>
        /// Logs that a link was clicked by visitor with specified email.
        /// </summary>
        /// <param name="linkId">ID of the link</param>
        /// <param name="email">Email of visitor that clicked the link.</param>        
        public static void LogClick(int linkId, string email)        
        {
            ProviderObject.LogClickInternal(linkId, email);
        }

        
        /// <summary>
        /// Creates a new dictionary for caching the transformations by the full name.
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(LinkInfo.OBJECT_TYPE, "LinkIssueID;LinkTarget");
        }


        /// <summary>
        /// Gets the where condition that searches the transformation based on the given full name.
        /// </summary>
        /// <param name="fullName">Transformation full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            return new LinkFullNameWhereConditionBuilder(fullName)
                .Build()
                .ToString(true);
        }
        
        
        /// <summary>
        /// Logs that a link was clicked by visitor with specified email.
        /// </summary>
        /// <param name="linkId">ID of the link</param>        
        /// <param name="email">Email of visitor that clicked the link.</param>        
        protected virtual void LogClickInternal(int linkId, string email)
        {
            // Do not create new version on email link click
            using (new CMSActionContext { CreateVersion = false })
            {
                ClickedLinkInfoProvider.SetClickedLinkInfo(new ClickedLinkInfo
                {
                    ClickedLinkEmail = email,
                    ClickedLinkNewsletterLinkID = linkId,
                    ClickedLinkTime = DateTime.Now,
                });
            }
        }

    }
}