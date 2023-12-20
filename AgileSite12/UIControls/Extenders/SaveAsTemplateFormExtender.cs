using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder.Internal;
using CMS.EventLog;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass(nameof(SaveAsTemplateFormExtender), typeof(SaveAsTemplateFormExtender))]

namespace CMS.UIControls
{
    /// <summary>
    /// Save as template UIForm extender.
    /// </summary>
    public class SaveAsTemplateFormExtender : ControlExtender<UIForm>
    {
        private TreeNode CurrentPage { get; } = DocumentHelper.GetDocument(UIContext.Current.ParentObjectID, null);


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.Page.Load += Page_Load;
            Control.OnBeforeSave += Control_OnBeforeSave;
        }


        /// <summary>
        /// Occurs when the server control is loaded.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="eventArgs">Event arguments</param>
        private void Page_Load(object sender, EventArgs eventArgs)
        {
            Control.SubmitButton.Visible = false;

            if (Control.Page is CMSUIPage page)
            {
                var btnSave = new LocalizedButton
                {
                    ResourceString = "general.save",
                };

                btnSave.Click += (s, e) =>
                {
                    if (Control.SaveData(null))
                    {
                        // Do not display the "Change were saved" message in the modal dialog. This message will be displayed in the main frame.
                        Control.MessagesPlaceHolder.Confirmation = false;
                    }
                };

                if (page.IsDialog)
                {
                    page.DialogFooter.AddControl(btnSave);
                }
            }
        }


        /// <summary>
        /// Event called during saving process after data are retrieved from form controls
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="eventArgs">Event arguments</param>
        private void Control_OnBeforeSave(object sender, EventArgs eventArgs)
        {
            var loader = Service.Resolve<IPageBuilderConfigurationSourceLoader>();
            var source = loader.Load(CurrentPage);
            var templateConfiguration = source.PageTemplateConfiguration;
            var widgetsConfiguration = source.WidgetsConfiguration;

            if (String.IsNullOrEmpty(templateConfiguration))
            {
                ShowError("Cannot create a new page template from a page which is not based on an existing page template.", "SAVETEMPLATE");
                return;
            }

            var templateConfigurationInfo = Control.EditedObject as PageTemplateConfigurationInfo;
            if (templateConfigurationInfo == null)
            {
                ShowError("Edited object is not of type PageTemplateConfigurationInfo.", "SAVETEMPLATE");
                return;
            }

            templateConfigurationInfo.PageTemplateConfigurationWidgets = widgetsConfiguration;
            templateConfigurationInfo.PageTemplateConfigurationTemplate = templateConfiguration;
        }


        private void ShowError(string message, string errorCode)
        {
            Control.AddError(ResHelper.GetString("pagetemplatesmvc.saveastemplate.error"));
            Control.StopProcessing = true;

            EventLogProvider.LogEvent(EventType.ERROR, "SaveAsTemplateFormExtender", errorCode, message);
        }
    }
}
