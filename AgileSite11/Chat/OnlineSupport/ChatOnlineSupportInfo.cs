using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Chat;

[assembly: RegisterObjectType(typeof(ChatOnlineSupportInfo), ChatOnlineSupportInfo.OBJECT_TYPE)]

namespace CMS.Chat
{
    /// <summary>
    /// ChatOnlineSupportInfo data container class.
    /// </summary>
    public class ChatOnlineSupportInfo : AbstractInfo<ChatOnlineSupportInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "chat.onlinesupport";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ChatOnlineSupportInfoProvider), OBJECT_TYPE, "Chat.OnlineSupport", "ChatOnlineSupportID", null, null, null, null, null, "ChatOnlineSupportSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("ChatOnlineSupportChatUserID", ChatUserInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            ModuleName = ModuleName.CHAT,
            MacroCollectionName = "OnlineChatSupportEngineer",
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Properties"

        
        /// <summary>
        /// DateTime of last support ping made by this instance.
        /// </summary>
        public virtual DateTime ChatOnlineSupportLastChecking
        {
            get
            {
                return GetDateTimeValue("ChatOnlineSupportLastChecking", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("ChatOnlineSupportLastChecking", value);
            }
        }


        /// <summary>
        /// Chat user who is online as a support.
        /// </summary>
        public virtual int ChatOnlineSupportChatUserID
        {
            get
            {
                return GetIntegerValue("ChatOnlineSupportChatUserID", 0);
            }
            set
            {         
                SetValue("ChatOnlineSupportChatUserID", value);
            }
        }


        /// <summary>
        /// ID.
        /// </summary>
        public virtual int ChatOnlineSupportID
        {
            get
            {
                return GetIntegerValue("ChatOnlineSupportID", 0);
            }
            set
            {         
                SetValue("ChatOnlineSupportID", value);
            }
        }


        /// <summary>
        /// Unique token.
        /// </summary>
        public virtual string ChatOnlineSupportToken
        {
            get
            {
                return GetStringValue("ChatOnlineSupportToken", null);
            }
            set
            {
                SetValue("ChatOnlineSupportToken", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ChatOnlineSupportInfoProvider.DeleteChatOnlineSupportInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ChatOnlineSupportInfoProvider.SetChatOnlineSupportInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ChatOnlineSupportInfo object.
        /// </summary>
        public ChatOnlineSupportInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ChatOnlineSupportInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ChatOnlineSupportInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
