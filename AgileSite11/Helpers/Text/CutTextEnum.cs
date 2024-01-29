using System;
using System.Linq;
using System.Text;

namespace CMS.Helpers
{
    /// <summary>
    /// Defines the cut location for the text when shortened
    /// </summary>
    public enum CutTextEnum
    {
        /// <summary>
        /// Cut at the end of the text
        /// </summary>
        End = 1,

        /// <summary>
        /// Cut in the middle of the text
        /// </summary>
        Middle = 2,

        /// <summary>
        /// Cut at the start of the text
        /// </summary>
        Start = 3
    }
}
