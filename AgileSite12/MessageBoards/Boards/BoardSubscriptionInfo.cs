using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.DataEngine;
using CMS.MessageBoards;

[assembly: RegisterObjectType(typeof(BoardSubscriptionInfo), BoardSubscriptionInfo.OBJECT_TYPE)]

namespace CMS.MessageBoards
{
    /// <summary>
    /// BoardSubscriptionInfo data container class.
    /// </summary>
    public class BoardSubscriptionInfo : AbstractInfo<BoardSubscriptionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.BOARDSUBSCRIPTION;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BoardSubscriptionInfoProvider), OBJECT_TYPE, "Board.Subscription", "SubscriptionID", "SubscriptionLastModified", "SubscriptionGUID", null, null, null, null, "SubscriptionBoardID", BoardInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SubscriptionUserID", UserInfo.OBJECT_TYPE)
            },
            MacroCollectionName = "CMS.BoardSubscription",
            AllowRestore = false,
            ModuleName = ModuleName.MESSAGEBOARD,
            LogEvents = true,
            TouchCacheDependencies = true,
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Subscription last modified.
        /// </summary>
        public virtual DateTime SubscriptionLastModified
        {
            get
            {
                return GetDateTimeValue("SubscriptionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SubscriptionLastModified", value);
            }
        }


        /// <summary>
        /// Subscription e-mail.
        /// </summary>
        public virtual string SubscriptionEmail
        {
            get
            {
                return GetStringValue("SubscriptionEmail", "");
            }
            set
            {
                SetValue("SubscriptionEmail", value);
            }
        }


        /// <summary>
        /// Subscription ID.
        /// </summary>
        public virtual int SubscriptionID
        {
            get
            {
                return GetIntegerValue("SubscriptionID", 0);
            }
            set
            {
                SetValue("SubscriptionID", value);
            }
        }


        /// <summary>
        /// Subscription GUID.
        /// </summary>
        public virtual Guid SubscriptionGUID
        {
            get
            {
                return GetGuidValue("SubscriptionGUID", Guid.Empty);
            }
            set
            {
                SetValue("SubscriptionGUID", value);
            }
        }


        /// <summary>
        /// Subscription board ID.
        /// </summary>
        public virtual int SubscriptionBoardID
        {
            get
            {
                return GetIntegerValue("SubscriptionBoardID", 0);
            }
            set
            {
                SetValue("SubscriptionBoardID", value);
            }
        }


        /// <summary>
        /// Subscription user ID.
        /// </summary>
        public virtual int SubscriptionUserID
        {
            get
            {
                return GetIntegerValue("SubscriptionUserID", 0);
            }
            set
            {
                SetValue("SubscriptionUserID", value, 0);
            }
        }


        /// <summary>
        /// Indicates if subscription approved.
        /// </summary>
        public virtual bool SubscriptionApproved
        {
            get
            {
                return GetBooleanValue("SubscriptionApproved", true);
            }
            set
            {
                SetValue("SubscriptionApproved", value);
            }
        }


        /// <summary>
        /// Subscription approval hash code.
        /// </summary>
        public virtual string SubscriptionApprovalHash
        {
            get
            {
                return GetStringValue("SubscriptionApprovalHash", null);
            }
            set
            {
                SetValue("SubscriptionApprovalHash", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BoardSubscriptionInfoProvider.DeleteBoardSubscriptionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BoardSubscriptionInfoProvider.SetBoardSubscriptionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BoardSubscriptionInfo object.
        /// </summary>
        public BoardSubscriptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BoardSubscriptionInfo object from the given DataRow.
        /// </summary>
        public BoardSubscriptionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}