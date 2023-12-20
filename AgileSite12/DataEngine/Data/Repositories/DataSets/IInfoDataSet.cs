using System.Collections;
using System.Data;

namespace CMS.DataEngine
{
    /// <summary>
    /// InfoDataSet interface
    /// </summary>
    public interface IInfoDataSet : IEnumerable
    {
        /// <summary>
        /// Gets new instance of the object hosted in this DataSet
        /// </summary>
        /// <param name="dr">Data row with the source data</param>
        BaseInfo GetNewObject(DataRow dr);
    }
}
