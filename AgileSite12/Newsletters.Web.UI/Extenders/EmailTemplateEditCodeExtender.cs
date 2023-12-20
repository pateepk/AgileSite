using System;
using System.Linq;
using System.Web.UI;

using CMS;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.Newsletters.Web.UI;
using CMS.PortalEngine.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("TemplateEditCodeExtender", typeof(EmailTemplateEditCodeExtender))]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Email marketing template UIForm extender.
    /// </summary>
    public class EmailTemplateEditCodeExtender : ControlExtender<UIForm>
    {
        private FormEngineUserControl mTemplateCodeField = null;
        private ExtendedTextArea mTemplateCodeEditor = null;

        /// <summary>
        /// Returns the form control for editing email template code.
        /// </summary>
        private FormEngineUserControl TemplateCodeField
        {
            get
            {
                if (mTemplateCodeField == null)
                {
                    mTemplateCodeField = Control.FieldControls["TemplateCode"];
                }

                return mTemplateCodeField;
            }
        }


        /// <summary>
        /// Returns the editor control for email template code field.
        /// </summary>
        private ExtendedTextArea TemplateCodeEditor
        {
            get
            {
                if (mTemplateCodeEditor == null)
                {
                    mTemplateCodeEditor = ControlsHelper.GetChildControl<ExtendedTextArea>(TemplateCodeField, false);
                }

                return mTemplateCodeEditor;
            }
        }


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.Page.PreRender += Page_PreRender;
            Control.OnItemValidation += Control_OnItemValidation;
            Control.OnAfterDataLoad += Control_OnAfterDataLoad;
        }

        private void Control_OnAfterDataLoad(object sender, EventArgs e)
        {
            ShowSmartTipForIssueTemplateType();
            ShowInsertImageButton();
        }


        private void Page_PreRender(object sender, EventArgs e)
        {
            InitHeaderActions();

            RenderInsertAttachmentUrlScript();

            RegisterScriptForInsertImageDialog();
        }


        private void Control_OnItemValidation(object sender, ref string errorMessage)
        {
            var control = sender as FormEngineUserControl;
            if (control == null || !control.HasValue || String.IsNullOrEmpty(control.Field))
            {
                return;
            }

            if (control.Field.Equals("TemplateCode", StringComparison.Ordinal))
            {
                //Check that email widget zone names are unique
                var templateCode = control.Value as string;
                var duplicatedPlaceholders = WidgetZonePlaceholderHelper.GetDuplicatedPlaceholders(templateCode);
                if (duplicatedPlaceholders.Any())
                {
                    errorMessage += ResHelper.GetStringFormat("newsletter.templatecode.validation.zonenamenotunique", duplicatedPlaceholders.Join(", "));
                }
            }
        }


        private void ShowSmartTipForIssueTemplateType()
        {
            var emailTemplate = Control.EditedObject as EmailTemplateInfo;
            if (emailTemplate == null)
            {
                return;
            }

            var smartTip = ControlsHelper.GetChildControl<SmartTipControl>(Control.Page);
            if (smartTip != null)
            {
                smartTip.Visible = IsIssueTemplateType(emailTemplate);
            }
        }


        private static bool IsIssueTemplateType(EmailTemplateInfo emailTemplate)
        {
            return emailTemplate.TemplateType == EmailTemplateTypeEnum.Issue;
        }


        private void InitHeaderActions()
        {
            EmailTemplateInfo emailTemplate = Control.EditedObject as EmailTemplateInfo;

            if ((emailTemplate != null) && (emailTemplate.TemplateID > 0))
            {
                Page page = Control.Page;

                ObjectEditMenu menu = (ObjectEditMenu)ControlsHelper.GetChildControl(page, typeof(ObjectEditMenu));
                if (menu != null)
                {
                    RegisterEditPageHeaderActions(page, menu, emailTemplate);
                }
            }
        }


        /// <summary>
        /// Registers email template actions headers to given page and menu item
        /// </summary>
        /// <param name="page">Page where the action is registered</param>
        /// <param name="menu">Menu where the action is registered</param>
        /// <param name="emailTemplate">Email template for which actions are registered</param>
        private void RegisterEditPageHeaderActions(Page page, ObjectEditMenu menu, EmailTemplateInfo emailTemplate)
        {
            ScriptHelper.RegisterDialogScript(page);

            RegisterAttachments(page, menu, emailTemplate);
        }


        private void RegisterAttachments(Page page, ObjectEditMenu menu, EmailTemplateInfo emailTemplate)
        {
            const string ATTACHMENTS_ACTION_CLASS = "attachments-header-action";

            // Register attachments count update module
            ScriptHelper.RegisterModule(page, "CMS/AttachmentsCountUpdater", new
            {
                Selector = "." + ATTACHMENTS_ACTION_CLASS,
                Text = ResHelper.GetString("general.attachments")
            });

            // Prepare metafile dialog URL
            var metaFileDialogUrl = UrlResolver.ResolveUrl(@"~/CMSModules/AdminControls/Controls/MetaFiles/MetaFileDialog.aspx");
            var query = $"?objectid={emailTemplate.TemplateID}&objecttype={EmailTemplateInfo.OBJECT_TYPE}&siteid={emailTemplate.TemplateSiteID}&hideobjectmenu=true&pasteVirtualPath=true";
            metaFileDialogUrl += $"{query}&category={ObjectAttachmentsCategories.TEMPLATE}&hash={QueryHelper.GetHash(query)}";

            var attachCount = GetAttachmentsCount(emailTemplate);
            var attachmentsCaption = ResHelper.GetString("general.attachments");
            bool isAuthorized = MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Newsletter", "ManageTemplates");

            // Add attachments action
            menu.AddExtraAction(new HeaderAction
            {
                Text = attachmentsCaption + (attachCount > 0 ? $" ({attachCount})" : string.Empty),
                Tooltip = attachmentsCaption,
                OnClientClick = $"if (modalDialog) {{modalDialog('{metaFileDialogUrl}', 'Attachments', '700', '500');}} return false;",
                Enabled = isAuthorized,
                CssClass = ATTACHMENTS_ACTION_CLASS,
                ButtonStyle = ButtonStyle.Default
            });
        }


        private int GetAttachmentsCount(EmailTemplateInfo emailTemplate)
        {
            var objectQuery = MetaFileInfoProvider.GetMetaFiles();
            objectQuery.WhereCondition = MetaFileInfoProvider.GetWhereCondition(emailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
            objectQuery.Columns(MetaFileInfo.TYPEINFO.IDColumn);

            return objectQuery.Count;
        }


        /// <summary>
        /// Renders the script which insert the selected attachment's URL into the template code editor.
        /// </summary>
        private void RenderInsertAttachmentUrlScript()
        {
            string codemirror = $"{TemplateCodeField.InputClientID}_editor";

            string script = ($@"
function PasteImage(imageurl) {{
    {codemirror}.replaceSelection(imageurl);
}}");

            ScriptHelper.RegisterClientScriptBlock(Control.Page, typeof(string), "InsertAttachmentURLScript_" + Control.ClientID, ScriptHelper.GetScript(script));
        }


        private DialogConfiguration GetInsertImageDialogConfiguration()
        {
            return new DialogConfiguration
            {
                ContentUseRelativeUrl = true,
                OutputFormat = OutputFormatEnum.URL,
                SelectableContent = SelectableContentEnum.OnlyImages,
                HideAnchor = true,
                HideContent = true,
                HideEmail = true,
                HideAttachments = true,
                HideWeb = true,
                LibGroupLibraries = AvailableLibrariesEnum.None,
                LibGroups = AvailableGroupsEnum.None,
                LibSites = AvailableSitesEnum.OnlyCurrentSite,
            };
        }


        private void ShowInsertImageButton()
        {
            if (TemplateCodeEditor != null)
            {
                TemplateCodeEditor.InsertImageDialogConfiguration = GetInsertImageDialogConfiguration();
                TemplateCodeEditor.ShowInsertImage = true;
            }
        }


        private void RegisterScriptForInsertImageDialog()
        {
            if (TemplateCodeEditor == null)
            {
                return;
            }

            // "SetUrl" is a function which the dialog calls after the value is selected.
            string script = $@"
function SetUrl(url) {{
    var editor = window['{TemplateCodeEditor.EditorID}'];
    if (editor) {{
        editor.replaceSelection(url);
        editor.focus();
    }}
}}
";

            ScriptHelper.RegisterClientScriptBlock(Control.Page, typeof(string), "SetUrlScript_" + Control.ClientID, ScriptHelper.GetScript(script));
        }
    }
}