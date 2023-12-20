using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Control that supports short IDs.
    /// </summary>
    public interface IShortID
    {
        /// <summary>
        /// Standard ID.
        /// </summary>
        string ID
        {
            get;
            set;
        }


        /// <summary>
        /// Short ID.
        /// </summary>
        string ShortID
        {
            get;
            set;
        }
    }
}