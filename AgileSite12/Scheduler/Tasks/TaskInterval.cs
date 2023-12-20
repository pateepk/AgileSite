using System;
using System.Collections.Generic;

namespace CMS.Scheduler
{
    /// <summary>
    /// Task interval data container class.
    /// </summary>
    public class TaskInterval
    {
        private List<DayOfWeek> mDays;


        /// <summary>
        /// The task interval Period type.
        /// </summary>
        public string Period
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// The task interval start time.
        /// </summary>
        public DateTime StartTime
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Time part of StartTime should be used.
        /// Used within work-flow only, does not affect algorithm computing next run time
        /// </summary>
        public bool UseSpecificTime
        {
            get;
            set;
        }


        /// <summary>
        /// The task interval Period value.
        /// </summary>
        public int Every
        {
            get;
            set;
        }


        /// <summary>
        /// The task interval run from time.
        /// </summary>
        public DateTime BetweenStart
        {
            get;
            set;
        }


        /// <summary>
        /// The task interval run to time.
        /// </summary>
        public DateTime BetweenEnd
        {
            get;
            set;
        }


        /// <summary>
        /// The task interval Days allowed for repeating.
        /// </summary>
        public List<DayOfWeek> Days
        {
            get
            {
                return mDays ?? (mDays = new List<DayOfWeek>());
            }
            set
            {
                mDays = value;
            }
        }


        /// <summary>
        /// The task interval Day repeating Period.
        /// </summary>
        public string Order
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// The task interval month Day for repeating.
        /// </summary>
        public string Day
        {
            get;
            set;
        } = String.Empty;


        /// <summary>
        /// Transforms Day repeating Period to integer.
        /// </summary>
        public int ToIntOrder()
        {
            int result;
            switch (Order?.ToLowerInvariant())
            {
                case SchedulingHelper.MONTHS_FIRST:
                    result = 1;
                    break;

                case SchedulingHelper.MONTHS_SECOND:
                    result = 2;
                    break;

                case SchedulingHelper.MONTHS_THIRD:
                    result = 3;
                    break;

                case SchedulingHelper.MONTHS_FOURTH:
                    result = 4;
                    break;

                case SchedulingHelper.MONTHS_LAST:
                    result = 5;
                    break;
                default:
                    result = Convert.ToInt32(Order);
                    break;
            }
            return result;
        }
    }
}