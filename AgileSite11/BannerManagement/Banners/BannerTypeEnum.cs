using CMS.Helpers;

namespace CMS.BannerManagement
{
    /// <summary>
    /// Banner type enumeration
    /// </summary>
    public enum BannerTypeEnum
    {
        /// <summary>
        /// Plain type
        /// </summary>
        [EnumOrder(1)]
        Plain = 0,

        /// <summary>
        /// HTML type
        /// </summary>
        [EnumOrder(2)]
        HTML = 1,
        
        /// <summary>
        /// Image type
        /// </summary>
        [EnumOrder(0)]
        Image = 2
    }
}
