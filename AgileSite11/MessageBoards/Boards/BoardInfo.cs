using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;
using CMS.DataEngine;
using CMS.MessageBoards;

[assembly: RegisterObjectType(typeof(BoardInfo), BoardInfo.OBJECT_TYPE)]

namespace CMS.MessageBoards
{
    /// <summary>
    /// BoardInfo data container class.
    /// </summary>
    public class BoardInfo : AbstractInfo<BoardInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.BOARD;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BoardInfoProvider), OBJECT_TYPE, "Board.Board", "BoardID", "BoardLastModified", "BoardGuid", "BoardName", "BoardDisplayName", null, "BoardSiteID", "BoardDocumentID", PredefinedObjectType.DOCUMENTLOCALIZATION)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("BoardUserID", UserInfo.OBJECT_TYPE),
            },
            MacroCollectionName = "CMS.MessageBoard",
            AllowRestore = false,
            GroupIDColumn = "BoardGroupID",
            ModuleName = ModuleName.MESSAGEBOARD,
            SupportsCloning = false,
            ImportExportSettings =
            {
                AllowSingleExport = false,
                IsExportable = true,
                LogExport = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, SOCIALANDCOMMUNITY),
                },
                ExcludedStagingColumns = new List<string>
                {
                    "BoardMessages"
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,

            EnabledColumn = "BoardEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "BoardLastMessageUserName",
                    "BoardLastMessageTime",
                    "BoardMessages"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Board site ID.
        /// </summary>
        public int BoardSiteID
        {
            get
            {
                return GetIntegerValue("BoardSiteID", 0);
            }
            set
            {
                SetValue("BoardSiteID", value);
            }
        }


        /// <summary>
        /// If board requires emails.
        /// </summary>
        public bool BoardRequireEmails
        {
            get
            {
                return GetBooleanValue("BoardRequireEmails", false);
            }
            set
            {
                SetValue("BoardRequireEmails", value);
            }
        }


        /// <summary>
        /// Board last message username.
        /// </summary>
        public string BoardLastMessageUserName
        {
            get
            {
                return GetStringValue("BoardLastMessageUserName", "");
            }
            set
            {
                SetValue("BoardLastMessageUserName", value);
            }
        }


        /// <summary>
        /// Board use captcha.
        /// </summary>
        public bool BoardUseCaptcha
        {
            get
            {
                return GetBooleanValue("BoardUseCaptcha", false);
            }
            set
            {
                SetValue("BoardUseCaptcha", value);
            }
        }


        /// <summary>
        /// Board access.
        /// </summary>
        public SecurityAccessEnum BoardAccess
        {
            get
            {
                return (SecurityAccessEnum)(ValidationHelper.GetInteger(GetValue("BoardAccess"), 0));
            }
            set
            {
                SetValue("BoardAccess", (int)value);
            }
        }


        /// <summary>
        /// Board description.
        /// </summary>
        public string BoardDescription
        {
            get
            {
                return GetStringValue("BoardDescription", "");
            }
            set
            {
                SetValue("BoardDescription", value);
            }
        }


        /// <summary>
        /// Board enable subscriptions.
        /// </summary>
        public bool BoardEnableSubscriptions
        {
            get
            {
                return GetBooleanValue("BoardEnableSubscriptions", false);
            }
            set
            {
                SetValue("BoardEnableSubscriptions", value);
            }
        }


        /// <summary>
        /// Board document ID.
        /// </summary>
        public int BoardDocumentID
        {
            get
            {
                return GetIntegerValue("BoardDocumentID", 0);
            }
            set
            {
                SetValue("BoardDocumentID", value, 0);
            }
        }


        /// <summary>
        /// Board user id.
        /// </summary>
        public int BoardUserID
        {
            get
            {
                return GetIntegerValue("BoardUserID", 0);
            }
            set
            {
                SetValue("BoardUserID", value, 0);
            }
        }


        /// <summary>
        /// Board group id.
        /// </summary>
        public int BoardGroupID
        {
            get
            {
                return GetIntegerValue("BoardGroupID", 0);
            }
            set
            {
                SetValue("BoardGroupID", value, 0);
            }
        }


        /// <summary>
        /// Board opened to.
        /// </summary>
        public DateTime BoardOpenedTo
        {
            get
            {
                return GetDateTimeValue("BoardOpenedTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BoardOpenedTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Board display name.
        /// </summary>
        public string BoardDisplayName
        {
            get
            {
                return GetStringValue("BoardDisplayName", "");
            }
            set
            {
                SetValue("BoardDisplayName", value);
            }
        }


        /// <summary>
        /// Board id.
        /// </summary>
        public int BoardID
        {
            get
            {
                return GetIntegerValue("BoardID", 0);
            }
            set
            {
                SetValue("BoardID", value);
            }
        }


        /// <summary>
        /// Board last modified.
        /// </summary>
        public DateTime BoardLastModified
        {
            get
            {
                return GetDateTimeValue("BoardLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BoardLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Board GUID.
        /// </summary>
        public Guid BoardGUID
        {
            get
            {
                return GetGuidValue("BoardGUID", Guid.Empty);
            }
            set
            {
                SetValue("BoardGUID", value);
            }
        }


        /// <summary>
        /// Board name.
        /// </summary>
        public string BoardName
        {
            get
            {
                return GetStringValue("BoardName", "");
            }
            set
            {
                SetValue("BoardName", value);
            }
        }


        /// <summary>
        /// Board moderated.
        /// </summary>
        public bool BoardModerated
        {
            get
            {
                return GetBooleanValue("BoardModerated", false);
            }
            set
            {
                SetValue("BoardModerated", value);
            }
        }


        /// <summary>
        /// Board last message time.
        /// </summary>
        public DateTime BoardLastMessageTime
        {
            get
            {
                return GetDateTimeValue("BoardLastMessageTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BoardLastMessageTime", value);
            }
        }


        /// <summary>
        /// Board opened from.
        /// </summary>
        public DateTime BoardOpenedFrom
        {
            get
            {
                return GetDateTimeValue("BoardOpenedFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BoardOpenedFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Returns inherited unsubscription url from settings.
        /// </summary>
        private string InheritedUnsubscriptionUrl
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(BoardSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSBoardUnsubsriptionURL"), "");
                }
                return String.Empty;
            }
        }


        /// <summary>
        /// Board unsubscription URL.
        /// </summary>
        public string BoardUnsubscriptionURL
        {
            get
            {
                return GetStringValue("BoardUnsubscriptionURL", InheritedUnsubscriptionUrl);
            }
            set
            {
                SetValue("BoardUnsubscriptionURL", value);
            }
        }


        /// <summary>
        /// Returns inherited base url from settings.
        /// </summary>
        private string InheritedBaseUrl
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(BoardSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSBoardBaseUrl"), "");
                }
                return String.Empty;
            }
        }


        /// <summary>
        /// Board base URL.
        /// </summary>
        public string BoardBaseURL
        {
            get
            {
                return GetStringValue("BoardBaseURL", InheritedBaseUrl);
            }
            set
            {
                SetValue("BoardBaseURL", value);
            }
        }


        /// <summary>
        /// Board enabled.
        /// </summary>
        public bool BoardEnabled
        {
            get
            {
                return GetBooleanValue("BoardEnabled", false);
            }
            set
            {
                SetValue("BoardEnabled", value);
            }
        }


        /// <summary>
        /// Board messages.
        /// </summary>
        public int BoardMessages
        {
            get
            {
                return GetIntegerValue("BoardMessages", 0);
            }
            set
            {
                SetValue("BoardMessages", value);
            }
        }


        /// <summary>
        /// Board opened.
        /// </summary>
        public bool BoardOpened
        {
            get
            {
                return GetBooleanValue("BoardOpened", false);
            }
            set
            {
                SetValue("BoardOpened", value);
            }
        }


        /// <summary>
        /// Indicates whether activity is logged.
        /// </summary>
        public bool BoardLogActivity
        {
            get
            {
                return GetBooleanValue("BoardLogActivity", false);
            }
            set
            {
                SetValue("BoardLogActivity", value);
            }
        }


        /// <summary>
        /// Returns inherited value indicating if the message board should use double opt-in from settings.
        /// </summary>
        private bool InheritedEnableOptIn
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(BoardSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetBoolean(SettingsKeyInfoProvider.GetBoolValue(si.SiteName + ".CMSBoardEnableOptIn"), false);
                }
                return false;
            }
        }


        /// <summary>
        /// Gets or sets whether the message board should use double opt-in.
        /// </summary>
        public bool BoardEnableOptIn
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("BoardEnableOptIn"), InheritedEnableOptIn);
            }
            set
            {
                SetValue("BoardEnableOptIn", value);
            }
        }


        /// <summary>
        /// Returns inherited value indicating whether subscription confirmation should be sent after double opt-in e-mail from settings.
        /// </summary>
        private bool InheritedSendOptInConfirmation
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(BoardSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetBoolean(SettingsKeyInfoProvider.GetBoolValue(si.SiteName + ".CMSBoardEnableOptInConfirmation"), false);
                }
                return false;
            }
        }


        /// <summary>
        /// Gets or sets whether subscription confirmation should be sent after double opt-in e-mail.
        /// </summary>
        public bool BoardSendOptInConfirmation
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("BoardSendOptInConfirmation"), InheritedSendOptInConfirmation);
            }
            set
            {
                SetValue("BoardSendOptInConfirmation", value);
            }
        }


        /// <summary>
        /// Returns inherited URL of the double opt-in page from settings.
        /// </summary>
        private string InheritedOptInApprovalURL
        {
            get
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(BoardSiteID);
                if (si != null)
                {
                    return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(si.SiteName + ".CMSBoardOptInApprovalPath"), "");
                }
                return String.Empty;
            }
        }


        /// <summary>
        /// Gets or sets the URL of the double opt-in page.
        /// </summary>
        public string BoardOptInApprovalURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("BoardOptInApprovalURL"), InheritedOptInApprovalURL);
            }
            set
            {
                SetValue("BoardOptInApprovalURL", value);
            }
        }


        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName => BoardInfoProvider.GetBoardFullName(BoardName, BoardDocumentID, BoardUserID, BoardGroupID);

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BoardInfoProvider.DeleteBoardInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BoardInfoProvider.SetBoardInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BoardInfo object.
        /// </summary>
        public BoardInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BoardInfo object from the given DataRow.
        /// </summary>
        public BoardInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Creates new board object according provided properties.
        /// </summary>
        /// <param name="properties">Board properties applied to created board</param>
        public BoardInfo(BoardProperties properties)
            : base(TYPEINFO)
        {
            // Create new message board according to webpart properties
            BoardEnabled = true;
            BoardAccess = properties.BoardAccess;
            BoardDescription = properties.BoardDescription;
            BoardDisplayName = properties.BoardDisplayName;
            BoardModerated = properties.BoardModerated;
            BoardName = (properties.BoardName != "") ? properties.BoardName : Guid.NewGuid().ToString();
            BoardOpened = properties.BoardOpened;
            BoardOpenedFrom = properties.BoardOpenedFrom;
            BoardOpenedTo = properties.BoardOpenedTo;
            BoardRequireEmails = properties.BoardRequireEmails;
            BoardUnsubscriptionURL = properties.BoardUnsubscriptionUrl;
            BoardBaseURL = properties.BoardBaseUrl;
            BoardUseCaptcha = properties.BoardUseCaptcha;
            BoardMessages = 0;
            BoardDocumentID = DocumentContext.CurrentPageInfo.DocumentID;
            BoardSiteID = SiteContext.CurrentSite.SiteID;
            BoardEnableSubscriptions = properties.BoardEnableSubscriptions;
            BoardLogActivity = properties.BoardLogActivity;

            // Switch by owner
            switch (properties.BoardOwner.ToLowerCSafe())
            {
                case "document":
                    BoardUserID = 0;
                    BoardGroupID = 0;
                    break;

                case "user":
                    BoardUserID = ((MembershipContext.CurrentUserProfile != null) ? MembershipContext.CurrentUserProfile.UserID : 0);
                    BoardGroupID = 0;
                    break;

                case "group":
                    BoardUserID = 0;
                    BoardGroupID = ModuleCommands.CommunityGetCurrentGroupID();
                    break;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            BoardDescription = "";
            BoardOpened = true;
            BoardEnabled = true;
            BoardAccess = SecurityAccessEnum.AllUsers;
            BoardModerated = false;
            BoardUseCaptcha = false;
            BoardMessages = 0;
        }

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Messages", m => m.Children[BoardMessageInfo.OBJECT_TYPE]);
        }
        
        #endregion
    }
}