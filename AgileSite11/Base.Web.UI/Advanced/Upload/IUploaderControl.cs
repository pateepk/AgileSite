using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Data control interface.
    /// </summary>
    public interface IUploaderControl : IInputControl
    {
        /// <summary>
        /// ID of the control.
        /// </summary>
        string ID
        {
            get;
            set;
        }


        /// <summary>
        /// Clears the data.
        /// </summary>
        void Clear();
    }
}