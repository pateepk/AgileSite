using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Personas;

[assembly: RegisterObjectType(typeof(PersonaNodeInfo), PersonaNodeInfo.OBJECT_TYPE)]

namespace CMS.Personas
{
    /// <summary>
    /// PersonaNodeInfo data container class.
    /// </summary>
    public class PersonaNodeInfo : AbstractInfo<PersonaNodeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "personas.personanode";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PersonaNodeInfoProvider), OBJECT_TYPE, "Personas.PersonaNode", null, null, null, null, null, null, null, "PersonaID", PersonaInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
			{
			    new ObjectDependency("NodeID", PredefinedObjectType.NODE, ObjectDependencyEnum.Binding),
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = ModuleName.PERSONAS,
            ImportExportSettings =
            {
                LogExport = false,
                IsExportable = false
            },
            Feature = FeatureEnum.Personas,
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Persona ID.
        /// </summary>
        public virtual int PersonaID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PersonaID"), 0);
            }
            set
            {
                SetValue("PersonaID", value);
            }
        }


        /// <summary>
        /// Node ID.
        /// </summary>
        public virtual int NodeID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NodeID"), 0);
            }
            set
            {
                SetValue("NodeID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PersonaNodeInfoProvider.DeletePersonaNodeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PersonaNodeInfoProvider.SetPersonaNodeInfo(this);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.CONTENT, "Read", siteName, (UserInfo)userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return UserInfoProvider.IsAuthorizedPerResource(ModuleName.CONTENT, "Modify", siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PersonaNodeInfo object.
        /// </summary>
        public PersonaNodeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PersonaNodeInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public PersonaNodeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}