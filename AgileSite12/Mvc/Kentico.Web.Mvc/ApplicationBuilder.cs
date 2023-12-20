namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Configures the request pipeline using Kentico ASP.NET MVC integration features.
    /// </summary>
    public sealed class ApplicationBuilder : IApplicationBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBuilder"/> class.
        /// </summary>
        private ApplicationBuilder()
        {
        }


        /// <summary>
        /// Gets the current application builder.
        /// </summary>
        public static ApplicationBuilder Current { get; } = new ApplicationBuilder();
    }
}
