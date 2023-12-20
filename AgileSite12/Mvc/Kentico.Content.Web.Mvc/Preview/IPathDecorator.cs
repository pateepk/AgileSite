namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides interface for decorating path with additional information.
    /// </summary>
    internal interface IPathDecorator
    {
        /// <summary>
        /// Decorates given path.
        /// </summary>
        /// <param name="path">Path to decorate</param>
        string Decorate(string path);
    }
}
