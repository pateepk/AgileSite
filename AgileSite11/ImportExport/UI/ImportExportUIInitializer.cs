using System;
using System.Linq;
using System.Text;


namespace CMS.CMSImportExport
{
    internal static class ImportExportUIInitializer
    {
        private const string BASE_EXPORT_CONTROL_PATH = "~/CMSModules/ImportExport/Controls/Export/";
        private const string BASE_EXPORT_SITE_CONTROL_PATH = "~/CMSModules/ImportExport/Controls/Export/Site/";

        private const string BASE_IMPORT_CONTROL_PATH = "~/CMSModules/ImportExport/Controls/Import/";
        private const string BASE_IMPORT_SITE_CONTROL_PATH = "~/CMSModules/ImportExport/Controls/Import/Site/";


        private static readonly string[][] EXPORT_CONTROLS =
        {
            new [] {"##objects##", "__objects__.ascx"},
            new [] {"cms.customtable", "cms_customtable.ascx"},
            new [] {"cms.resource", "cms_resource.ascx"},
        };


        private static readonly string[][] EXPORT_SITE_CONTROLS =
        {
            new [] {"board.board", "board_board.ascx"},
            new [] {"cms.document", "cms_document.ascx"},
            new [] {"cms.form", "cms_form.ascx"},
            new [] {"community.group", "community_group.ascx"},
            new [] {"forums.forum", "forums_forum.ascx"},
            new [] {"media.library", "media_library.ascx"},
        };


        private static readonly string[][] IMPORT_CONTROLS =
        {
            new [] {"##objects##", "__objects__.ascx"},
            new [] {"cms.customtable", "cms_customtable.ascx"},
            new [] {"cms.formusercontrol", "cms_formusercontrol.ascx"},
            new [] {"cms.pagetemplate", "cms_pagetemplate.ascx"},
            new [] {"cms.resource", "cms_resource.ascx"},
            new [] {"cms.user", "cms_user.ascx"},
            new [] {"cms.webpart", "cms_webpart.ascx"},
            new [] {"cms.widget", "cms_widget.ascx"},
            new [] {"cms.workflow", "cms_workflow.ascx"},
            new [] {"ma.automationprocess", "ma_automationprocess.ascx"}
        };


        private static readonly string[][] IMPORT_SITE_CONTROLS =
        {
            new [] {"board.board", "board_board.ascx"},
            new [] {"cms.document", "cms_document.ascx"},
            new [] {"cms.form", "cms_form.ascx"},
            new [] {"cms.pagetemplate", "cms_pagetemplate.ascx"},
            new [] {"community.group", "community_group.ascx"},
            new [] {"forums.forum", "forums_forum.ascx"},
            new [] {"media.library", "media_library.ascx"},
        };


        public static void InitDefaultSettingsControls()
        {
            EXPORT_CONTROLS.ToList().ForEach(it => ExportSettingsControlsRegister.RegisterSettingsControl(it[0], BASE_EXPORT_CONTROL_PATH + it[1]));
            EXPORT_SITE_CONTROLS.ToList().ForEach(it => ExportSettingsControlsRegister.RegisterSiteSettingsControl(it[0], BASE_EXPORT_SITE_CONTROL_PATH + it[1]));

            IMPORT_CONTROLS.ToList().ForEach(it => ImportSettingsControlsRegister.RegisterSettingsControl(it[0], BASE_IMPORT_CONTROL_PATH + it[1]));
            IMPORT_SITE_CONTROLS.ToList().ForEach(it => ImportSettingsControlsRegister.RegisterSiteSettingsControl(it[0], BASE_IMPORT_SITE_CONTROL_PATH + it[1]));
        }
    }
}
