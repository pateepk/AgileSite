using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Serves to expose uploaders properties necesary for its extensibility.
    /// </summary>
    public interface IUploadHandler
    {
        /// <summary>
        /// Event fired when additional parameters are constructed. These parameters are passed to the upload handler.
        /// Can be used to add custom parameters.
        /// </summary>
        event EventHandler<MfuAdditionalParameterEventArgs> AttachAdditionalParameters;


        /// <summary>
        /// Gets or sets the Upload handler's URL
        /// If not set or set to null automatic value according to uploader mode. is provided.
        /// Set to override automatic handler selection.
        /// </summary>
        string UploadHandlerUrl
        {
            get;
            set;
        }
    }
}
