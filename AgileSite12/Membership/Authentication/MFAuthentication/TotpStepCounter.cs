using System;

namespace CMS.Membership
{
    /// <summary>
    /// Computes step numbers in time based on epoch and step length.
    /// </summary>
    internal class TotpStepCounter
    {
        private static readonly DateTime DEFAULT_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        /// <summary>
        /// Gets time in which a step number calculation begins.
        /// </summary>
        public DateTime Epoch
        {
            get;
        }


        /// <summary>
        /// Gets step length in seconds.
        /// </summary>
        public int StepLengthSeconds
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the TOTP step counter class with epoch set to UNIX epoch and time step of length of 30 seconds.
        /// </summary>
        public TotpStepCounter()
            : this(DEFAULT_EPOCH, 30)
        {
        }


        /// <summary>
        /// Initializes a new instance of the TOTP step counter class with specified epoch and step length.
        /// </summary>
        /// <param name="epoch">Time in which a step number calculation begins.</param>
        /// <param name="stepLengthSeconds">Step length in seconds.</param>
        public TotpStepCounter(DateTime epoch, int stepLengthSeconds)
        {
            if (stepLengthSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stepLengthSeconds), "Length of step must be greater than 0.");
            }

            Epoch = epoch;
            StepLengthSeconds = stepLengthSeconds;
        }


        /// <summary>
        /// Gets step number for current UTC time (<see cref="DateTime.UtcNow"/>).
        /// </summary>
        /// <returns>Current step number.</returns>
        public virtual long GetUtcNowStepNumber()
        {
            return GetStepNumber(DateTime.UtcNow);
        }


        /// <summary>
        /// Gets step number for given date and time.
        /// </summary>
        /// <returns>Step number for date and time specified by <paramref name="when"/>.</returns>
        public long GetStepNumber(DateTime when)
        {
            return (long)(when - Epoch).TotalSeconds / StepLengthSeconds;
        }
    }
}
