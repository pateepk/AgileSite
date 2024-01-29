namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Represents a generator of representative sample data suitable for demonstration purposes.
    /// </summary>
    /// <remarks>
    /// For internal use only. No backward compatibility guaranteed.
    /// </remarks>
    public interface ISampleDataGenerator
    {
        /// <summary>
        /// Generates sample data on specified site.
        /// </summary>
        /// <param name="siteID">ID of the site to generate sample data for.</param>
        void Generate(int siteID);
    }
}
