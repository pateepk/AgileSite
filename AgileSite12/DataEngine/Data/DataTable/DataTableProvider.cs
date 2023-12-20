using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using CMS.Base;

namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Provides method for creating <see cref="DataTable"/> from given collection of <see cref="IDataTransferObject"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DataTableProvider : IDataTableProvider
    {
        /// <summary>
        /// Converts given <paramref name="objects"/> to <see cref="DataTableContainer"/>.
        /// </summary>
        /// <param name="objects">Collection of objects to be converted</param>
        /// <param name="className">Class name specifying schema of the returned <see cref="DataTableContainer"/></param>
        /// <returns><see cref="DataTableContainer"/> containing values from <paramref name="objects"/></returns>
        public DataTableContainer ConvertToDataTable(IEnumerable<IDataTransferObject> objects, string className)
        {
            var dataSet = ClassStructureInfo.GetClassInfo(className).GetNewDataSet();
            var dataTable = dataSet.Tables[0];

            foreach (var dto in objects)
            {
                var dataRow = new DataRowContainer(dataTable.NewRow());
                dto.FillDataContainer(dataRow);
                dataTable.Rows.Add(dataRow.DataRow);
            }

            return new DataTableContainer(dataTable);
        }
    }
}
