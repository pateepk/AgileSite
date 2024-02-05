using System;
using System.Diagnostics;
using System.Xml;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// The class that represents counter definition.
    /// </summary>
    public class Counter
    {
        #region "Variables"

        private readonly Lazy<IPerformanceCounter> mOriginalCounter = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private readonly CMSLazy<IPerformanceCounter> mPerformanceCounter = new CMSLazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the counter key.
        /// </summary>
        public string Key
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the counter name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets counter description.
        /// </summary>
        public string Description
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the counter type.
        /// </summary>
        public PerformanceCounterType Type
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the counter is enabled. Default value is True.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the counter is logged per instance or per sites (True is per instance). Default value is True.
        /// </summary>
        public bool OnlyGlobal
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if occurred error during logging to the counter. 
        /// </summary>
        public bool Error
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if counter is logged per second.
        /// </summary>
        public bool PerSecond
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if performance counter is cleared after logging values.
        /// </summary>
        public bool Interval
        {
            get;
            set;
        }


        /// <summary>
        /// Last error message.
        /// </summary>
        public string LastErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Stores original values of CMS performance counter.
        /// </summary>
        public IPerformanceCounter OriginalCounter
        {
            get
            {
                return mOriginalCounter.Value;
            }
        }


        /// <summary>
        /// Gets or sets performance counter.
        /// </summary>
        public IPerformanceCounter PerformanceCounter
        {
            get
            {
                return mPerformanceCounter.Value;
            }
            internal set
            {
                mPerformanceCounter.Value = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">XML node</param>
        public Counter(XmlNode node)
        {
            Key = XmlHelper.GetAttributeValue(node, "Key", null);
            Name = XmlHelper.GetAttributeValue(node, "Name", null);
            Description = XmlHelper.GetAttributeValue(node, "Description", string.Empty);
            string typeValue = XmlHelper.GetAttributeValue(node, "Type", null);

            // Check data integrity
            if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Name) || !Enum.IsDefined(typeof(PerformanceCounterType), typeValue))
            {
                throw new Exception("Wrong counter configuration. Missing either Key or Name attribute or Type attribute is in wrong format.");
            }

            Type = (PerformanceCounterType)Enum.Parse(typeof(PerformanceCounterType), typeValue);
            Enabled = ValidationHelper.GetBoolean(XmlHelper.GetAttributeValue(node, "Enabled", null), true);
            OnlyGlobal = ValidationHelper.GetBoolean(XmlHelper.GetAttributeValue(node, "OnlyGlobal", null), true);
            PerSecond = ValidationHelper.GetBoolean(XmlHelper.GetAttributeValue(node, "PerSecond", null), false);
            Interval = ValidationHelper.GetBoolean(XmlHelper.GetAttributeValue(node, "Interval", null), false);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Logs global and sites values.
        /// </summary>
        public void Log()
        {
            if (!Error)
            {
                // Log global value
                LogToCounter(null);

                if (!Error && !OnlyGlobal && HealthMonitoringHelper.SiteCountersEnabled && (HealthMonitoringManager.Sites != null))
                {
                    foreach (string siteName in HealthMonitoringManager.Sites)
                    {
                        // Log site values
                        LogToCounter(siteName);
                    }
                }

                if (Interval)
                {
                    // Clear performance counter
                    PerformanceCounter.Clear();
                }
            }
        }


        /// <summary>
        /// Logs value to the performance counter.
        /// </summary>
        /// <param name="siteName">Site name</param>
        private void LogToCounter(string siteName)
        {
            // Last read time
            DateTime lastTime = PerformanceCounter.GetLastLog(siteName);

            // Get current value
            long currentValue = PerformanceCounter.GetValue(siteName, true);

            // Set category name - general or sites
            string categoryName = string.IsNullOrEmpty(siteName) ? HealthMonitoringManager.GeneralCategoryName : HealthMonitoringManager.SitesCategoryName;

            // Get value per second
            if (PerSecond)
            {
                // Last value
                long lastValue = OriginalCounter.GetValue(siteName);
                // Set actual value to original counter
                OriginalCounter.SetValue(currentValue, siteName);

                if (lastTime != DateTime.MinValue)
                {
                    // Get interval
                    double interval = DateTime.Now.Subtract(lastTime).TotalSeconds;

                    // Minimum interval
                    int minInterval = HealthMonitoringHelper.UseExternalService ? HealthMonitoringHelper.ServiceMonitoringInterval : HealthMonitoringHelper.ApplicationMonitoringInterval;
                    if ((interval == 0) || (interval < minInterval))
                    {
                        interval = minInterval;
                    }

                    // Calculate value per second
                    currentValue = Convert.ToInt64(Math.Abs(currentValue - lastValue) / interval);
                }
            }

            if (lastTime == DateTime.MinValue)
            {
                currentValue = 0;
            }

            // Log value
            HealthMonitoringManager.SetCounterValue(categoryName, this, siteName, currentValue);
        }

        #endregion
    }
}