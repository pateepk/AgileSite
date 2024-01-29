using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Font icon size enumeration.
    /// </summary>
    public enum FontIconSizeEnum
    {
        /// <summary>
        /// Icon size is not defined.
        /// </summary>
        [EnumStringRepresentation(null)]
        NotDefined = 0,

        
        /// <summary>
        /// Status or indicator icon.
        /// </summary>
        [EnumStringRepresentation("cms-icon-30")]
        Status = 30,
            

        /// <summary>
        /// Decorative icon for tree (plus and minus).
        /// </summary>
        [EnumStringRepresentation("cms-icon-50")]
        TreeDecoration = 50,


        /// <summary>
        /// Standard icon size.
        /// </summary>
        [EnumStringRepresentation("cms-icon-80")]
        Standard = 80,


        /// <summary>
        /// Icon in header.
        /// </summary>
        [EnumStringRepresentation("cms-icon-100")]
        Header = 100,

        /// <summary>
        /// Logo icon size.
        /// </summary>
        [EnumStringRepresentation("cms-icon-130")]
        Logo = 130,

        /// <summary>
        /// Notification icon size.
        /// </summary>
        [EnumStringRepresentation("cms-icon-150")]
        Notifications = 150,

        /// <summary>
        /// Dashboard icon size.
        /// </summary>
        [EnumStringRepresentation("cms-icon-200")]
        Dashboard = 200,
    }
}