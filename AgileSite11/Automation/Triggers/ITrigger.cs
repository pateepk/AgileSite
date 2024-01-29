using System.Collections.Generic;

namespace CMS.Automation
{
    /// <summary>
    /// Automation trigger interface.
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Processes the given collection of trigger options.
        /// </summary>
        void Process(IEnumerable<TriggerOptions> options);
    }
}
