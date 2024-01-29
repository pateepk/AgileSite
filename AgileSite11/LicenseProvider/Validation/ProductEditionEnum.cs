using CMS.Helpers;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// License types. The higher the integer value, the better the license type is.
    /// </summary>
    public enum ProductEditionEnum : int
    {
        /// <summary>
        /// Free.
        /// </summary>
        [EnumStringRepresentation("F")]
        Free = 0,
        
        /// <summary>
        /// Base.
        /// </summary>
        [EnumStringRepresentation("B")]
        Base = 2,

        /// <summary>
        /// Small business license.
        /// </summary>
        [EnumStringRepresentation("N")]
        SmallBusiness = 3,

        /// <summary>
        /// Ultimate version 7 and higher.
        /// </summary>
        [EnumStringRepresentation("V")]
        UltimateV7 = 7,

        /// <summary>
        /// Enterprise marketing solution
        /// </summary>
        [EnumStringRepresentation("X")]
        EnterpriseMarketingSolution = 9
    }
}