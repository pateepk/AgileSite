using System.Collections.Generic;
using System.Linq;

namespace CMS.Helpers.UniGraphConfig
{
    /// <summary>
    /// Object holding information about allowed source point counts for node.
    /// </summary>
    public static class NodeSourcePointsLimits
    {
        #region "Variables"

        private static Dictionary<NodeTypeEnum, int> mMin = null;

        private static Dictionary<NodeTypeEnum, int> mMax = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Minimum amount of source points with respect to node type.
        /// </summary>
        public static Dictionary<NodeTypeEnum, int> Min
        {
            get
            {
                if (mMin == null)
                {
                    mMin = GetDefaultMin();
                }
                return mMin;
            }
        }


        /// <summary>
        /// Maximum amount of source points with respect to node type.
        /// </summary>
        public static Dictionary<NodeTypeEnum, int> Max
        {
            get
            {
                if (mMax == null)
                {
                    mMax = GetDefaultMax();
                }
                return mMax;
            }
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Method returning default maximal values in dictionary.
        /// </summary>
        /// <returns>Dictionary</returns>
        private static Dictionary<NodeTypeEnum, int> GetDefaultMax()
        {
            return new Dictionary<NodeTypeEnum, int>
            {
                {NodeTypeEnum.Action, 1},
                {NodeTypeEnum.Condition, 2},
                {NodeTypeEnum.Multichoice, 21},
                {NodeTypeEnum.Standard, 1},
                {NodeTypeEnum.Userchoice, 20}
            };
        }


        /// <summary>
        /// Method returning default minimal values in dictionary.
        /// </summary>
        /// <returns>Dictionary</returns>
        private static Dictionary<NodeTypeEnum, int> GetDefaultMin()
        {
            return new Dictionary<NodeTypeEnum, int>
            {
                {NodeTypeEnum.Action, 1},
                {NodeTypeEnum.Condition, 2},
                {NodeTypeEnum.Multichoice, 2},
                {NodeTypeEnum.Standard, 0},
                {NodeTypeEnum.Userchoice, 2}
            };
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns serializable object containing all data.
        /// </summary>
        public static object GetSerializableObject()
        {
            return new
            {
                Min = Min.ToDictionary(k => ((int)k.Key).ToString(), v => v.Value),
                Max = Max.ToDictionary(k => ((int)k.Key).ToString(), v => v.Value)
            };
        }

        #endregion
    }
}
