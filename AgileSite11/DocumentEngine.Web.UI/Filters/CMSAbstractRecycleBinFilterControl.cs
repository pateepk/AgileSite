
namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Abstract class for recycle bin filter controls.
    /// </summary>
    public class CMSAbstractRecycleBinFilterControl : CMSAbstractBaseFilterControl
    {
        /// <summary>
        /// Indicates if control is used only in one site mode.
        /// </summary>
        public virtual bool IsSingleSite
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if all available users should be displayed.
        /// </summary>
        public virtual bool DisplayUsersFromAllSites
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if date time filter should be displayed.
        /// </summary>
        public virtual bool DisplayDateTimeFilter
        {
            get;
            set;
        }
    }
}
