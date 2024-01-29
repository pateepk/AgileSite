using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Internal;

namespace CMS.ContactManagement.Internal
{
    /// <summary>
    /// Represents lightweight version of <see cref="ActivityRecalculationQueueInfo"/>. This class should be used instead of <see cref="ActivityRecalculationQueueInfo"/> in places
    /// when dealing with the regular info object has to big performance impact and features provided by <see cref="BaseInfo"/> are not required.
    /// </summary>
    public class ActivityRecalculationQueueDto : IActivityRecalculationQueueInfo, IDataTransferObject
    {
        /// <summary>
        /// Activity recalculation queue ID.
        /// </summary>
        public int ActivityRecalculationQueueID
        {
            get;
            set;
        }


        /// <summary>
        /// Activity recalculation queue activity ID.
        /// </summary>
        public int ActivityRecalculationQueueActivityID
        {
            get;
            set;
        }


        /// <summary>
        /// Fills given <paramref name="dataContainer"/> with values from current <see cref="ActivityRecalculationQueueDto"/>.
        /// </summary>
        /// <param name="dataContainer">Data container to be filled</param>
        public void FillDataContainer(IDataContainer dataContainer)
        {
            dataContainer["ActivityRecalculationQueueID"] = ActivityRecalculationQueueID;
            dataContainer["ActivityRecalculationQueueActivityID"] = ActivityRecalculationQueueActivityID;
        }
    }
}
