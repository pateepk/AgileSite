using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using CMS.Base;
using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class for parsing XML tree with staging objects.
    /// </summary>
    public class ObjectTypeTreeNode
    {
        /// <summary>
        /// Delegate for handling the OnBeforeCreate action of the tree node.
        /// </summary>
        /// <param name="source">Source node</param>
        public delegate bool OnBeforeCreateNodeHandler(ObjectTypeTreeNode source);


        #region "Variables"

        private bool? mActive;

        private readonly List<ObjectTypeTreeNode> mChildNodes = new List<ObjectTypeTreeNode>();
        private readonly StringSafeDictionary<ObjectTypeTreeNode> mChildNodesByKey = new StringSafeDictionary<ObjectTypeTreeNode>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Parent node of actual node.
        /// </summary>
        public ObjectTypeTreeNode ParentNode
        {
            get;
            set;
        }


        /// <summary>
        /// List of child nodes of actual node.
        /// </summary>
        public List<ObjectTypeTreeNode> ChildNodes
        {
            get
            {
                return mChildNodes;
            }
        }


        /// <summary>
        /// Group where belongs actual node.
        /// </summary>
        public string Group
        {
            get;
            set;
        }


        /// <summary>
        /// Object type of actual node.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Module where belongs actual node.
        /// </summary>
        public string Module
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if actual node belongs to site.
        /// </summary>
        public bool Site
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if actual node is expanded.
        /// </summary>
        public bool Expand
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if actual node is main.
        /// </summary>
        public bool Main
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if actual node is Active.
        /// </summary>
        public bool Active
        {
            get
            {
                if (mActive == null)
                {
                    return (ObjectType != null);
                }

                return mActive.Value;
            }
            set
            {
                mActive = value;
            }
        }


        /// <summary>
        /// Node key
        /// </summary>
        protected string Key
        {
            get
            {
                return Group ?? ObjectType;
            }
        }


        /// <summary>
        /// If true, children of this node are sorted
        /// </summary>
        public bool SortChildren
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor which creates group tree node
        /// </summary>
        public ObjectTypeTreeNode(string group, bool parentSiteValue)
        {
            Group = group;
            Site = parentSiteValue;
        }


        /// <summary>
        /// Constructor which creates the tree node from location
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="location">Tree location</param>
        /// <param name="parentSiteValue">Parent site value</param>
        public ObjectTypeTreeNode(string objectType, ObjectTreeLocation location, bool parentSiteValue)
        {
            if (location != null)
            {
                ObjectType = location.ObjectType ?? objectType;
            }
            else
            {
                ObjectType = objectType;
            }

            Site = parentSiteValue;
        }


        /// <summary>
        /// Creates a new empty object type tree. Returns identity
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="setup">Sets up the newly created node</param>
        /// <param name="atStart">If true, the node is added to the start of the list</param>
        public ObjectTypeTreeNode AddObjectType(string objectType, Action<ObjectTypeTreeNode> setup = null, bool atStart = false)
        {
            var newNode = new ObjectTypeTreeNode(objectType, null, Site);

            if (setup != null)
            {
                setup(newNode);
            }

            AddChild(newNode, atStart);

            return this;
        }


        /// <summary>
        /// Creates a new empty object type tree. Returns identity
        /// </summary>
        /// <param name="groupName">Name of the root group</param>
        /// <param name="main">If true, the group is main group</param>
        /// <param name="groupSetup">Sets up the newly created node</param>
        /// <param name="atStart">If true, the node is added to the start of the list</param>
        public ObjectTypeTreeNode AddGroup(string groupName, bool main = false, Action<ObjectTypeTreeNode> groupSetup = null, bool atStart = false)
        {
            var newNode = new ObjectTypeTreeNode(groupName, Site);

            newNode.Main = main;
            newNode.Expand = main;
            newNode.SortChildren = !main;

            if (groupSetup != null)
            {
                groupSetup(newNode);
            }

            AddChild(newNode, atStart);

            return this;
        }


        /// <summary>
        /// Creates a new empty object type tree
        /// </summary>
        /// <param name="rootGroup">Name of the root group</param>
        public static ObjectTypeTreeNode NewObjectTree(string rootGroup)
        {
            var tree = new ObjectTypeTreeNode(rootGroup, false)
                .AddGroup("##SITE##", true, node =>
                    {
                        node.Site = true;
                        node
                            .AddGroup("##CONTENTMANAGEMENT##")
                            .AddGroup("##ONLINEMARKETING##")
                            .AddGroup("##ECOMMERCE##")
                            .AddGroup("##SOCIALANDCOMMUNITY##")
                            .AddGroup("##DEVELOPMENT##")
                            .AddGroup("##CONFIGURATION##");
                    })
                .AddGroup("##GLOBAL##", true, node => node
                            .AddGroup("##CONTENTMANAGEMENT##")
                            .AddGroup("##ONLINEMARKETING##")
                            .AddGroup("##ECOMMERCE##")
                            .AddGroup("##SOCIALANDCOMMUNITY##")
                            .AddGroup("##DEVELOPMENT##")
                            .AddGroup("##CONFIGURATION##")
                    );

            return tree;
        }
        

        /// <summary>
        /// Returns all object types of given node or child tree separated by semicolon.
        /// </summary>
        /// <param name="childNodes">Indicates if child object types should be included</param>
        /// <returns>String of object types separated by semicolon</returns>
        public string GetObjectTypes(bool childNodes)
        {
            // Get its own object type
            string types = "";
            if (!String.IsNullOrEmpty(ObjectType))
            {
                types = ObjectType + ";";
            }

            // Get from the child nodes
            if (childNodes)
            {
                foreach (var child in ChildNodes)
                {
                    types += child.GetObjectTypes(true);
                }
            }

            return types;
        }


        /// <summary>
        /// Find the node with matching object type.
        /// </summary>
        /// <param name="objectType">Object type to find</param>
        /// <param name="siteNode">Site node</param>
        public void RemoveNode(string objectType, bool siteNode)
        {
            ObjectTypeTreeNode parent;
            var node = FindNode(objectType, siteNode, out parent);
            if ((node != null) && (parent != null))
            {
                parent.ChildNodes.Remove(node);
            }
        }


        /// <summary>
        /// Find the node with matching object type.
        /// </summary>
        /// <param name="objectType">Object type to find</param>
        /// <param name="siteNode">Site node</param>
        public ObjectTypeTreeNode FindNode(string objectType, bool siteNode)
        {
            ObjectTypeTreeNode parent;
            return FindNode(objectType, siteNode, out parent);
        }


        /// <summary>
        /// Find the node with matching object type.
        /// </summary>
        /// <param name="objectType">Object type to find</param>
        /// <param name="siteNode">Site node</param>
        /// <param name="parentNode">Parent node</param>
        public ObjectTypeTreeNode FindNode(string objectType, bool siteNode, out ObjectTypeTreeNode parentNode)
        {
            parentNode = null;

            // Find itself
            if (((ObjectType == objectType) || (Group == objectType)) && (Site == siteNode))
            {
                return this;
            }

            // Find in child nodes
            foreach (ObjectTypeTreeNode childNode in ChildNodes)
            {
                var foundNode = childNode.FindNode(objectType, siteNode, out parentNode);
                if (foundNode != null)
                {
                    if (parentNode == null)
                    {
                        parentNode = this;
                    }
                    return foundNode;
                }
            }

            return null;
        }


        /// <summary>
        /// Ensures node with the given relative path
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        public ObjectTypeTreeNode EnsureNode(string relativePath)
        {
            var node = this;

            var path = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (path.Length > 0)
            {
                foreach (var part in path)
                {
                    // Ensure child
                    var child = node.GetChild(part);
                    if (child == null)
                    {
                        child = new ObjectTypeTreeNode(part, node.Site);
                        child.SortChildren = true;

                        node.AddChild(child);
                    }

                    node = child;
                }
            }

            return node;
        }


        /// <summary>
        /// Adds the given child
        /// </summary>
        /// <param name="child">Child to add</param>
        /// <param name="atStart">If true, the node is added to the start of the list</param>
        private void AddChild(ObjectTypeTreeNode child, bool atStart = false)
        {
            if (atStart)
            {
                mChildNodes.Insert(0, child);
            }
            else
            {
                mChildNodes.Add(child);
            }

            mChildNodesByKey[child.Key] = child;
        }


        /// <summary>
        /// Gets a child node by its name
        /// </summary>
        /// <param name="key">Child key</param>
        private ObjectTypeTreeNode GetChild(string key)
        {
            return mChildNodesByKey[key];
        }

        #endregion
    }
}