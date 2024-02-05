using CMS.UIControls.UniGridConfig;

namespace CMS.UIControls
{
    /// <summary>
    /// Specifies contract which can be used to extend UniGrid from the outside. 
    /// </summary>
    /// <remarks>
    /// Contract is minimal and more members can be added if needed.
    /// </remarks>
    public interface IExtensibleUniGrid
    {
        /// <summary>
        /// Raised after UniGrid's data are retrieved.
        /// </summary>
        event OnAfterRetrieveData OnAfterRetrieveData;


        /// <summary>
        /// Adds column to the UniGrid's columns with optional callback which specifies what will be displayed in a column cells. 
        /// </summary>
        /// <remarks>
        /// If <paramref name="externalDataBoundCallback"/> is specified, property ExternalSourceName of the <paramref name="column"/> parameter has to be specified as well.
        /// Callback <paramref name="externalDataBoundCallback"/> will be called only for the cells in the added column.
        /// This method has to be called before the <see cref="OnLoadColumns"/> event is fired, what usually happens on the Page_Load.
        /// </remarks>
        /// <param name="column">Column</param>
        /// <param name="externalDataBoundCallback">Optional external data bound callback. Function which returns content of the column's cells</param>
        void AddAdditionalColumn(Column column, OnExternalDataBoundEventHandler externalDataBoundCallback = null);
    }
}