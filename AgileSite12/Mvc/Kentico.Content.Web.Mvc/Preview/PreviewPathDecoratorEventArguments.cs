using System;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Preview path decorator event data.
    /// </summary>
    internal sealed class PreviewPathDecoratorEventArguments : EventArgs
    {
        /// <summary>
        /// Path to be modified.
        /// </summary>
        public string Path { get; set; }
    }
}
