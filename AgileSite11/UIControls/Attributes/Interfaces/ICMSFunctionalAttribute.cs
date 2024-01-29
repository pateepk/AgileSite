using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Security attribute interface.
    /// </summary>
    public interface ICMSFunctionalAttribute : ICMSAttribute
    {
        /// <summary>
        /// Applies the attribute data to the page (object).
        /// </summary>
        /// <param name="sender">Sender object</param>
        void Apply(object sender);
    }
}