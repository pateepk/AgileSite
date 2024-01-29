namespace CMS.WebAnalytics
{
    /// <summary>
    /// Represents information about the campaign objective - especially objective's actual and target values.
    /// </summary>
    public sealed class CampaignObjectiveStatistics
    {
        /// <summary>
        /// Target (desired) value of the objective.
        /// </summary>
        public int Target
        {
            get;
            set;
        }


        /// <summary>
        /// Actual value of the objective.
        /// </summary>
        public int Actual
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the objective.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// The result of the objective expressed in percents. 
        /// Value of 100 and above means that the objective was achieved.
        /// </summary>
        /// <returns>The ratio between the <see cref="Actual"/> and the <see cref="Target"/> values expressed in percents. 
        /// Returns <c>100</c> if the <see cref="Target"/> is equal to <c>0</c> (an easy target).</returns>
        public decimal ResultPercent
        {
            get
            {
                if (Target == 0)
                {
                    return 100;
                }

                return ((decimal)Actual / Target) * 100;
            }
        }
    }
}
