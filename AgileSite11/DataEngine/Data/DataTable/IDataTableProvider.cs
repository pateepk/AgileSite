using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine.Internal;

[assembly: RegisterImplementation(typeof(IDataTableProvider), typeof(DataTableProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Provides method for creating <see cref="DataTable"/> from given collection of <see cref="IDataTransferObject"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDataTableProvider
    {
        /// <summary>
        /// Converts given <paramref name="objects"/> to <see cref="DataTableContainer"/>.
        /// </summary>
        /// <param name="objects">Collection of objects to be converted</param>
        /// <param name="className">Class name specifying schema of the returned <see cref="DataTableContainer"/></param>
        /// <returns><see cref="DataTableContainer"/> containing values from <paramref name="objects"/></returns>
        DataTableContainer ConvertToDataTable(IEnumerable<IDataTransferObject> objects, string className);
    }
}