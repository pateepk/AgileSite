using CMS.DocumentEngine.PageBuilder;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Decorates path with virtual context prefix to include preview context and edit mode query string.
    /// </summary>
    internal sealed class EditModePathDecorator : IPathDecorator
    {
        private readonly string cultureCode;


        /// <summary>
        /// Creates an instance of <see cref="EditModePathDecorator"/> class.
        /// </summary>
        /// <param name="cultureCode">Culture code to be used by the decorator.</param>
        public EditModePathDecorator(string cultureCode = null)
        {
            this.cultureCode = cultureCode;
        }


        /// <summary>
        /// Decorates given path.
        /// </summary>
        /// <param name="path">Path to decorate</param>
        public string Decorate(string path)
        {
            path = new PreviewPathDecorator(false, cultureCode).Decorate(path);
            return PageBuilderHelper.AddEditModeParameter(path);
        }
    }
}
