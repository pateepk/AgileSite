using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// This class generates URLs which then performs mass action selected on a list of documents.
    /// </summary>
    public class DocumentListMassActionsUrlGenerator
    {
        private enum ArchivePublishMassActionEnum
        {
            Publish,
            Archive,
        }


        private enum MoveCopyLinkMassActionEnum
        {
            Move,
            Copy,
            Link,
        }


        #region "Public methods"


        /// <summary>
        /// Gets URL of the page which handles deletion of documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be deleted or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <returns>URL to which user is redirected</returns>
        public string GetDeleteActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams)
        {
            string returnUrl = additionalParams.DeleteReturnUrl;
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "alllevels", additionalParams.ShowAllLevels.ToString());
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "classid", additionalParams.ClassID.ToString());

            Hashtable parameters = new Hashtable();

            // Process parameters
            if (selectedItems == null)
            {
                if ((additionalParams.Node != null) && !string.IsNullOrEmpty(additionalParams.Node.NodeAliasPath))
                {
                    parameters["parentaliaspath"] = additionalParams.Node.NodeAliasPath;
                }
                if (!string.IsNullOrEmpty(additionalParams.CurrentWhereCondition))
                {
                    parameters["where"] = additionalParams.CurrentWhereCondition;
                }
            }
            else
            {
                if (selectedItems.Any())
                {
                    returnUrl = URLHelper.AddParameterToUrl(returnUrl, "nodeId", Uri.EscapeDataString(string.Join("|", selectedItems)));
                }

                SetRefreshNodeId(parameters, selectedItems, additionalParams);
            }

            // Update the refresh node when the Delete action is performed
            if (additionalParams.RequiresDialog)
            {
                SetRefreshNodeId(parameters, selectedItems, additionalParams);
            }

            return FinishUrl(returnUrl, parameters, additionalParams.Identifier);
        }


        /// <summary>
        /// Gets URL of the page which handles translation of documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be translated or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <returns>URL to which user is redirected</returns>
        public string GetTranslateActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams)
        {
            string returnUrl = additionalParams.TranslateReturnUrl;
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "alllevels", additionalParams.ShowAllLevels.ToString());
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "classid", additionalParams.ClassID.ToString());

            Hashtable parameters = new Hashtable();

            // Process parameters
            if (selectedItems == null)
            {
                if ((additionalParams.Node != null) && !string.IsNullOrEmpty(additionalParams.Node.NodeAliasPath))
                {
                    parameters["parentaliaspath"] = additionalParams.Node.NodeAliasPath;
                }
                if (!string.IsNullOrEmpty(additionalParams.CurrentWhereCondition))
                {
                    parameters["where"] = additionalParams.CurrentWhereCondition;
                }
            }
            else
            {
                if (selectedItems.Any())
                {
                    parameters["nodeids"] = string.Join("|", selectedItems);
                }

                SetRefreshNodeId(parameters, selectedItems, additionalParams);
            }

            return FinishUrl(returnUrl, parameters, additionalParams.Identifier);
        }


        /// <summary>
        /// Gets URL of the page which handles publishing of documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be published or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <returns>URL to which user is redirected</returns>
        public string GetPublishActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams)
        {
            return GetPublishArchiveActionUrl(selectedItems, additionalParams, ArchivePublishMassActionEnum.Publish);
        }


        /// <summary>
        /// Gets URL of the page which handles archiving of documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be archived or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <returns>URL to which user is redirected</returns>
        public string GetArchiveActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams)
        {
            return GetPublishArchiveActionUrl(selectedItems, additionalParams, ArchivePublishMassActionEnum.Archive);
        }


        /// <summary>
        /// Gets URL of the modal dialog which handles copying documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be copied or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <returns>URL of the modal dialog which is opened after user wants to perform mass action</returns>
        public string GetCopyActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams)
        {
            return GetCopyMoveLinkActionUrl(selectedItems, additionalParams, MoveCopyLinkMassActionEnum.Copy);
        }


        /// <summary>
        /// Gets URL of the modal dialog which handles moving documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be moved or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <returns>URL of the modal dialog which is opened after user wants to perform mass action</returns>
        public string GetMoveActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams)
        {
            return GetCopyMoveLinkActionUrl(selectedItems, additionalParams, MoveCopyLinkMassActionEnum.Move);
        }


        /// <summary>
        /// Gets URL of the modal dialog which handles linking documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be linked or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <returns>URL of the modal dialog which is opened after user wants to perform mass action</returns>
        public string GetLinkActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams)
        {
            return GetCopyMoveLinkActionUrl(selectedItems, additionalParams, MoveCopyLinkMassActionEnum.Link);
        }


        #endregion


        #region "Private methods"


        /// <summary>
        /// Gets URL of the modal dialog which handles publishing or archiving documents.
        /// </summary>
        /// <param name="selectedItems">NodeIDs to be archived or null if action is All</param>
        /// <param name="additionalParams">Additional parameters used to construct the Url. Its actual type has to be DocumentListMassActionsParameters</param>
        /// <param name="action">Determines which action should actually be performed (publish/archive)</param>
        /// <returns>URL of the modal dialog which is opened after user wants to perform mass action</returns>
        private string GetPublishArchiveActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams, ArchivePublishMassActionEnum action)
        {
            string returnUrl = action == ArchivePublishMassActionEnum.Archive ? additionalParams.ArchiveReturnUrl : additionalParams.PublishReturnUrl;

            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "action", action.ToString());
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "alllevels", additionalParams.ShowAllLevels.ToString());
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "classid", additionalParams.ClassID.ToString());

            Hashtable parameters = new Hashtable();

            // Process parameters
            if (selectedItems == null)
            {
                if ((additionalParams.Node != null) && !string.IsNullOrEmpty(additionalParams.Node.NodeAliasPath))
                {
                    parameters["parentaliaspath"] = additionalParams.Node.NodeAliasPath;
                }
                if (!string.IsNullOrEmpty(additionalParams.CurrentWhereCondition))
                {
                    parameters["where"] = additionalParams.CurrentWhereCondition;
                }
            }
            else
            {
                if (selectedItems.Any())
                {
                    parameters["nodeids"] = string.Join("|", selectedItems);
                }

                SetRefreshNodeId(parameters, selectedItems, additionalParams);
            }

            return FinishUrl(returnUrl, parameters, additionalParams.Identifier);
        }


        /// <summary>
        /// Gets URL of the modal dialog 
        /// </summary>
        /// <param name="selectedItems"></param>
        /// <param name="additionalParams"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private string GetCopyMoveLinkActionUrl(List<int> selectedItems, DocumentListMassActionsParameters additionalParams, MoveCopyLinkMassActionEnum action)
        {
            string returnUrl = additionalParams.GetCopyMoveLinkBaseActionUrl(action.ToString());

            // Adjust URL to our needs
            returnUrl = URLHelper.RemoveParameterFromUrl(returnUrl, "hash");
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "multiple", "true");

            Hashtable parameters = new Hashtable();

            // Update the refresh node when the Move action is performed
            if ((action == MoveCopyLinkMassActionEnum.Move) && additionalParams.RequiresDialog)
            {
                SetRefreshNodeId(parameters, selectedItems, additionalParams);
            }

            // Process parameters
            if (selectedItems != null)
            {
                returnUrl = URLHelper.AddParameterToUrl(returnUrl, "sourcenodeids", Uri.EscapeDataString(string.Join("|", selectedItems)));
            }
            else if ((additionalParams.Node != null) && !string.IsNullOrEmpty(additionalParams.Node.NodeAliasPath))
            {
                parameters["parentalias"] = additionalParams.Node.NodeAliasPath;
            }

            if (!string.IsNullOrEmpty(additionalParams.CurrentWhereCondition))
            {
                parameters["where"] = SqlHelper.AddWhereCondition(additionalParams.CurrentWhereCondition, "", "OR");
            }

            if (additionalParams.ClassID > 0)
            {
                parameters["classid"] = additionalParams.ClassID;
            }

            if (additionalParams.ShowAllLevels)
            {
                parameters["alllevels"] = additionalParams.ShowAllLevels;
            }

            return FinishUrl(returnUrl, parameters, additionalParams.Identifier);
        }


        /// <summary>
        /// Adds common parameters to the URL and stores parameters in the WindowHelper.
        /// </summary>
        /// <param name="returnUrl">Base URL which will be expanded</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="identifier">Unique identifier under which the data will be stored in the WindowHelper</param>
        /// <returns>Absolute URL with additional parameters (params and hash)</returns>
        private string FinishUrl(string returnUrl, Hashtable parameters, string identifier)
        {
            WindowHelper.Add(identifier, parameters);

            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "params", identifier);
            returnUrl = UrlResolver.ResolveUrl(returnUrl);
            returnUrl = URLHelper.AddParameterToUrl(returnUrl, "hash", QueryHelper.GetHash(URLHelper.GetQuery(returnUrl)));

            return returnUrl;
        }


        /// <summary>
        /// Sets the refresh node id in the given parameters hash table.
        /// </summary>
        /// <param name="parameters">The parameters</param>
        /// <param name="selectedItems">List of selected NodeIDs</param>
        /// <param name="additionalParameters">Additional parameters specifying selection</param>
        private void SetRefreshNodeId(Hashtable parameters, List<int> selectedItems, DocumentListMassActionsParameters additionalParameters)
        {
            // Get the current node
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            TreeNode wOpenerNode = tree.SelectSingleDocument(additionalParameters.WOpenerNodeID);


            int refreshNodeId = 0;
            if (wOpenerNode != null)
            {
                if (selectedItems == null)
                {
                    // Include the parent node in the selected node ids
                    List<int> selectedNodeIds = new List<int>();
                    selectedNodeIds.Add(additionalParameters.Node.NodeID);

                    // Get the refresh node id (checks also its parent nodes)
                    refreshNodeId = GetRefreshNodeID(wOpenerNode, selectedNodeIds);
                }
                // urlParameter - selected node ids (separated by '|')
                else if (selectedItems.Any())
                {
                    refreshNodeId = GetRefreshNodeID(wOpenerNode, selectedItems);
                }

                // Set the refreshNodeId to the current document if the current document is not selected (nor any of its parent nodes is selected) for the move action
                if ((refreshNodeId == 0) && (additionalParameters.Node != null))
                {
                    refreshNodeId = wOpenerNode.NodeID;
                }

                parameters["refreshnodeid"] = refreshNodeId;
            }
        }


        /// <summary>
        /// Gets the refresh node ID.
        /// This method checks all parent nodes and indicates whether any of them is contained in the selected node ids list.
        /// If none of the parent nodes is contained in the list, this method returns 0 (it means that the current wopener node can be used for refreshing the dialog).
        /// If any of the parent nodes is contained in the list, this method returns the parent node id (this node will be used for refreshing the dialog).
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="selectedNodeIds">The selected node ids</param>
        private int GetRefreshNodeID(TreeNode node, List<int> selectedNodeIds)
        {
            if ((node.NodeParentID == 0) || selectedNodeIds.Contains(node.NodeID))
            {
                return node.NodeParentID;
            }

            return GetRefreshNodeID(node.Parent, selectedNodeIds);
        }


        #endregion
    }
}