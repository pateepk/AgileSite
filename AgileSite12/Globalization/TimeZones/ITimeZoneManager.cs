namespace CMS.Globalization
{
    /// <summary>
    /// Time zone manager.
    /// </summary>
    public interface ITimeZoneManager
    {
        /// <summary>
        /// Time zone type.
        /// </summary>
        TimeZoneTypeEnum TimeZoneType
        {
            get;
        }


        /// <summary>
        /// Custom time zone.
        /// </summary>
        TimeZoneInfo CustomTimeZone
        {
            get;
        }
    }
}