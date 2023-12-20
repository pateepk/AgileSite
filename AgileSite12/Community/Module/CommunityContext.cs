using System;

using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.DataEngine;
using System.Data;
using CMS.PortalEngine;

namespace CMS.Community
{
    /// <summary>
    /// Site related context methods and variables.
    /// </summary>
    [RegisterAllProperties]
    public class CommunityContext : AbstractContext<CommunityContext>
    {
        #region "Variables"

        private GroupInfo mCurrentGroup;
        private TreeNode mCurrentDepartment;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current group info object matching groupid, groupguid or groupname specified in the URL parameter of the current request.
        /// </summary>
        public static GroupInfo CurrentGroup
        {
            get
            {
                return GetCurrentGroup();
            }
            set
            {
                Current.mCurrentGroup = value;
            }
        }


        /// <summary>
        /// Returns the current department node.
        /// </summary>
        public static TreeNode CurrentDepartment
        {
            get
            {
                return GetCurrentDepartment();
            }
            set
            {
                Current.mCurrentDepartment = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns information on the current group according the groupid, groupguid or groupname specified as an URL parameter of the current request.
        /// </summary>        
        public static GroupInfo GetCurrentGroup()
        {
            // Try to get the group info from the request items collection
            var c = Current;

            GroupInfo gi = c.mCurrentGroup;
            if (gi == null)
            {
                // Try to get group by its GroupID first
                int groupId = QueryHelper.GetInteger("groupid", 0);
                if (groupId > 0)
                {
                    gi = GroupInfoProvider.GetGroupInfo(groupId);
                }

                // If group was not found by its GroupID
                if (gi == null)
                {
                    // Try to get group by its GroupName
                    string groupName = QueryHelper.GetString("groupname", "");
                    if (groupName != "")
                    {
                        gi = GroupInfoProvider.GetGroupInfo(groupName, SiteContext.CurrentSiteName);
                    }
                }

                if (gi == null)
                {
                    // Try to get group by its GroupName
                    Guid groupGuid = QueryHelper.GetGuid("groupguid", Guid.Empty);
                    if (groupGuid != Guid.Empty)
                    {
                        gi = GroupInfoProvider.GetGroupInfo(groupGuid);
                        if ((gi != null) && (gi.GroupSiteID != SiteContext.CurrentSiteID))
                        {
                            gi = null;
                        }
                    }
                }

                // If group was not found
                if ((gi == null) && (DocumentContext.CurrentPageInfo != null))
                {
                    // Try to get group from current document
                    groupId = DocumentContext.CurrentPageInfo.NodeGroupID;
                    if (groupId > 0)
                    {
                        gi = GroupInfoProvider.GetGroupInfo(groupId);
                    }
                }

                // Save the group to the request items if new group was loaded from DB
                c.mCurrentGroup = gi;
            }

            return gi;
        }


        /// <summary>
        /// Gets the current department document
        /// </summary>
        private static TreeNode GetCurrentDepartment()
        {
            // Try to get from the request
            var c = Current;

            if (c.mCurrentDepartment == null)
            {
                // Check the page info
                PageInfo pi = DocumentContext.CurrentPageInfo;
                if (pi == null)
                {
                    return null;
                }

                // Get the data
                DebugHelper.SetContext("CurrentDepartment");

                string siteName = SiteContext.CurrentSiteName;
                string aliasPath = pi.NodeAliasPath;

                TreeProvider tree = new TreeProvider();
                TreeNode node = GetCurrentDepartment(aliasPath, siteName, tree);

                if (node != null)
                {
                    // Get latest version if not live site mode
                    if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
                    {
                        node = DocumentHelper.GetDocument(node, tree);
                    }
                    else if (!node.IsPublished)
                    {
                        node = null;
                    }
                }

                DebugHelper.ReleaseContext();

                c.mCurrentDepartment = node;
            }

            return c.mCurrentDepartment;
        }


        /// <summary>
        /// Returns department document of the specified document.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="tree">Tree provider</param>
        private static TreeNode GetCurrentDepartment(string aliasPath, string siteName, TreeProvider tree)
        {
            // Get data class to check that doc. type is available
            DataClassInfo dc = DataClassInfoProvider.GetDataClassInfo("IntranetPortal.Department");
            if (dc != null)
            {
                // Ensure tree provider
                if (tree == null)
                {
                    tree = new TreeProvider();
                }

                // Get department document
                string where = TreePathUtils.GetNodesOnPathWhereCondition(aliasPath, false, true).ToString(true);
                DataSet ds = tree.SelectNodes(siteName, "/%", TreeProvider.ALL_CULTURES, true, "IntranetPortal.Department", where, "NodeLevel DESC", -1, false, 1);

                // If found, return the department document
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    return TreeNode.New("IntranetPortal.Department", ds.Tables[0].Rows[0]);
                }
            }

            return null;
        }

        #endregion
    }
}