namespace CMS.Base
{
    /// <summary>
    /// Event arguments for <see cref="SystemEvents.RestartRequired"/> event.
    /// </summary>
    public sealed class RestartRequiredEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Gets or sets whether the application was restarted during <see cref="SystemEvents.RestartRequired"/> event execution.
        /// </summary>
        public bool IsRestarted
        {
            get;
            set;
        }
    }
}
