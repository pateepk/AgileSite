using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Membership;
using CMS.DataEngine;
using CMS.MessageBoards;

[assembly: RegisterObjectType(typeof(BoardRoleInfo), BoardRoleInfo.OBJECT_TYPE)]

namespace CMS.MessageBoards
{
    /// <summary>
    /// BoardRoleInfo data container class.
    /// </summary>
    public class BoardRoleInfo : AbstractInfo<BoardRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "board.boardrole";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BoardRoleInfoProvider), OBJECT_TYPE, "Board.BoardRole", null, null, null, null, null, null, null, "BoardID", BoardInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            ModuleName = ModuleName.MESSAGEBOARD,
            RegisterAsOtherBindingToObjectTypes = new List<string>
            {
                RoleInfo.OBJECT_TYPE, RoleInfo.OBJECT_TYPE_GROUP
            },
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Role id.
        /// </summary>
        public virtual int RoleID
        {
            get
            {
                return GetIntegerValue("RoleID", 0);
            }
            set
            {
                SetValue("RoleID", value);
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
            BoardRoleInfoProvider.DeleteBoardRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BoardRoleInfoProvider.SetBoardRoleInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BoardRoleInfo object.
        /// </summary>
        public BoardRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BoardRoleInfo object from the given DataRow.
        /// </summary>
        public BoardRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}