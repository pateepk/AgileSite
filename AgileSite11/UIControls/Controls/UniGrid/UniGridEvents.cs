using System;
using System.Data;

namespace CMS.UIControls
{
    /// <summary>
    /// Action event handler.
    /// </summary>
    /// <param name="actionName">Action name</param>
    /// <param name="actionArgument">Action parameter</param>
    public delegate void OnActionEventHandler(string actionName, object actionArgument);


    /// <summary>
    /// External data binding event handler.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="sourceName">External source name</param>
    /// <param name="parameter">Source parameter</param>
    public delegate object OnExternalDataBoundEventHandler(object sender, string sourceName, object parameter);


    /// <summary>
    /// Data reloading event handler.
    /// </summary>
    /// <param name="completeWhere">Complete where condition</param>
    /// <param name="currentOrder">Current order by clause</param>
    /// <param name="currentTopN">Current top N value</param>
    /// <param name="columns">Currently selected columns</param>
    /// <param name="currentOffset">Current page offset</param>
    /// <param name="currentPageSize">Current size of page</param>
    /// <param name="totalRecords">Returns number of returned records</param>
    public delegate DataSet OnDataReloadEventHandler(string completeWhere, string currentOrder, int currentTopN, string columns, int currentOffset, int currentPageSize, ref int totalRecords);


    /// <summary>
    /// On before data reloading event handler.
    /// </summary>        
    public delegate void OnBeforeDataReload();


    /// <summary>
    /// On after data reloading event handler.
    /// </summary>        
    public delegate void OnAfterDataReload();


    /// <summary>
    /// On before unigrid sorting event handler.
    /// </summary>        
    public delegate void OnBeforeSorting(object sender, EventArgs e);


    /// <summary>
    /// On before unigrid filtering event handler.
    /// </summary>
    public delegate string OnBeforeFiltering(string whereCondition);


    /// <summary>
    /// On page size change event handler.
    /// </summary>        
    public delegate void OnPageSizeChanged();


    /// <summary>
    /// On columns load event handler.
    /// </summary>
    public delegate void OnLoadColumns();


    /// <summary>
    /// On after retrieve data handler.
    /// </summary>
    /// <param name="ds">Dataset</param>
    public delegate DataSet OnAfterRetrieveData(DataSet ds);


    /// <summary>
    /// On after filter field created handler.
    /// </summary>
    /// <param name="columnName">Filter column name</param>
    /// <param name="filterDefinition">Filter definition</param>
    public delegate void OnFilterFieldCreated(string columnName, UniGridFilterField filterDefinition);
}