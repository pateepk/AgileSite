using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Basic interface for all attributes.
    /// </summary>
    public interface ICMSAttribute
    {
        /// <summary>
        /// If true, the attribute contains macros
        /// </summary>
        bool ContainsMacro
        {
            get;
            set;
        }
    }
}