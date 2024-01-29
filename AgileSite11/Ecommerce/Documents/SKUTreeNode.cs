using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Base;
using CMS.Synchronization;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing document connected with product
    /// </summary>
    public class SKUTreeNode : TreeNode
    {
        #region "Variables"

        /// <summary>
        /// List of properties which should be prioritized in the macro controls (Intellisense, MacroTree).
        /// </summary>
        private List<string> mPrioritizedProperties;

        /// <summary>
        /// Inner SKU info
        /// </summary>
        private SKUInfo mSKU;

        /// <summary>
        /// Registered properties
        /// </summary>
        private static RegisteredProperties<TreeNode> mRegisteredProperties;

        #endregion


        #region "Document SKU properties"

        /// <summary>
        /// Document SKU name
        /// </summary>
        public virtual string DocumentSKUName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DocumentSKUName"), "");
            }
            set
            {
                SetValue("DocumentSKUName", value);
            }
        }


        /// <summary>
        /// Document SKU description
        /// </summary>
        public virtual string DocumentSKUDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DocumentSKUDescription"), "");
            }
            set
            {
                SetValue("DocumentSKUDescription", value);
            }
        }


        /// <summary>
        /// Document SKU short description
        /// </summary>
        public virtual string DocumentSKUShortDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DocumentSKUShortDescription"), "");
            }
            set
            {
                SetValue("DocumentSKUShortDescription", value);
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Registered properties
        /// </summary>
        protected override RegisteredProperties<TreeNode> RegisteredProperties
        {
            get
            {
                return mRegisteredProperties ?? (mRegisteredProperties = new RegisteredProperties<TreeNode>(RegisterProperties));
            }
        }


        /// <summary>
        /// List of prioritized properties.
        /// </summary>
        protected override List<string> PrioritizedProperties
        {
            get
            {
                if (mPrioritizedProperties == null)
                {
                    mPrioritizedProperties = new List<string>();
                    mPrioritizedProperties.AddRange(base.PrioritizedProperties);
                    mPrioritizedProperties.AddRange(new SKUInfo().ColumnNames);
                    mPrioritizedProperties.Add("SKU");
                }
                return mPrioritizedProperties;
            }
        }


        /// <summary>
        /// SKU class - Custom document fields
        /// </summary>
        public SKUInfo SKU
        {
            get
            {
                if (mSKU == null)
                {
                    if (NodeSKUID > 0)
                    {
                        // SKU must be cloned because of versioning. 
                        // SKU is changed and saved within document version, SKU table is not updated in this case => we don't want to change sku cached in memory.
                        mSKU = SKUInfoProvider.GetSKUInfo(NodeSKUID)?.Clone();
                    }
                }

                return mSKU;
            }
            set
            {
                mSKU = value;

                // Update NodeSKUID
                base.NodeSKUID = value?.SKUID ?? 0;
            }
        }


        /// <summary>
        /// E-commerce SKU (product) ID.
        /// </summary>
        public override int NodeSKUID
        {
            get
            {
                return base.NodeSKUID;
            }
            set
            {
                if (base.NodeSKUID != value)
                {
                    base.NodeSKUID = value;
                    mSKU = null;
                }
            }
        }


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>   
        public override bool HasChanged
        {
            get
            {
                if (base.HasChanged)
                {
                    return true;
                }

                // SKU DataRow
                return (SKU != null) && SKU.Generalized.HasChanged;
            }
        }


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>       
        public override bool IsComplete
        {
            get
            {
                if (!base.IsComplete)
                {
                    return false;
                }

                // SKU DataRow
                return (SKU == null) || SKU.Generalized.IsComplete;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor, allowed only if Initialize is called immediately after it.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use method TreeNode.New.")]
        public SKUTreeNode()
            : this(null)
        {
        }


        /// <summary>
        /// Base constructor for inherited classes and internal purposes
        /// </summary>
        /// <param name="className">Class name of the document</param>
        protected SKUTreeNode(string className)
            : base(className)
        {
        }

        #endregion


        #region "Overridden TreeNode methods"

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<SKUInfo>("NodeSKU", m => ((SKUTreeNode)m).SKU);
            RegisterProperty<SKUInfo>("SKU", m => ((SKUTreeNode)m).SKU);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            if (!base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure))
            {
                return false;
            }

            return (SKU == null) || SKU.CheckPermissions(permission, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Gets the type of the given property
        /// </summary>
        /// <param name="columnName">Property name</param>
        protected override Type GetPropertyType(string columnName)
        {
            Type result = base.GetPropertyType(columnName);
            if (result != null)
            {
                return result;
            }

            // SKU class
            if (SKU != null)
            {
                result = SKU.Generalized.GetColumnType(columnName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            // Try to get data from page
            var result = base.TryGetValue(columnName, out value);

            if (!result)
            {
                // Try to get data from page by paired column
                var columns = GetPairedPageColumns(columnName);
                if (columns.Length > 0)
                {
                    result = base.TryGetValue(columns[0], out value);
                }

                // Try to get data from SKU in case we have not found data OR found data is null
                if ((SKU != null) && (!result || (value == null)))
                {
                    result = SKU.TryGetValue(columnName, out value);
                }
            }

            // Ensure value of DocumentSKUDescription and DocumentSKUShortDescription if value in original document is null
            if ((value == null) && columnName.StartsWith("DocumentSKU", StringComparison.OrdinalIgnoreCase) && (SKU != null))
            {
                var column = GetPairedSKUColumn(columnName);
                result = SKU.TryGetValue(column, out value);
            }

            // Ensure the null value
            value = DataHelper.GetNull(value);

            return result;
        }


        /// <summary>
        /// Sets value of the specified node column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (IsColumnFromPage(columnName))
            {
                base.SetValue(columnName, value);
                if (DocumentActionContext.CurrentSynchronizeFieldValues)
                {
                    var skuColumn = GetPairedSKUColumn(columnName);
                    SetSKUColumn(skuColumn, value);
                }

                return true;
            }

            if (IsColumnFromSKU(columnName))
            {
                SetSKUColumn(columnName, value);
                if (DocumentActionContext.CurrentSynchronizeFieldValues)
                {
                    var pageColumns = GetPairedPageColumns(columnName);
                    Array.ForEach(pageColumns, column => base.SetValue(column, value));
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the object contains specific column
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ContainsColumn(string columnName)
        {
            if (base.ContainsColumn(columnName))
            {
                return true;
            }

            // Check SKU class
            return (SKU != null) && SKU.ContainsColumn(columnName);
        }


        /// <summary>
        /// Returns true if the item on specified column name changed.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ItemChanged(string columnName)
        {
            if (base.ItemChanged(columnName))
            {
                return true;
            }

            // Try SKU
            if ((SKU != null) && SKU.ContainsColumn(columnName))
            {
                return SKU.ItemChanged(columnName);
            }

            return false;
        }


        /// <summary>
        /// Gets the original value of the given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetOriginalValue(string columnName)
        {
            object result = base.GetOriginalValue(columnName);
            if (result != null)
            {
                return result;
            }

            // Try SKU
            if ((SKU != null) && SKU.ContainsColumn(columnName))
            {
                return SKU.Generalized.GetOriginalValue(columnName);
            }

            return null;
        }


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged.
        /// </summary>
        public override void ResetChanges()
        {
            base.ResetChanges();

            SKU?.Generalized.ResetChanges();
        }


        /// <summary>
        /// Reverts the object changes and keeps the new values as unchanged.
        /// </summary>
        public override void RevertChanges()
        {
            base.RevertChanges();

            // SKU
            SKU?.Generalized.RevertChanges();
        }


        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
        public override void MakeComplete(bool loadFromDb)
        {
            base.MakeComplete(loadFromDb);

            // SKU
            SKU?.MakeComplete(loadFromDb);
        }


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        /// <param name="excludedColumns">List of columns excluded from change (separated by ';')</param>
        public override bool DataChanged(string excludedColumns)
        {
            bool changed = base.DataChanged(excludedColumns);

            // SKU DataRow
            if (SKU != null)
            {
                changed |= SKU.DataChanged(excludedColumns);
            }

            return changed;
        }


        /// <summary>
        /// Gets the column names that are specific to the type
        /// </summary>
        protected override IEnumerable<string> GetTypeSpecificColumnNames()
        {
            var names = base.GetTypeSpecificColumnNames().ToList();

            // Add SKU columns
            var sku = SKU ?? ModuleManager.GetReadOnlyObject(SKUInfo.OBJECT_TYPE_SKU);
            names.AddRange(sku.ColumnNames);

            return names;
        }


        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        public override TreeNode Clone(bool clear)
        {
            var result = base.Clone(clear);

            if (clear && (result is SKUTreeNode))
            {
                // SKU
                var sku = ((SKUTreeNode)result).SKU;
                sku?.SetValue("SKUID", null);

                result.SetValue("NodeSKUID", null);
            }

            return result;
        }


        /// <summary>
        /// Copies the node data to the destination node according to the settings. 
        /// </summary>
        /// <param name="destNode">Destination node</param>
        /// <param name="settings">Copy settings</param>
        public override void CopyDataTo(TreeNode destNode, CopyNodeDataSettings settings)
        {
            // Copy SKU data
            if (settings.CopySKUData)
            {
                settings.ExcludeColumns.Add("NodeSKUID");

                SKUTreeNode destSkuNode = destNode as SKUTreeNode;
                if (destSkuNode != null)
                {
                    destSkuNode.SKU = SKU?.Clone();

                    // Update original values and reset changes, treat copied data as original data
                    if (settings.ResetChanges)
                    {
                        destSkuNode.SKU?.ResetChanges();
                    }
                }
            }

            base.CopyDataTo(destNode, settings);
        }


        /// <summary>
        /// Loads the object data from given data container.
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override void LoadData(LoadDataSettings settings)
        {
            base.LoadData(settings);

            // Create the SKU data
            var data = settings.Data;
            if (data != null)
            {
                int skuId = ValidationHelper.GetInteger(data.GetValue("NodeSKUID"), 0);
                if (skuId > 0)
                {
                    mSKU = new SKUInfo(data);
                }
            }
        }


        /// <summary>
        /// Inserts the CMS_Tree part of the document node
        /// </summary>
        /// <param name="parent">Parent node</param>
        protected override void InsertTreeNodeData(BaseInfo parent)
        {
            SetSKU();

            // Insert the Tree record
            base.InsertTreeNodeData(parent);
        }


        /// <summary>
        /// Updates the CMS_Tree part of the document node
        /// </summary>
        protected override void UpdateTreeNodeData()
        {
            SetSKU();

            // Update the Tree record
            base.UpdateTreeNodeData();
        }


        /// <summary>
        /// Maps the document name based on the document type settings
        /// </summary>
        public override string MapDocumentName()
        {
            string name = base.MapDocumentName();

            // Set DocumentName from DocumentSKUName if Document name source field is not set
            if (String.IsNullOrEmpty(name) && (!string.IsNullOrEmpty(DocumentSKUName)))
            {
                name = DocumentSKUName;
                DocumentName = name;
            }

            return name;
        }


        /// <summary>
        /// Returns true when node is bound to the same SKU as some other non-link node does.
        /// </summary>
        /// <param name="tree">Tree provider to use</param>
        public bool NodeSharesSKUWithOtherNode(TreeProvider tree)
        {
            string where = "NodeLinkedNodeID IS NULL AND NodeSKUID = " + NodeSKUID + " AND NodeID <> " + NodeID;
            return 0 < tree.SelectNodesCount(TreeProvider.ALL_SITES, "/%", TreeProvider.ALL_CULTURES, true, TreeProvider.ALL_CLASSNAMES, where, null, TreeProvider.ALL_LEVELS, false);
        }


        /// <summary>
        /// Handles removing of dependencies for product document.
        /// </summary>
        /// <param name="settings">Delete document settings</param>
        protected override void RemoveDocumentDependencies(DeleteDocumentSettings settings)
        {
            base.RemoveDocumentDependencies(settings);

            int skuId = NodeSKUID;
            if (skuId <= 0)
            {
                return;
            }

            // Get product
            SKUInfo product = SKUInfoProvider.GetSKUInfo(skuId);
            UserInfo user = TreeProvider.UserInfo;

            if (product == null)
            {
                return;
            }

            // Cancel delete/disable action when SKU is assigned to any other node
            if (NodeSharesSKUWithOtherNode(TreeProvider))
            {
                return;
            }

            // Check product dependencies
            if (SKUInfoProvider.CheckDependencies(skuId))
            {
                // Disable product if user is authorized to modify it
                if (product.CheckPermissions(PermissionsEnum.Modify, NodeSiteName, user))
                {
                    // Disable product
                    product.SKUEnabled = false;
                    SKUInfoProvider.SetSKUInfo(product);
                }
            }
            else
            {
                // Delete product if user is authorized to delete it
                // and if deleting last culture version of the page or deleting all cultures
                if (product.CheckPermissions(PermissionsEnum.Delete, NodeSiteName, user) && ((CultureVersions.Count == 1) || settings.DeleteAllCultures))
                {
                    // Figure out if object history is to be destroyed
                    bool destroySKU = settings.DestroyHistory && product.CheckPermissions(PermissionsEnum.Destroy, NodeSiteName, user);

                    // Do not create recycle bin version - data is part of the document data
                    using (new CMSActionContext { CreateVersion = false, User = user })
                    {
                        SKUInfoProvider.DeleteSKUInfo(product);
                    }

                    if (destroySKU)
                    {
                        // Destroy history
                        ObjectVersionManager.DestroyObjectHistory(SKUInfo.OBJECT_TYPE_SKU, skuId);
                    }
                }
            }
        }


        /// <summary>
        /// For the link node, the DataSet of original data is returned, otherwise returns DataSet of node.
        /// </summary>
        protected override DataSet GetOriginalDataSet()
        {
            var dataSet = base.GetOriginalDataSet();
            AddSKUData(dataSet);

            return dataSet;
        }


        /// <summary>
        /// Returns the DataSet of the node data.
        /// </summary>
        public override DataSet GetDataSet()
        {
            var dataSet = base.GetDataSet();
            AddSKUData(dataSet);

            return dataSet;
        }

        #endregion


        #region "Private SKU methods"

        private void AddSKUData(DataSet dataSet)
        {
            if (SKU == null)
            {
                return;
            }

            var row = dataSet.Tables[0].Rows[0];
            foreach (var columnName in SKU.ColumnNames)
            {
                SetDataRowColumn(row, columnName, GetValue(columnName));
            }
        }


        /// <summary>
        /// Inserts or updates the internal SKU object
        /// </summary>
        private void SetSKU()
        {
            // Insert the SKU if set
            if (SKU == null)
            {
                return;
            }

            // Synchronize SKUName with document sku name representation in default culture
            SetSKUColumn("SKUName", DocumentSKUName);

            if (SKU.Generalized.ObjectID > 0)
            {
                // Update the existing SKU, if it changed, and if the document is the published version when under the workflow (not last version)
                var workflow = GetWorkflow();
                if (SKU.Generalized.DataChanged() && ((workflow == null) || !IsLastVersion))
                {
                    SKU.Update();
                }
            }
            else
            {
                // Insert a new SKU, if all the required information are specified
                if (SKU.CheckRequiredColumns("SKUName", "SKUPrice", "SKUEnabled"))
                {
                    SKU.Insert();
                    SetValue("NodeSKUID", SKU.Generalized.ObjectID);
                }
                else
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "Create product", "CreateSKUWithMapping",
                        "SKU can not be saved. Some of these fields: SKUName, SKUPrice, SKUEnabled were not set due to product fields mapping configuration.",
                        documentName: DocumentName, siteId: NodeSiteID
                    );
                }
            }
        }


        /// <summary>
        /// Sets value of the specified SKU column
        /// </summary>
        /// <param name="column">SKU column to be set</param>
        /// <param name="value">Value to be used</param>
        private void SetSKUColumn(string column, object value)
        {
            if (string.IsNullOrEmpty(column))
            {
                return;
            }

            if (TreeProvider.UpdateSKUColumns && (SKU != null) && SKU.ContainsColumn(column))
            {
                // Fields that are paired with document fields can be updated only with default culture values (SKU object itself cannot be localized)
                if ((GetPairedPageColumns(column).Length < 1) || IsDefaultCulture || String.IsNullOrEmpty(SKU.GetStringValue(column, "")))
                {
                    SKU.SetValue(column, value);
                }
            }
        }


        private bool IsColumnFromSKU(string columnName)
        {
            return (SKU != null) && SKU.ContainsColumn(columnName);
        }


        private bool IsColumnFromPage(string columnName)
        {
            return base.ContainsColumn(columnName);
        }


        private string GetPairedSKUColumn(string documentColumn)
        {
            switch (documentColumn.ToLowerInvariant())
            {
                // Set SKU name and its paired document columns
                case "documentskuname":
                case "documentname":
                    return "SKUName";

                // Set SKUDescription
                case "documentskudescription":
                    return "SKUDescription";

                // Set SKUShortDescription
                case "documentskushortdescription":
                    return "SKUShortDescription";
            }

            return null;
        }


        private string[] GetPairedPageColumns(string skuColumn)
        {
            switch (skuColumn.ToLowerInvariant())
            {
                // Set DocumentName and DocumentSKUName
                case "skuname":
                    return new[]
                    {
                        "DocumentSKUName",
                        "DocumentName"
                    };

                // Set DocumentSKUDescription
                case "skudescription":
                    return new[]
                    {
                        "DocumentSKUDescription"
                    };

                // Set DocumentSKUShortDescription
                case "skushortdescription":
                    return new[]
                    {
                        "DocumentSKUShortDescription"
                    };
            }

            return new string[] { };
        }

        #endregion


        #region "Versioning methods"

        /// <summary>
        /// Ensures data consistency.
        /// </summary>
        public override void EnsureConsistency()
        {
            base.EnsureConsistency();

            // Check product
            int skuID = ValidationHelper.GetInteger(GetValue("NodeSKUID"), 0);
            if (skuID > 0)
            {
                SKUInfo sku = SKUInfoProvider.GetSKUInfo(skuID);
                if (sku == null)
                {
                    SetValue("NodeSKUID", null);
                    SetValue("SKUID", null);
                }
            }
            else
            {
                SetValue("NodeSKUID", null);
                SetValue("SKUID", null);
            }

            // Check other FKs
            CheckSKUReferencedObject("SKUSiteID", SiteInfoProvider.GetSiteInfo, NodeSiteID);
            CheckSKUReferencedObject("SKUDepartmentID", DepartmentInfoProvider.GetDepartmentInfo);
            CheckSKUReferencedObject("SKUTaxClassID", TaxClassInfoProvider.GetTaxClassInfo);
            CheckSKUReferencedObject("SKUInternalStatusID", InternalStatusInfoProvider.GetInternalStatusInfo);
            CheckSKUReferencedObject("SKUPublicStatusID", PublicStatusInfoProvider.GetPublicStatusInfo);
            CheckSKUReferencedObject("SKUManufacturerID", ManufacturerInfoProvider.GetManufacturerInfo);
            CheckSKUReferencedObject("SKUBrandID", BrandInfoProvider.GetBrandInfo);
            CheckSKUReferencedObject("SKUCollectionID", CollectionInfoProvider.GetCollectionInfo);
            CheckSKUReferencedObject("SKUSupplierID", SupplierInfoProvider.GetSupplierInfo);
            CheckSKUReferencedObject("SKUParentSKUID", SKUInfoProvider.GetSKUInfo);
            CheckSKUReferencedObject("SKUOptionCategoryID", OptionCategoryInfoProvider.GetOptionCategoryInfo);
        }


        /// <summary>
        /// Checks the presence of referenced object. 
        /// When the referenced object does not exist, <c>null</c> or <paramref name="replacement"/> is stored to the FK column. 
        /// </summary>
        private void CheckSKUReferencedObject(string columnName, Func<int, BaseInfo> getById, object replacement = null)
        {
            var infoID = GetValue(columnName, 0);
            if (infoID > 0)
            {
                var infoObj = getById(infoID);
                if (infoObj == null)
                {
                    SetValue(columnName, replacement);
                }
            }
        }

        #endregion
    }
}
