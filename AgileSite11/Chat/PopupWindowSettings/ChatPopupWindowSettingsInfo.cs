using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatPopupWindowSettingsInfo), ChatPopupWindowSettingsInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatPopupWindowSettings data container class.
    /// </summary>
    public class ChatPopupWindowSettingsInfo : AbstractInfo<ChatPopupWindowSettingsInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.popupwindowsettings";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatPopupWindowSettingsInfoProvider), OBJECT_TYPE, "Chat.PopupWindowSettings", "ChatPopupWindowSettingsID", null, null, null, null, null, null, null, null)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            ModuleName = ModuleName.CHAT,
            MacroCollectionName = "ChatPopupWindowSettings",
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Row id.
        /// </summary>
        public virtual int ChatPopupWindowSettingsID
        {
            get
            {
                return GetIntegerValue("ChatPopupWindowSettingsID", 0);
            }
            set
            {
                SetValue("ChatPopupWindowSettingsID", value);
            }
        }


        /// <summary>
        /// Row id.
        /// </summary>
        public virtual int ChatPopupWindowSettingsChecksum
        {
            get
            {
                return GetIntegerValue("ChatPopupWindowSettingsChecksum", 0);
            }
            set
            {
                SetValue("ChatPopupWindowSettingsChecksum", value);
            }
        }


        /// <summary>
        /// Name of transformation used for error messages.
        /// </summary>
        public virtual string ErrorTransformationName
        {
            get
            {
                return GetStringValue("ErrorTransformationName", "");
            }
            set
            {         
                SetValue("ErrorTransformationName", value);
            }
        }


        /// <summary>
        /// Name of transformation used for messages.
        /// </summary>
        public virtual string MessageTransformationName
        {
            get
            {
                return GetStringValue("MessageTransformationName", "");
            }
            set
            {         
                SetValue("MessageTransformationName", value);
            }
        }


        /// <summary>
        /// Name of transformation used for errors clearing label.
        /// </summary>
        public virtual string ErrorClearTransformationName
        {
            get
            {
                return GetStringValue("ErrorClearTransformationName", "");
            }
            set
            {
                SetValue("ErrorClearTransformationName", value);
            }
        }


        /// <summary>
        /// Name of transformation used for user in users list.
        /// </summary>
        public virtual string UserTransformationName
        {
            get
            {
                return GetStringValue("UserTransformationName", "");
            }
            set
            {
                SetValue("UserTransformationName", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatPopupWindowSettingsInfoProvider.DeleteChatPopupWindowSettings(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatPopupWindowSettingsInfoProvider.SetChatPopupWindowSettings(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatPopupWindowSettings object.
        /// </summary>
        public ChatPopupWindowSettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatPopupWindowSettings object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatPopupWindowSettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
