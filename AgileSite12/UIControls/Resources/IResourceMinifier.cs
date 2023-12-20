namespace CMS.UIControls
{
    /// <summary>
    /// Defines an interface a compliant minifier must implement to be usable in resource handler.
    /// </summary>
    public interface IResourceMinifier
    {
        /// <summary>
        /// Returns a minified version of a given resource.
        /// </summary>
        /// <param name="resource">Text to be minified</param>
        /// <returns>Minified resource</returns>
        string Minify(string resource);
    }
}