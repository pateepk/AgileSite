using System;

namespace CMS.Base.Web.UI
{
    ///<summary>Form mode enumeration.</summary>
    public enum FormModeEnum : int
    {
        ///<summary>
        /// Insert mode.
        ///</summary>
        Insert = 0,

        ///<summary>
        /// Update mode.
        ///</summary>
        Update = 1,

        /// <summary>
        /// Insert new culture version.
        /// </summary>
        InsertNewCultureVersion = 2
    }
}