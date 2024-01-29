using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Personas;

[assembly: RegisterObjectType(typeof(PersonaContactHistoryInfo), PersonaContactHistoryInfo.OBJECT_TYPE)]

namespace CMS.Personas
{
    /// <summary>
    /// PersonaContactHistory data container class.
    /// </summary>
    [Serializable]
    public class PersonaContactHistoryInfo : AbstractInfo<PersonaContactHistoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "personas.personacontacthistory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PersonaContactHistoryInfoProvider), OBJECT_TYPE, "Personas.PersonaContactHistory", "PersonaContactHistoryID", null, null, null, null, null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("PersonaContactHistoryPersonaID", PredefinedObjectType.PERSONA, ObjectDependencyEnum.Required)
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
        /// Persona contact history ID
        /// </summary>
		[DatabaseField]
        public virtual int PersonaContactHistoryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PersonaContactHistoryID"), 0);
            }
            set
            {
                SetValue("PersonaContactHistoryID", value);
            }
        }


        /// <summary>
        /// Persona contact history persona ID
        /// </summary>
		[DatabaseField]
        public virtual int? PersonaContactHistoryPersonaID
        {
            get
            {
                var value = GetIntegerValue("PersonaContactHistoryPersonaID", 0);
                return value == 0 ? null : (int?)value;
            }
            set
            {
                SetValue("PersonaContactHistoryPersonaID", value, value > 0);
            }
        }


        /// <summary>
        /// Persona contact history date
        /// </summary>
		[DatabaseField]
        public virtual DateTime PersonaContactHistoryDate
        {
            get
            {
                return ValidationHelper.GetDate(GetValue("PersonaContactHistoryDate"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PersonaContactHistoryDate", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Persona contact history contacts
        /// </summary>
		[DatabaseField]
        public virtual int PersonaContactHistoryContacts
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PersonaContactHistoryContacts"), 0);
            }
            set
            {
                SetValue("PersonaContactHistoryContacts", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PersonaContactHistoryInfoProvider.DeletePersonaContactHistoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PersonaContactHistoryInfoProvider.SetPersonaContactHistoryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected PersonaContactHistoryInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty PersonaContactHistoryInfo object.
        /// </summary>
        public PersonaContactHistoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PersonaContactHistoryInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public PersonaContactHistoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
