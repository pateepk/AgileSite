namespace CMS.DataProtection
{
    /// <summary>
    /// Represents result of the data collecting.
    /// </summary>
    /// <seealso cref="IPersonalDataCollector"/>
    public class PersonalDataCollectorResult
    {
        /// <summary>
        /// Returns data collecting result as text.
        /// Is null when no data was found.
        /// </summary>
        public string Text { get; set; }
    }
}
