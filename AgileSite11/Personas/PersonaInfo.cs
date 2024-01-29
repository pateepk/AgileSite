using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Personas;

[assembly: RegisterObjectType(typeof(PersonaInfo), PersonaInfo.OBJECT_TYPE)]

namespace CMS.Personas
{
    /// <summary>
    /// PersonaInfo data container class.
    /// </summary>
    public class PersonaInfo : AbstractInfo<PersonaInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.PERSONA;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PersonaInfoProvider), OBJECT_TYPE, "Personas.Persona", "PersonaID", null, "PersonaGUID", "PersonaName", "PersonaDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("PersonaScoreID", ScoreInfo.OBJECT_TYPE_PERSONA, ObjectDependencyEnum.Required),
            },
            ModuleName = ModuleName.PERSONAS,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            MaxCodeNameLength = 200,
            ImportExportSettings =
            {
                LogExport = false,
                IsExportable = false,
            },
            Feature = FeatureEnum.Personas,
            AllowRestore = false,
            SupportsCloning = true,
            ThumbnailGUIDColumn = "PersonaPictureMetafileGUID",
            HasMetaFiles = true,
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
        /// Persona display name.
        /// </summary>
        public virtual string PersonaDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PersonaDisplayName"), "");
            }
            set
            {
                SetValue("PersonaDisplayName", value);
            }
        }


        /// <summary>
        /// Persona name.
        /// </summary>
        public virtual string PersonaName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PersonaName"), "");
            }
            set
            {
                SetValue("PersonaName", value);
            }
        }


        /// <summary>
        /// Persona description.
        /// </summary>
        public virtual string PersonaDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PersonaDescription"), "");
            }
            set
            {
                SetValue("PersonaDescription", value);
            }
        }


        /// <summary>
        /// Persona enabled.
        /// </summary>
        public virtual bool PersonaEnabled
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("PersonaEnabled"), true);
            }
            set
            {
                SetValue("PersonaEnabled", value);
            }
        }


        /// <summary>
        /// Persona GUID.
        /// </summary>
        public virtual Guid PersonaGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("PersonaGUID"), Guid.Empty);
            }
            set
            {
                SetValue("PersonaGUID", value);
            }
        }


        /// <summary>
        /// Persona score ID.
        /// </summary>
        public virtual int PersonaScoreID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PersonaScoreID"), 0);
            }
            set
            {
                SetValue("PersonaScoreID", value);
            }
        }


        /// <summary>
        /// Persona score limit.
        /// </summary>
        public virtual int PersonaPointsThreshold
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PersonaPointsThreshold"), 100);
            }
            set
            {
                SetValue("PersonaPointsThreshold", value);
            }
        }


        /// <summary>
        /// Guid of the metafile which represents persona picture. This metafile can belong to this PersonaInfo or to the persona ClassInfo. 
        /// Null means that persona does not have picture selected and default one should be used.
        /// </summary>
        public Guid? PersonaPictureMetafileGUID
        {
            get
            {
                Guid guid = GetGuidValue("PersonaPictureMetafileGUID", Guid.Empty);
                return guid == Guid.Empty ? (Guid?)null : guid;
            }
            set
            {
                SetValue("PersonaPictureMetafileGUID", value);
            }
        }


        /// <summary>
        /// Persona ScoreInfo object that is used for calculation of persona score.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ScoreInfo"/> for <see cref="PersonaScoreID"/> was not found.</exception>
        public ScoreInfo RelatedScore
        {
            get
            {
                var score = ScoreInfoProvider.GetScoreInfo(PersonaScoreID);
                if (score == null)
                {
                    throw new InvalidOperationException("ScoreInfo for persona with ID " + PersonaID + " could not be found.");
                }

                return score;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PersonaInfoProvider.DeletePersonaInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PersonaInfoProvider.SetPersonaInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PersonaInfo object.
        /// </summary>
        public PersonaInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PersonaInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public PersonaInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Clones related score before cloning persona.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            int originalScoreId = ((PersonaInfo)originalObject).PersonaScoreID;
            var originalScore = ScoreInfoProvider.GetScoreInfo(originalScoreId);

            var clonedScore = (ScoreInfo)originalScore.Generalized.InsertAsClone(settings, result);

            PersonaScoreID = clonedScore.ScoreID;

            base.InsertAsCloneInternal(settings, result, originalObject);
        }

        #endregion
    }
}