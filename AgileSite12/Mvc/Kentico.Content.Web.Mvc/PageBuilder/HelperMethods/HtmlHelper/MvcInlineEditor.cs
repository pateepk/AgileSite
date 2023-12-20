using System;
using System.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents inline editor element in page builder.
    /// </summary>
    public sealed class MvcInlineEditor : IDisposable
    {
        private readonly ViewContext viewContext;
        private readonly TagBuilder editorElement;


        /// <summary>
        /// Creates an instance of <see cref="MvcInlineEditor"/>.
        /// </summary>
        /// <param name="viewContext">View context.</param>
        /// <param name="editorElement">Editor element to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewContext"/> or <paramref name="editorElement"/> is null.</exception>
        public MvcInlineEditor(ViewContext viewContext, TagBuilder editorElement)
        {
            this.viewContext = viewContext ?? throw new ArgumentNullException(nameof(viewContext));
            this.editorElement = editorElement ?? throw new ArgumentNullException(nameof(editorElement));
        }


        /// <summary>
        /// Releases all resources that are used by the current instance of the <see cref="MvcInlineEditor"/> class.
        /// </summary>
        public void Dispose()
        {
            viewContext.Writer.Write(editorElement.ToString(TagRenderMode.EndTag));
        }
    }
}
