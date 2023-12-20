using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Uni tree base class.
    /// </summary>
    public class UniTree : CMSUserControl, IObjectTypeDriven
    {
        #region "Variables"

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets custom root node.
        /// </summary>
        public TreeNode CustomRootNode
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets ors sets the value which determines whether the tree will generate postbacks on node click.
        /// </summary>
        public bool UsePostBack
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Gets or sets default image path. (Image path has higher priority that default font icon class.)
        /// </summary>
        public string DefaultImagePath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets default font icon class.
        /// </summary>
        public string DefaultIconClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets expand path.
        /// </summary>
        public string ExpandPath
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// Gets or sets select path.
        /// </summary>
        public string SelectPath
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// Gets or sets TreeProvider object.
        /// </summary>
        public UniTreeProvider ProviderObject
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the selected objects.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return ProviderObject?.ObjectType;
            }
        }


        /// <summary>
        /// Tree view control
        /// </summary>
        protected virtual UITreeView TreeView
        {
            get
            {
                throw new NotImplementedException("Tree view control to be used is not provided.");
            }
        }


        /// <summary>
        /// Gets or sets selected item.
        /// </summary>
        public virtual string SelectedItem
        {
            get;
            set;
        }

        #endregion


        #region "Template properties"

        /// <summary>
        /// Gets or sets node template. You can use following macros:
        /// ##NODEID## (ID of item),##PARENTNODEID## (ID of parent), ##NODEJAVA## (encoded item name for using in javascript), 
        /// ##NODECHILDNODESCOUNT## (count of children of node), ##NODENAME## (name of item), ##ICON## (image), ##OBJECTTYPE## (object type).
        /// </summary>
        public string NodeTemplate
        {
            get;
            set;
        } = "##ICON####NODENAME##";


        /// <summary>
        /// Gets or sets node template. You can use following macros:
        /// ##NODEID## (ID of item), ##PARENTNODEID## (ID of parent), ##NODEJAVA## (encoded item name for using in javascript), 
        /// ##NODECHILDNODESCOUNT## (count of children of node), ##NODENAME## (name of item), ##ICON## (image), ##OBJECTTYPE## (object type). 
        /// </summary>
        public string SelectedNodeTemplate
        {
            get;
            set;
        } = "##ICON####NODENAME##";


        /// <summary>
        /// Gets or sets node template. You can use following macros:
        /// ##NODEJAVA## (encoded item name for using in javascript), 
        /// ##NODENAME## (name of item), ##ICON## (image) 
        /// </summary>
        public string DefaultItemTemplate
        {
            get;
            set;
        } = "##ICON####NODENAME##";


        /// <summary>
        /// Gets or sets node template. You can use following macros:
        /// ##NODEJAVA## (encoded item name for using in javascript), 
        /// ##NODENAME## (name of item), ##ICON## (image) 
        /// </summary>
        public string SelectedDefaultItemTemplate
        {
            get;
            set;
        } = "##ICON####NODENAME##";

        #endregion


        #region "Custom events"

        /// <summary>
        /// Get image event
        /// </summary>
        public UniTreeGetImageHandler OnGetImage = new UniTreeGetImageHandler();

        /// <summary>
        /// On selected item event handler.
        /// </summary>    
        public delegate void ItemSelectedEventHandler(string selectedValue);


        /// <summary>
        /// On selected item event handler.
        /// </summary>
        public event ItemSelectedEventHandler OnItemSelected;


        /// <summary>
        /// Node created delegate.
        /// </summary>
        public delegate TreeNode NodeCreatedEventHandler(DataRow itemData, TreeNode defaultNode);


        /// <summary>
        /// Node created event handler.
        /// </summary>
        public event NodeCreatedEventHandler OnNodeCreated;


        /// <summary>
        /// Raises on selected item event.
        /// </summary>
        /// <param name="selectedValue">Selected value</param>    
        public void RaiseOnItemSelected(string selectedValue)
        {
            SelectedItem = selectedValue;

            OnItemSelected?.Invoke(selectedValue);
        }


        /// <summary>
        /// Creates node.
        /// </summary>
        /// <param name="childNode">Child node</param>
        /// <param name="node">Node which is being created</param>
        public void RaiseOnNodeCreated(UniTreeNode childNode, ref TreeNode node)
        {
            if (OnNodeCreated != null)
            {
                node = OnNodeCreated((DataRow)childNode.ItemData, node);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Reloads data.
        /// </summary>
        public virtual void ReloadData()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}