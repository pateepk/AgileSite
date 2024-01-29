using System;
using System.Collections;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.Scheduler
{
    /// <summary>
    /// Task interval data container class.
    /// </summary>
    public class TaskInterval
    {
        #region "Variables"

        private string mPeriod = String.Empty;
        private List<DayOfWeek> mDays;
        private string mOrder = String.Empty;
        private string mDay = String.Empty;

        #endregion

        /// <summary>
        /// The task interval Period type.
        /// </summary>
        public string Period
        {
            get
            {
                return mPeriod;
            }
            set
            {
                mPeriod = value;
            }
        }


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
            get
            {
                return mOrder;
            }
            set
            {
                mOrder = value;
            }
        }


        /// <summary>
        /// The task interval month Day for repeating.
        /// </summary>
        public string Day
        {
            get
            {
                return mDay;
            }
            set
            {
                mDay = value;
            }
        }


        /// <summary>
        /// Transforms Day repeating Period to integer.
        /// </summary>
        public int ToIntOrder()
        {
            int result;
            switch (Order.ToLowerCSafe())
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