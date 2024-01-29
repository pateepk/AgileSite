using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to SharePoint lists.
    /// </summary>
    public interface ISharePointListService : ISharePointService
    {
        /// <summary>
        /// Gets all lists on SharePoint server.
        /// The DataSet has an empty table (with possibly no columns) if no lists are retrieved.
        /// </summary>
        /// <param name="listType">Specifies whether to list only lists of certain type.</param>
        /// <returns>DataSet containing DataTable with all lists.</returns>
        /// <seealso cref="SharePointListType"/>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        DataSet GetLists(int listType = 0);


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
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        /// <remarks>
        /// The value of parameter folderServerRelativeUrl can originate from <em>FileRef</em> column of parent folder items listing. To determine whether an item is a folder,
        /// check the <em>FSObjType</em> column's value equality to <strong>1</strong>.
        /// </remarks>
        DataSet GetListItems(string listTitle, string folderServerRelativeUrl = null, SharePointView view = null, SharePointListItemsSelection listItemsSelection = null);
    }
}
