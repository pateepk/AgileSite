using System;

using CMS.Base;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Simple import handler
    /// </summary>
    public class SimpleImportHandler : SimpleHandler<SimpleImportHandler, ImportEventArgs>
    {
    }


    /// <summary>
    /// Simple import handler with one parameter
    /// </summary>
    public class SimpleImportHandler<ParameterType> : SimpleHandler<SimpleImportHandler<ParameterType>, CMSEventArgs<ParameterType>>
    {
    }
}