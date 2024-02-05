using System;
using System.Collections;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class defining string constants for each dialog parameter.
    /// </summary>
    public class DialogParameters
    {
        #region "Image constants"

        // General tab
        /// <summary>
        /// Image URL.
        /// </summary>
        public const string IMG_URL = "img_url";

        /// <summary>
        /// Image size added to URL.
        /// </summary>
        public const string IMG_SIZETOURL = "img_sizetourl";

        /// <summary>
        /// Image extension.
        /// </summary>
        public const string IMG_EXT = "img_ext";

        /// <summary>
        /// Image alternate text.
        /// </summary>
        public const string IMG_ALT = "img_alt";

        /// <summary>
        /// Image alternate text.
        /// </summary>
        public const string IMG_ALT_CLIENTID = "imgalt_clientid";

        /// <summary>
        /// Image width.
        /// </summary>
        public const string IMG_WIDTH = "img_width";

        /// <summary>
        /// Image height.
        /// </summary>
        public const string IMG_HEIGHT = "img_height";

        /// <summary>
        /// Image border width.
        /// </summary>
        public const string IMG_BORDERWIDTH = "img_borderwidth";

        /// <summary>
        /// Image border color.
        /// </summary>
        public const string IMG_BORDERCOLOR = "img_bordercolor";

        /// <summary>
        /// Image horizontal space.
        /// </summary>
        public const string IMG_HSPACE = "img_hspace";

        /// <summary>
        /// Image vertical space.
        /// </summary>
        public const string IMG_VSPACE = "img_vspace";

        /// <summary>
        /// Image alignment.
        /// </summary>
        public const string IMG_ALIGN = "img_align";

        // Advanced tab
        /// <summary>
        /// Image ID.
        /// </summary>
        public const string IMG_ID = "img_id";

        /// <summary>
        /// Image tooltip.
        /// </summary>
        public const string IMG_TOOLTIP = "img_tooltip";

        /// <summary>
        /// Image class.
        /// </summary>
        public const string IMG_CLASS = "img_class";

        /// <summary>
        /// Image style.
        /// </summary>
        public const string IMG_STYLE = "img_style";

        // Link tab
        /// <summary>
        /// Image link.
        /// </summary>
        public const string IMG_LINK = "img_link";

        /// <summary>
        /// Image target.
        /// </summary>
        public const string IMG_TARGET = "img_target";

        // Behaviour tab
        /// <summary>
        /// Image behavior.
        /// </summary>
        public const string IMG_BEHAVIOR = "img_behavior";

        // Additional parameters
        /// <summary>
        /// Image direction.
        /// </summary>
        public const string IMG_DIR = "img_dir";

        /// <summary>
        /// Image map definition.
        /// </summary>
        public const string IMG_USEMAP = "img_usemap";

        /// <summary>
        /// Image language definition.
        /// </summary>
        public const string IMG_LANG = "img_lang";

        /// <summary>
        /// Image long description definition.
        /// </summary>
        public const string IMG_LONGDESCRIPTION = "img_longdescription";

        /// <summary>
        /// Image original width.
        /// </summary>
        public const string IMG_ORIGINALWIDTH = "img_originalwidth";

        /// <summary>
        /// Image original height.
        /// </summary>
        public const string IMG_ORIGINALHEIGHT = "img_originalheight";

        #endregion


        #region "Link constants"

        // General tab
        /// <summary>
        /// Link text.
        /// </summary>
        public const string LINK_TEXT = "link_text";

        /// <summary>
        /// Link protocol.
        /// </summary>
        public const string LINK_PROTOCOL = "link_protocol";

        /// <summary>
        /// Link URL.
        /// </summary>
        public const string LINK_URL = "link_url";

        // Advanced tab
        /// <summary>
        /// Link target.
        /// </summary>
        public const string LINK_TARGET = "link_target";

        /// <summary>
        /// Link ID.
        /// </summary>
        public const string LINK_ID = "link_id";

        /// <summary>
        /// Link name.
        /// </summary>
        public const string LINK_NAME = "link_name";

        /// <summary>
        /// Link tool tip.
        /// </summary>
        public const string LINK_TOOLTIP = "link_tooltip";

        /// <summary>
        /// Link class.
        /// </summary>
        public const string LINK_CLASS = "link_class";

        /// <summary>
        /// Link style.
        /// </summary>
        public const string LINK_STYLE = "link_style";

        // Anchor
        /// <summary>
        /// Link anchor text.
        /// </summary>
        public const string ANCHOR_LINKTEXT = "anchor_linktext";

        /// <summary>
        /// Anchor name.
        /// </summary>
        public const string ANCHOR_NAME = "anchor_name";

        /// <summary>
        /// Anchor ID.
        /// </summary>
        public const string ANCHOR_ID = "anchor_id";

        /// <summary>
        /// Anchor custom.
        /// </summary>
        public const string ANCHOR_CUSTOM = "anchor_custom";

        // E-mail
        /// <summary>
        /// E-mail link text.
        /// </summary>
        public const string EMAIL_LINKTEXT = "email_linktext";

        /// <summary>
        /// E-mail to.
        /// </summary>
        public const string EMAIL_TO = "email_to";

        /// <summary>
        /// E-mail copy to.
        /// </summary>
        public const string EMAIL_CC = "email_cc";

        /// <summary>
        /// E-mail blind copy to.
        /// </summary>
        public const string EMAIL_BCC = "email_bcc";

        /// <summary>
        /// E-mail subject.
        /// </summary>
        public const string EMAIL_SUBJECT = "email_subject";

        /// <summary>
        /// E-mail body.
        /// </summary>
        public const string EMAIL_BODY = "email_body";

        #endregion


        #region "Audio/Video constants"

        // General tab
        /// <summary>
        /// Audio/Video URL.
        /// </summary>
        public const string AV_URL = "av_url";

        /// <summary>
        /// Audio/Video extension.
        /// </summary>
        public const string AV_EXT = "av_ext";

        /// <summary>
        /// Audio/Video width.
        /// </summary>
        public const string AV_WIDTH = "av_width";

        /// <summary>
        /// Audio/Video height.
        /// </summary>
        public const string AV_HEIGHT = "av_height";

        /// <summary>
        /// Audio/Video auto-play.
        /// </summary>
        public const string AV_AUTOPLAY = "av_autoplay";

        /// <summary>
        /// Audio/Video loop.
        /// </summary>
        public const string AV_LOOP = "av_loop";

        /// <summary>
        /// Audio/Video controls.
        /// </summary>
        public const string AV_CONTROLS = "av_controls";

        #endregion


        #region "YouTube constants"

        // General tab
        /// <summary>
        /// YouTube URL.
        /// </summary>
        public const string YOUTUBE_URL = "youtube_url";

        /// <summary>
        /// YouTube fullscreen.
        /// </summary>
        public const string YOUTUBE_FS = "youtube_fs";

        /// <summary>
        /// YouTube auto-play.
        /// </summary>
        public const string YOUTUBE_AUTOPLAY = "youtube_autoplay";

        /// <summary>
        /// YouTube related.
        /// </summary>
        public const string YOUTUBE_REL = "youtube_rel";

        /// <summary>
        /// YouTube width.
        /// </summary>
        public const string YOUTUBE_WIDTH = "youtube_width";

        /// <summary>
        /// YouTube height.
        /// </summary>
        public const string YOUTUBE_HEIGHT = "youtube_height";

        #endregion


        #region "Url constants"

        /// <summary>
        /// File url.
        /// </summary>
        public const string URL_URL = "url_url";

        /// <summary>
        /// Permanent version of the file URL.
        /// </summary>
        public const string URL_PERMANENT = "url_permanent";

        /// <summary>
        /// Direct version of the file URL.
        /// </summary>
        public const string URL_DIRECT = "url_direct";

        /// <summary>
        /// File guid.
        /// </summary>
        public const string URL_GUID = "url_guid";

        /// <summary>
        /// File extension.
        /// </summary>
        public const string URL_EXT = "url_ext";

        /// <summary>
        /// File width.
        /// </summary>
        public const string URL_WIDTH = "url_width";

        /// <summary>
        /// File height.
        /// </summary>
        public const string URL_HEIGHT = "url_height";

        /// <summary>
        /// File site naem.
        /// </summary>
        public const string URL_SITENAME = "url_sitename";

        /// <summary>
        /// Indicates if url is from old dialogs.
        /// </summary>
        public const string URL_OLDFORMAT = "url_oldformat";

        #endregion


        #region "Filesystem constants"

        /// <summary>
        /// Path to image in filesystem.
        /// </summary>
        public const string ITEM_PATH = "item_path";

        /// <summary>
        /// Resolved path to image
        /// </summary>
        public const string ITEM_RESOLVED_PATH = "item_resolved_path";

        /// <summary>
        /// Size of file.
        /// </summary>
        public const string ITEM_SIZE = "item_size";

        /// <summary>
        /// Determines if file name or folder name is contained.
        /// </summary>
        public const string ITEM_ISFILE = "item_isfile";

        /// <summary>
        /// Determines if file is specified with relative or absolute path.
        /// </summary>
        public const string ITEM_RELATIVEPATH = "item_relativepath";

        #endregion


        #region "Document constants"

        /// <summary>
        /// Node ID.
        /// </summary>
        public const string DOC_NODEALIASPATH = "doc_nodealiaspath";

        /// <summary>
        /// Targer Aliaspath.
        /// </summary>
        public const string DOC_TARGETNODEID = "doc_targetnodeid";

        #endregion


        #region "Global constants"

        /// <summary>
        /// Editor client id.
        /// </summary>
        public const string EDITOR_CLIENTID = "editor_clientid";

        /// <summary>
        /// Object type used in WYSIWYG editor.
        /// </summary>
        public const string OBJECT_TYPE = "cms_type";

        /// <summary>
        /// Indicates what kind of properties was last used.
        /// </summary>
        public const string LAST_TYPE = "last_type";

        /// <summary>
        /// File name.
        /// </summary>
        public const string FILE_NAME = "file_name";

        /// <summary>
        /// File size.
        /// </summary>
        public const string FILE_SIZE = "file_size";

        /// <summary>
        /// Content changed flag
        /// </summary>
        public const string CONTENT_CHANGED = "content_changed";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns initialized image parameters object.
        /// </summary>
        /// <param name="parameters">Collection with dialog parameters</param>        
        public static ImageParameters GetImageParameters(Hashtable parameters)
        {
            var imgParameters = new ImageParameters();

            imgParameters.Url = ValidationHelper.GetString(parameters[IMG_URL], null);
            imgParameters.SizeToURL = ValidationHelper.GetBoolean(parameters[IMG_SIZETOURL], false);
            imgParameters.Extension = ValidationHelper.GetString(parameters[IMG_EXT], null);
            imgParameters.Alt = ValidationHelper.GetString(parameters[IMG_ALT], null);
            imgParameters.Width = ValidationHelper.GetInteger(parameters[IMG_WIDTH], 0);
            imgParameters.Height = ValidationHelper.GetInteger(parameters[IMG_HEIGHT], 0);
            imgParameters.BorderWidth = ValidationHelper.GetInteger(parameters[IMG_BORDERWIDTH], 0);
            imgParameters.BorderColor = ValidationHelper.GetString(parameters[IMG_BORDERCOLOR], null);
            imgParameters.HSpace = ValidationHelper.GetInteger(parameters[IMG_HSPACE], 0);
            imgParameters.VSpace = ValidationHelper.GetInteger(parameters[IMG_VSPACE], 0);
            imgParameters.Align = ValidationHelper.GetString(parameters[IMG_ALIGN], null);
            imgParameters.Id = ValidationHelper.GetString(parameters[IMG_ID], null);
            imgParameters.Tooltip = ValidationHelper.GetString(parameters[IMG_TOOLTIP], null);
            imgParameters.Class = ValidationHelper.GetString(parameters[IMG_CLASS], null);
            imgParameters.Style = ValidationHelper.GetString(parameters[IMG_STYLE], null);
            imgParameters.Link = ValidationHelper.GetString(parameters[IMG_LINK], null);
            imgParameters.Behavior = ValidationHelper.GetString(parameters[IMG_BEHAVIOR], null);
            imgParameters.EditorClientID = ValidationHelper.GetString(parameters[EDITOR_CLIENTID], null);
            imgParameters.Dir = ValidationHelper.GetString(parameters[IMG_DIR], null);
            imgParameters.UseMap = ValidationHelper.GetString(parameters[IMG_USEMAP], null);
            imgParameters.Lang = ValidationHelper.GetString(parameters[IMG_LANG], null);
            imgParameters.LongDesc = ValidationHelper.GetString(parameters[IMG_LONGDESCRIPTION], null);

            return imgParameters;
        }


        /// <summary>
        /// Returns initialized AudioVideo parameters object.
        /// </summary>
        /// <param name="parameters">Collection with dialog parameters</param> 
        public static AudioVideoParameters GetAudioVideoParameters(Hashtable parameters)
        {
            var avParameters = new AudioVideoParameters();

            avParameters.SiteName = SiteContext.CurrentSiteName;
            avParameters.Url = ValidationHelper.GetString(parameters[AV_URL], null);
            avParameters.Extension = ValidationHelper.GetString(parameters[AV_EXT], null);
            avParameters.Width = ValidationHelper.GetInteger(parameters[AV_WIDTH], 0);
            avParameters.Height = ValidationHelper.GetInteger(parameters[AV_HEIGHT], 0);
            avParameters.AutoPlay = ValidationHelper.GetBoolean(parameters[AV_AUTOPLAY], false);
            avParameters.Loop = ValidationHelper.GetBoolean(parameters[AV_LOOP], false);
            avParameters.Controls = ValidationHelper.GetBoolean(parameters[AV_CONTROLS], true);
            avParameters.EditorClientID = ValidationHelper.GetString(parameters[EDITOR_CLIENTID], null);

            return avParameters;
        }


        /// <summary>
        /// Returns initialized YouTubeVideo parameters object.
        /// </summary>
        /// <param name="parameters">Collection with dialog parameters</param> 
        public static YouTubeVideoParameters GetYouTubeVideoParameters(Hashtable parameters)
        {
            var ytParameters = new YouTubeVideoParameters();

            ytParameters.Url = ValidationHelper.GetString(parameters[YOUTUBE_URL], null);
            ytParameters.Width = ValidationHelper.GetInteger(parameters[YOUTUBE_WIDTH], 0);
            ytParameters.Height = ValidationHelper.GetInteger(parameters[YOUTUBE_HEIGHT], 0);
            ytParameters.FullScreen = ValidationHelper.GetBoolean(parameters[YOUTUBE_FS], false);
            ytParameters.AutoPlay = ValidationHelper.GetBoolean(parameters[YOUTUBE_AUTOPLAY], false);
            ytParameters.RelatedVideos = ValidationHelper.GetBoolean(parameters[YOUTUBE_REL], true);
            ytParameters.EditorClientID = ValidationHelper.GetString(parameters[EDITOR_CLIENTID], null);

            return ytParameters;
        }

        #endregion
    }
}