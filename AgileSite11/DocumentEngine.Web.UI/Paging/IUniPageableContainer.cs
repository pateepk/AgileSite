using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Container referencing the IUniPageable control.
    /// </summary>
    public interface IUniPageableContainer
    {
        /// <summary>
        /// Gets or sets the encapsulated control that implements IUniPageable interface.
        /// </summary>
        IUniPageable PageableControl
        {
            get;
        }
    }
}