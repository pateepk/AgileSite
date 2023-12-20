using System.Collections.Generic;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// View model for rendering Form item edit view.
    /// </summary>
    public class FormItemEdit
    {
        /// <summary>
        /// Specifies whether notification email is sent after form submission.
        /// </summary>
        public bool SendNotificationEmail { get; set; }

        
        /// <summary>
        /// Specifies whether autoresponder email is sent after form submission.
        /// </summary>
        public bool SendAutoResponderEmail { get; set; }

                
        /// <summary>
        /// Identifier of the form whose record is edited.
        /// </summary>
        public int FormId { get; set; }


        /// <summary>
        /// Identifier of the form record which is edited.
        /// </summary>
        public int FormItemId { get; set; }


        /// <summary>
        /// Form components with values used to render the form.
        /// </summary>
        public IEnumerable<FormComponent> FormComponents { get; set; }


        /// <summary>
        /// Specifies whether changes saved message is rendered.
        /// </summary>
        public bool ChangesSavedMessage { get; set; }


        /// <summary>
        /// URL for submitting edited form item.
        /// </summary>
        public string SubmitUrl { get; set; }
    }
}
