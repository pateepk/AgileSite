using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Membership;
using CMS.DataEngine;
using CMS.MessageBoards;

[assembly: RegisterObjectType(typeof(BoardModeratorInfo), BoardModeratorInfo.OBJECT_TYPE)]

namespace CMS.MessageBoards
{
    /// <summary>
    /// BoardModeratorInfo data container class.
    /// </summary>
    public class BoardModeratorInfo : AbstractInfo<BoardModeratorInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.BOARDMODERATOR;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BoardModeratorInfoProvider), OBJECT_TYPE, "Board.Moderator", null, null, null, null, null, null, null, "BoardID", BoardInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("UserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            MacroCollectionName = "CMS.BoardModerator",
            ModuleName = ModuleName.MESSAGEBOARD,
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// User id.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// Board id.
        /// </summary>
        public virtual int BoardID
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BoardModeratorInfoProvider.DeleteBoardModeratorInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BoardModeratorInfoProvider.SetBoardModeratorInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BoardModeratorInfo object.
        /// </summary>
        public BoardModeratorInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BoardModeratorInfo object from the given DataRow.
        /// </summary>
        public BoardModeratorInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}