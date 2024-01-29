using System;

using CMS.Core;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Registered performance counter
    /// </summary>
    internal class RegisteredCounter
    {
        private readonly Action<Counter> updateMethod;
        private readonly IPerformanceCounter underlyingCounter;


        /// <summary>
        /// Initializes <see cref="RegisteredCounter"/> with given <paramref name="updateMethod"/>.
        /// </summary>
        /// <param name="updateMethod">Method which is called when updating of the counter is requested</param>
        public RegisteredCounter(Action<Counter> updateMethod)
        {
            this.updateMethod = updateMethod;
        }


        /// <summary>
        /// Initializes <see cref="RegisteredCounter"/> with given <paramref name="underlyingCounter"/>.
        /// </summary>
        /// <param name="underlyingCounter">Underlying performance counter</param>
        public RegisteredCounter(IPerformanceCounter underlyingCounter)
        {
            this.underlyingCounter = underlyingCounter;
        }


        /// <summary>
        /// Updates the performance counter.
        /// </summary>
        /// <param name="counter">Counter to update</param>
        public void Update(Counter counter)
        {
            if (updateMethod != null)
            {
                updateMethod(counter);
            }
            else if (underlyingCounter != null)
            {
                counter.PerformanceCounter = underlyingCounter;
            }
        }


        /// <summary>
        /// Clears the underlying counter if available.
        /// </summary>
        public void Clear()
        {
            if (underlyingCounter != null)
            {
                underlyingCounter.Clear();
            }
        }
    }
}
