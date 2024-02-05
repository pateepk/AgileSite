using CMS.Base;

namespace CMS.Forums
{
    /// <summary>
    /// Summary description for ForumModes.
    /// </summary>
    public static class ForumModes
    {
        /// <summary>
        /// Returns the layout mode based on the given string.
        /// </summary>
        public static ShowModeEnum GetShowMode(string showMode)
        {
            if (showMode == null)
            {
                return ShowModeEnum.TreeMode;
            }
            else
            {
                switch (showMode.ToLowerCSafe())
                {
                    case "detailmode":
                        return ShowModeEnum.DetailMode;
                    case "dynamicdetailmode":
                        return ShowModeEnum.DynamicDetailMode;
                    default:
                        return ShowModeEnum.TreeMode;
                }
            }
        }


        /// <summary>
        /// Returns the layout mode based on the given string.
        /// </summary>
        public static FlatModeEnum GetFlatMode(string flatMode)
        {
            if (flatMode == null)
            {
                return FlatModeEnum.Threaded;
            }
            else
            {
                switch (flatMode.ToLowerCSafe())
                {
                    case "newesttooldest":
                        return FlatModeEnum.NewestToOldest;
                    case "oldesttonewest":
                        return FlatModeEnum.OldestToNewest;
                    default:
                        return FlatModeEnum.Threaded;
                }
            }
        }
    }
}