using System;

using CMS.Activities;
using CMS.DataEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Represents implementation of <see cref="IActivityInitializer"/> for custom table form submission activity.
    /// </summary>
    public class CustomTableFormSubmitActivityInitializer : IActivityInitializer
    {
        private readonly DataClassInfo mCustomTable;
        private readonly int mItemId;
        private readonly string mActivityTitle;


        /// <summary>
        /// Instantiate new instance of <see cref="CustomTableFormSubmitActivityInitializer"/>.
        /// </summary>
        /// <param name="customTable">Custom table the record was inserted or updated into</param>
        /// <param name="itemID">ID of the record in custom table</param>
        /// <param name="activityTitle">Title of the activity</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="activityTitle"/> is <c>null</c> or <c>empty</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="itemID"/> is not a positive integer number greater than zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="customTable"/> is <c>null</c>.</exception>
        public CustomTableFormSubmitActivityInitializer(DataClassInfo customTable, int itemID, string activityTitle)
        {
            if (customTable == null)
            {
                throw new ArgumentNullException("customTable");
            }
            if (itemID <= 0)
            {
                throw new ArgumentOutOfRangeException("itemID");
            }
            if (String.IsNullOrEmpty(activityTitle))
            {
                throw new ArgumentException("Argument is null or empty", "activityTitle");
            }

            mCustomTable = customTable;
            mItemId = itemID;
            mActivityTitle = activityTitle;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info to be initialized</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is <c>null</c>.</exception>
        public void Initialize(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            activity.ActivityTitle = String.Format(mActivityTitle, mCustomTable.ClassDisplayName);
            activity.ActivityItemID = mCustomTable.ClassID;
            activity.ActivityItemDetailID = mItemId;
        }


        /// <summary>
        /// Activity type
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.CUSTOM_TABLE_SUBMIT;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMCustomTableForm";
            }
        }
    }
}
