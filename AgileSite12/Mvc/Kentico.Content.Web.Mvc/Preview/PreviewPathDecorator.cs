using CMS.Base;
using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Decorates path with virtual context prefix to include preview context.
    /// </summary>
    internal sealed class PreviewPathDecorator : IPathDecorator
    {
        private readonly bool readonlyMode;
        private readonly string cultureCode;
        private readonly bool embededInAdministration;


        /// <summary>
        /// Rises before the path is decorated.
        /// </summary>
        /// <remarks>
        /// This event can be used to modify the path prior its decoration.
        /// </remarks>
        public static readonly SimpleHandler<PreviewPathDecoratorEventArguments> OnDecorate
            = new SimpleHandler<PreviewPathDecoratorEventArguments>()
            {
                Name = $"{nameof(PreviewPathDecorator)}.{nameof(OnDecorate)}"
            };


        /// <summary>
        /// Creates an instance of <see cref="PreviewPathDecorator"/> class.
        /// </summary>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests.</param>
        /// <param name="cultureCode">Culture code to be used by the decorator.</param>
        public PreviewPathDecorator(bool readonlyMode, string cultureCode = null)
        {
            this.readonlyMode = readonlyMode;
            this.cultureCode = cultureCode;
            embededInAdministration = ValidationHelper.GetBoolean(VirtualContext.GetItem(VirtualContext.PARAM_EMBEDED_IN_ADMINISTRATION), false);
        }


        /// <summary>
        /// Decorates given path.
        /// </summary>
        /// <param name="path">Path to decorate</param>
        public string Decorate(string path)
        {
            var args = new PreviewPathDecoratorEventArguments()
            {
                Path = path
            };

            OnDecorate.StartEvent(args);

            // Replace with the updated path
            path = args.Path;

            return VirtualContext.GetPreviewPathFromVirtualContext(path, readonlyMode, cultureCode, embededInAdministration);
        }
    }
}
