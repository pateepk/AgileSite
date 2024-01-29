using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Event arguments for events where only information about score is needed.
    /// </summary>
    public class ScoreEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Score (its meaning depends on which Handler fired the event with this event argument).
        /// </summary>
        public ScoreInfo Score
        {
            get;
            set;
        }
    }
}
