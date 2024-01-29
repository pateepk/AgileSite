namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for retrieving sender email and name.
    /// </summary>
    public interface ISenderRetriever
    {
        /// <summary>
        /// Gets sender for message From value.
        /// </summary>
        string GetFrom();


        /// <summary>
        /// Gets sender for message Reply-To value.
        /// </summary>
        string GetReplyTo();
    }
}