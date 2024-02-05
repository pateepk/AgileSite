using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Load default selection handler
    /// </summary>
    public class LoadSelectionHandler<EventArgsType> : SimpleHandler<LoadSelectionHandler<EventArgsType>, EventArgsType>
        where EventArgsType : EventArgs, new()
    {
    }


    /// <summary>
    /// Load import default selection handler
    /// </summary>
    public class ImportLoadSelectionHandler : LoadSelectionHandler<ImportLoadSelectionArgs>
    {
    }


    /// <summary>
    /// Load export default selection handler
    /// </summary>
    public class ExportLoadSelectionHandler : LoadSelectionHandler<ExportLoadSelectionArgs>
    {
    }
}