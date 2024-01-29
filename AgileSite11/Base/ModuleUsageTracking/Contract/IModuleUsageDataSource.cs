using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Provides statistical information about module.
    /// </summary>
    public interface IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        string Name
        {
            get;
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        IModuleUsageDataCollection GetData();
    }
}
