using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;

using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to SharePoint lists.
    /// </summary>
    internal class SharePoint2010ListService : SharePointAbstractService, ISharePointListService
    {
        internal const string CONNECTION_NAME_COLUMN_NAME = "#connectionname";
        private const string TITLE_COLUMN_NAME = "Title";

        /// <summary>
        /// Creates a new SharePoint 2010 ISharePointListService 
        /// </summary>
        /// <param name="connectionData"></param>
        public SharePoint2010ListService(SharePointConnectionData connectionData)
            : base(connectionData)
        {

        }


        /// <summary>
        /// Gets all lists on SharePoint server.
        /// The DataSet has an empty table (with no columns) if no lists are retrieved.
        /// Includes only public properties of value type (not Nullable) or string type.
        /// </summary>
        /// <param name="listType">Specifies whether to list only lists of certain type.</param>
        /// <returns>DataSet containing DataTable with all lists.</returns>
        /// <seealso cref="SharePointListType"/>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public DataSet GetLists(int listType = 0)
        {
            var context = CreateClientContext();
            ListCollection lists = context.Web.Lists;

            // Fetch lists from SharePoint server
            context.Load(lists);
            try
            {
                ExecuteQuery(context);
            }
            catch (ServerException ex)
            {
                throw new SharePointServerException(ex.Message, ex);
            }

            // Filter by list type
            List<List> listsAsList = (listType == SharePointListType.ALL) ? lists.ToList() : lists.AsEnumerable().Where(l => l.BaseTemplate == listType).ToList();

            // Convert result to DataSet
            DataSet ds = new DataSet();
            DataTable dt = ListCollectionToDataTable(listsAsList);
            ds.Tables.Add(dt);

            return ds;
        }


        /// <summary>
        /// <para>
        /// Gets a selection of items of a SharePoint list identified by its title.
        /// Keeping <see cref="SharePointListItemsSelection.FieldName"/> or <see cref="SharePointListItemsSelection.FieldType"/> unspecified causes all items (optionally restricted by <see cref="SharePointView.QueryInnerXml"/>) to be selected.
        /// Keeping <see cref="SharePointView.QueryInnerXml"/> unspecified causes all list items to be selected (the <paramref name="listItemsSelection"/> still applies).
        /// </para>
        /// <para>
        /// The <paramref name="listItemsSelection"/> parameter is provided just for comfortability reason and produces a query similar to the following
        /// <pre>
        /// &lt;Query>
        ///     &lt;And>
        ///         <see cref="SharePointView.QueryInnerXml"/> &lt;Eq>&lt;FieldRef Name="<see cref="SharePointListItemsSelection.FieldName"/>" />&lt;Value Type="<see cref="SharePointListItemsSelection.FieldType"/>"><see cref="SharePointListItemsSelection.FieldValue"/>&lt;/Value>&lt;/Eq>
        ///     &lt;/And>
        /// &lt;/Query>
        /// </pre>
        /// </para>
        /// <para>
        /// The DataSet has an empty table (with no columns) if no list items are retrieved.
        /// </para>
        /// </summary>
        /// <param name="listTitle">SharePoint list title</param>
        /// <param name="folderServerRelativeUrl">Relative URL of listed folder (if list supports subfolders). Keep blank for root listing.</param>
        /// <param name="view">Configuration options for list items retrieval.</param>
        /// <param name="listItemsSelection">Constraint for selection of certain items.</param>
        /// <returns>DataSet containing DataTable with list items.</returns>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        /// <remarks>
        /// The value of parameter folderServerRelativeUrl can originate from <em>FileRef</em> column of parent folder items listing. To determine whether an item is a folder,
        /// check the <em>FSObjType</em> column's value equality to <strong>1</strong>.
        /// </remarks>
        public DataSet GetListItems(string listTitle, string folderServerRelativeUrl = null, SharePointView view = null, SharePointListItemsSelection listItemsSelection = null)
        {
            var context = CreateClientContext();
            List list = context.Web.Lists.GetByTitle(listTitle);

            // Prepare CAML query - general
            CamlQuery camlQuery = new CamlQuery();
            CamlQueryBuilder camlQueryBuilder = new CamlQueryBuilder();
            if (view != null)
            {
                camlQueryBuilder.QueryInnerXml = view.QueryInnerXml;
                camlQueryBuilder.ViewFields = view.ViewFields;
                camlQueryBuilder.RowLimit = view.RowLimit;
            }

            // Prepare CAML query - item selection
            if (listItemsSelection != null)
            {
                camlQueryBuilder.ItemSelectionFieldName = listItemsSelection.FieldName;
                camlQueryBuilder.ItemSelectionFieldType = listItemsSelection.FieldType;
                camlQueryBuilder.ItemSelectionFieldValue = listItemsSelection.FieldValue;
            }

            camlQuery.ViewXml = camlQueryBuilder.GetViewXmlAsString(view != null ? view.Attributes : null);
            if (!string.IsNullOrEmpty(folderServerRelativeUrl))
            {
                camlQuery.FolderServerRelativeUrl = folderServerRelativeUrl;
            }

            // Fetch items from SharePoint server
            ListItemCollection items = list.GetItems(camlQuery);
            context.Load(items);
            try
            {
                ExecuteQuery(context);
            }
            catch (ServerException ex)
            {
                throw new SharePointServerException(ex.Message, ex);
            }

            // Convert result to DataSet
            DataSet ds = new DataSet();
            DataTable dt = ListItemCollectionToDataTable(items);
            ds.Tables.Add(dt);

            return ds;
        }


        /// <summary>
        /// Converts ListItemCollection to DataTable.
        /// Returns empty DataTable if ListItemCollection contains no items.
        /// </summary>
        /// <param name="listItemCollection">Items to be converted</param>
        /// <returns>DataTable containing all items, or empty one if zero items provided</returns>
        private DataTable ListItemCollectionToDataTable(ListItemCollection listItemCollection)
        {
            DataTable dt = new DataTable();

            // Return empty DataTable if no items passed
            if ((listItemCollection == null) || (listItemCollection.Count == 0))
            {
                return dt;
            }

            // Prepare the column names in DataTable
            foreach (var pair in listItemCollection[0].FieldValues)
            {
                dt.Columns.Add(pair.Key);
            }

            // Add column containing connection data code name
            dt.Columns.Add(CONNECTION_NAME_COLUMN_NAME);

            bool containsTitleColumn = listItemCollection[0].FieldValues.Any(f => f.Key.Equals(TITLE_COLUMN_NAME, StringComparison.InvariantCulture));

            // Fill the DataTable
            foreach (ListItem listItem in listItemCollection)
            {
                DataRow dr = dt.NewRow();

                foreach (var pair in listItem.FieldValues)
                {
                    if (pair.Value is FieldLookupValue)
                    {
                        FieldLookupValue flv = (FieldLookupValue)pair.Value;
                        dr[pair.Key] = flv.LookupId + ";" + flv.LookupValue;
                    }
                    else if (pair.Value is string[])
                    {
                        dr[pair.Key] = String.Join(Environment.NewLine, (string[])pair.Value);
                    }
                    else if (pair.Value is FieldLookupValue[])
                    {
                        var valueArray = (FieldLookupValue[])pair.Value;
                        dr[pair.Key] = String.Join(Environment.NewLine, valueArray.Select(f => $"{f.LookupId};{f.LookupValue}"));
                    }
                    else if (pair.Value is TaxonomyFieldValue)
                    {
                        var taxonomyValue = (TaxonomyFieldValue)pair.Value;
                        dr[pair.Key] = $"{taxonomyValue.WssId};{taxonomyValue.Label};{taxonomyValue.TermGuid}";
                    }
                    else if (pair.Value is TaxonomyFieldValueCollection)
                    {
                        var taxonomyValueCollection = (TaxonomyFieldValueCollection)pair.Value;
                        dr[pair.Key] = String.Join(Environment.NewLine, taxonomyValueCollection.Select(f => $"{f.WssId};{f.Label};{f.TermGuid}"));
                    }
                    else
                    {
                        dr[pair.Key] = pair.Value;
                    }
                }

                // SharePoint 2016 doesn't provide "Title" is case when the item is a folder. Actually, "Title" is generated so let's fill it with value of "FileLeafRef" property.
                if ((listItem.FileSystemObjectType == FileSystemObjectType.Folder) &&
                    containsTitleColumn && (listItem.FieldValues[TITLE_COLUMN_NAME] == null))
                {
                    dr[TITLE_COLUMN_NAME] = dr["FileLeafRef"]; 
                }

                // Add connection data code name
                dr[CONNECTION_NAME_COLUMN_NAME] = ConnectionData.SharePointConnectionName;
                dt.Rows.Add(dr);
            }

            return dt;
        }


        /// <summary>
        /// Converts ListCollection to DataTable.
        /// Returns empty DataTable if ListCollection contains no items.
        /// Includes only public properties of value type (not Nullable) or string type.
        /// </summary>
        /// <param name="listCollection">Lists to be converted</param>
        /// <returns>DataTable containing all lists, or empty one if zero items provided</returns>
        private DataTable ListCollectionToDataTable(IList<List> listCollection)
        {
            DataTable dt = new DataTable();

            // Return empty DataTable if no items passed
            if (listCollection == null || !listCollection.Any())
            {
                return dt;
            }

            // Prepare the column names in DataTable
            Dictionary<string, PropertyInfo> cachedProperties = new Dictionary<string, PropertyInfo>();
            var firstList = listCollection[0];
            foreach (PropertyInfo propertyInfo in firstList.GetType().GetProperties())
            {
                cachedProperties.Add(propertyInfo.Name, propertyInfo);
                var getMethod = propertyInfo.GetGetMethod();
                if (getMethod.IsPublic // Property getter is public
                    && !(getMethod.ReturnType.IsGenericType && getMethod.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>)) // Type is not Nullable (can not be put into DataTable)
                    && (getMethod.ReturnType.IsValueType || typeof(string).IsAssignableFrom(getMethod.ReturnType))) // Type is either value type or string
                {
                    dt.Columns.Add(propertyInfo.Name, propertyInfo.PropertyType);
                }
            }

            // Fill the DataTable
            HashSet<string> skippedProperties = new HashSet<string>();
            foreach (var list in listCollection)
            {
                DataRow dr = dt.NewRow();

                foreach (DataColumn column in dt.Columns)
                {
                    if (skippedProperties.Contains(column.ColumnName))
                    {
                        continue;
                    }

                    try
                    {
                        dr[column.ColumnName] = cachedProperties[column.ColumnName].GetValue(list, null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException is PropertyOrFieldNotInitializedException)
                        {
                            // Keep uninitialized values blank and prevent throwing exception on the same column name again
                            skippedProperties.Add(column.ColumnName);
                        }
                    }
                }
                dt.Rows.Add(dr);
            }

            // Remove columns which were skipped
            foreach (var skippedProperty in skippedProperties)
            {
                dt.Columns.Remove(skippedProperty);
            }

            return dt;
        }
    }
}
