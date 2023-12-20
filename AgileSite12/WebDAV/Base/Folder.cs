using System;
using System.Collections.Generic;
using System.Data;
using CMS.Community;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.Membership;
using CMS.Base;
using CMS.SiteProvider;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents general folder.
    /// </summary>
    internal class Folder : BaseFolder, IFolder
    {
        #region "Contructors"

        /// <summary>
        /// Creates general folder.
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <param name="name">Folder name</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent folder</param>
        public Folder(string path, string name, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
        }

        #endregion


        #region "IFolder Members"

        /// <summary>
        /// Gets the array of this folder's children.
        /// </summary>
        public IHierarchyItem[] Children
        {
            get
            {
                List<IHierarchyItem> childrenItems = new List<IHierarchyItem>();

                try
                {
                    // Get subfolders for different type of folder
                    switch (UrlParser.ItemType)
                    {
                            // Root folder
                        case ItemTypeEnum.Root:
                            {
                                // Add attachment folder
                                AttachmentFolder attachmentsFolder = new AttachmentFolder(WebDAVSettings.AttachmentsBasePath, WebDAVSettings.AttachmentsFolder, DateTime.Now, DateTime.Now, null, Engine, this);
                                childrenItems.Add(attachmentsFolder);

                                // Add content folder
                                ContentFolder contentFolder = new ContentFolder(WebDAVSettings.ContentFilesBasePath, WebDAVSettings.ContentFilesFolder, DateTime.Now, DateTime.Now, null, Engine, this);
                                childrenItems.Add(contentFolder);

                                if (WebDAVHelper.IsCommunityModuleLoaded)
                                {
                                    // Add groups folder
                                    Folder groupsFolder = new Folder(WebDAVSettings.GroupsBasePath, WebDAVSettings.GroupsFolder, DateTime.Now, DateTime.Now, null, Engine, this);
                                    childrenItems.Add(groupsFolder);
                                }

                                if (WebDAVHelper.IsMediaLibraryModuleLoaded)
                                {
                                    // Add media folder
                                    MediaFolder mediaFolder = new MediaFolder(WebDAVSettings.MediaFilesBasePath, WebDAVSettings.MediaFilesFolder, DateTime.Now, DateTime.Now, null, Engine, this);
                                    childrenItems.Add(mediaFolder);
                                }
                            }
                            break;

                            // 'Groups' folder
                        case ItemTypeEnum.Groups:
                            {
                                if (WebDAVHelper.IsCommunityModuleLoaded)
                                {
                                    AddGroups(childrenItems);
                                }
                            }
                            break;

                            // Group name folder
                        case ItemTypeEnum.GroupName:
                            {
                                // Add 'Attachments' folder
                                string attachmentGroupPath = Path + "/" + WebDAVSettings.AttachmentsFolder;
                                AttachmentFolder attachmentsFolder = new AttachmentFolder(attachmentGroupPath, WebDAVSettings.AttachmentsFolder, DateTime.Now, DateTime.Now, null, Engine, this);
                                childrenItems.Add(attachmentsFolder);

                                // Add 'Pages' folder
                                string contentGroupPath = Path + "/" + WebDAVSettings.PagesFolder;
                                ContentFolder contentGroupFolder = new ContentFolder(contentGroupPath, WebDAVSettings.PagesFolder, DateTime.Now, DateTime.Now, null, Engine, this);
                                childrenItems.Add(contentGroupFolder);

                                if (WebDAVHelper.IsMediaLibraryModuleLoaded)
                                {
                                    // Add 'Media' folder
                                    string mediaGroupPath = Path + "/" + WebDAVSettings.MediaFilesFolder;
                                    MediaFolder mediaFolder = new MediaFolder(mediaGroupPath, WebDAVSettings.MediaFilesFolder, DateTime.Now, DateTime.Now, null, Engine, this);
                                    childrenItems.Add(mediaFolder);
                                }
                            }
                            break;

                            // 'Content' or 'Attachments' folder
                        case ItemTypeEnum.ContentFolder:
                        case ItemTypeEnum.AttachmentFolder:
                            {
                                // Add culture codes as folder
                                foreach (string cultureCode in WebDAVHelper.CulturesCodes)
                                {
                                    childrenItems.Add(GetFolder(UrlParser.ItemType, cultureCode));
                                }
                            }
                            break;

                            // 'Media' folder
                        case ItemTypeEnum.MediaFolder:
                            {
                                if (WebDAVHelper.IsMediaLibraryModuleLoaded)
                                {
                                    AddMediaFolder(childrenItems);
                                }
                            }
                            break;

                            // Nothing
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Folder_Children", "WebDAV", ex);
                }

                return childrenItems.ToArray();
            }
        }


        /// <summary>
        /// Creates new WebDAV resource with the specified name in this folder. 
        /// </summary>
        /// <param name="resourceName">Name of the resource to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateResource(string resourceName)
        {
            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Creates new WebDAV folder with the specified name in this folder. 
        /// </summary>
        /// <param name="folderName">Name of the folder to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateFolder(string folderName)
        {
            return new AccessDeniedResponse();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets content or attachment folder.
        /// </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="folderName">Folder name</param>
        private IHierarchyItem GetFolder(ItemTypeEnum itemType, string folderName)
        {
            switch (itemType)
            {
                case ItemTypeEnum.ContentFolder:
                    {
                        string folderPath = Path + "/" + folderName;
                        ContentFolder contentFolder = new ContentFolder(folderPath, folderName, DateTime.Now, DateTime.Now, null, Engine, this);
                        return contentFolder;
                    }

                case ItemTypeEnum.AttachmentFolder:
                    {
                        string folderPath = Path + "/" + folderName;
                        AttachmentFolder contentFolder = new AttachmentFolder(folderPath, folderName, DateTime.Now, DateTime.Now, null, Engine, this);
                        return contentFolder;
                    }

                default:
                    return null;
            }
        }


        /// <summary>
        /// Add group folders to list.
        /// </summary>
        /// <param name="childrenItems">List of folders</param>
        private void AddGroups(List<IHierarchyItem> childrenItems)
        {
            InfoDataSet<GroupInfo> groups = null;
            const string columns = "GroupName, GroupCreatedWhen, GroupLastModified";

            // Get all groups in site
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Groups", "Manage", SiteContext.CurrentSiteName, false))
            {
                groups = GroupInfoProvider.GetGroups()
                    .WhereEquals("GroupSiteID", SiteContext.CurrentSiteID)
                    .Columns(columns)
                    .TypedResult;
            }
            // Get groups managed by user
            else
            {
                groups = GroupInfoProvider.GetGroupsManagedByUser(MembershipContext.AuthenticatedUser.UserID, SiteContext.CurrentSiteName, null, 0, columns);
            }

            // Check if data source contains groups
            if (!DataHelper.DataSourceIsEmpty(groups))
            {
                foreach (DataRow row in groups.Tables[0].Rows)
                {
                    string groupName = ValidationHelper.GetString(row["GroupName"], string.Empty);
                    DateTime created = ValidationHelper.GetDateTime(row["GroupCreatedWhen"], DateTime.Now);
                    DateTime modified = ValidationHelper.GetDateTime(row["GroupLastModified"], DateTime.Now);

                    // Create folder
                    string groupPath = Path + "/" + groupName;
                    Folder groupFolder = new Folder(groupPath, groupName, created, modified, null, Engine, this);
                    childrenItems.Add(groupFolder);
                }
            }
        }


        /// <summary>
        /// Add media folder to list.
        /// </summary>
        /// <param name="childrenItems">List of folders</param>
        private void AddMediaFolder(List<IHierarchyItem> childrenItems)
        {
            // Check 'Read' permission on Media Library module or current user is group administrator
            if (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.MediaLibrary", "Read", SiteContext.CurrentSiteName)
                || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
            {
                string where = "LibrarySiteID = " + SiteContext.CurrentSiteID;
                DataSet libraries = null;

                if (UrlParser.Group != null)
                {
                    int libraryGroupId = ValidationHelper.GetInteger(UrlParser.Group.GetValue("GroupID"), 0);
                    where = SqlHelper.AddWhereCondition(where, "LibraryGroupID = " + libraryGroupId);
                }
                else
                {
                    where = SqlHelper.AddWhereCondition(where, "LibraryGroupID IS NULL");
                }

                const string columns = "LibraryName, LibraryLastModified";
                // Get media libraries
                libraries = MediaLibraryInfoProvider.GetMediaLibraries(where, null, 0, columns);

                // Check if data set contains libraries
                if (!DataHelper.DataSourceIsEmpty(libraries))
                {
                    foreach (DataRow row in libraries.Tables[0].Rows)
                    {
                        string name = ValidationHelper.GetString(row["LibraryName"], string.Empty);
                        DateTime modified = ValidationHelper.GetDateTime(row["LibraryLastModified"], DateTime.Now);
                        string path = Path + "/" + name;

                        MediaFolder mediaFolder = new MediaFolder(path, name, modified, modified, null, Engine, this);
                        childrenItems.Add(mediaFolder);
                    }
                }
            }
        }

        #endregion
    }
}