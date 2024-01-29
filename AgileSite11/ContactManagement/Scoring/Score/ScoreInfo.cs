using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(ScoreInfo), ScoreInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(ScoreInfo), ScoreInfo.OBJECT_TYPE_PERSONA)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ScoreInfo data container class.
    /// </summary>
    public class ScoreInfo : AbstractInfo<ScoreInfo>
    {
        #region "Constants"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.SCORE;


        /// <summary>
        /// Object type for the score that is used by persona
        /// </summary>
        public const string OBJECT_TYPE_PERSONA = "om.personascore";

        #endregion


        #region "Variables

        private IInfoObjectCollection mContacts;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ScoreInfoProvider), OBJECT_TYPE, "OM.Score", "ScoreID", "ScoreLastModified", "ScoreGUID", "ScoreName", "ScoreDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING),
                },
                ExcludedStagingColumns = new List<string>
                {
                    "ScoreStatus",
                    "ScoreScheduledTaskID"
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.SCORING,
            ImportExportSettings =
            {
                LogExport = false,
                IsExportable = false
            },
            EnabledColumn = "ScoreEnabled",
            IsTriggerTarget = true,
            Feature = FeatureEnum.LeadScoring,
            TypeCondition = new TypeCondition().WhereEquals("ScoreBelongsToPersona", false),
        };


        /// <summary>
        /// Type information for persona objects that use scoring module.
        /// </summary>
        public static ObjectTypeInfo PERSONASCORETYPEINFO = new ObjectTypeInfo(typeof(ScoreInfoProvider), OBJECT_TYPE_PERSONA, "OM.Score", "ScoreID", "ScoreLastModified", "ScoreGUID", "ScoreName", "ScoreDisplayName", null, null, null, null)
        {
            MacroCollectionName = "PersonaScore",
            OriginalTypeInfo = TYPEINFO,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.PERSONAS,
            ImportExportSettings =
            {
                LogExport = false,
                IsExportable = false,
            },
            EnabledColumn = "ScoreEnabled",
            Feature = FeatureEnum.Personas,
            TypeCondition = new TypeCondition().WhereEquals("ScoreBelongsToPersona", true),
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the display name of the score.
        /// </summary>
        public virtual string ScoreDisplayName
        {
            get
            {
                return GetStringValue("ScoreDisplayName", string.Empty);
            }
            set
            {
                SetValue("ScoreDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the description of the score.
        /// </summary>
        public virtual string ScoreDescription
        {
            get
            {
                return GetStringValue("ScoreDescription", string.Empty);
            }
            set
            {
                SetValue("ScoreDescription", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets or sets the ID of the score.
        /// </summary>
        public virtual int ScoreID
        {
            get
            {
                return GetIntegerValue("ScoreID", 0);
            }
            set
            {
                SetValue("ScoreID", value);
            }
        }


        /// <summary>
        /// Gets or sets the name of the score.
        /// </summary>
        public virtual string ScoreName
        {
            get
            {
                return GetStringValue("ScoreName", string.Empty);
            }
            set
            {
                SetValue("ScoreName", value);
            }
        }


        /// <summary>
        /// Gets or sets if the score is enabled.
        /// </summary>
        public virtual bool ScoreEnabled
        {
            get
            {
                return GetBooleanValue("ScoreEnabled", true);
            }
            set
            {
                SetValue("ScoreEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets score value at which notification e-mail should be sent.
        /// </summary>
        public virtual int ScoreEmailAtScore
        {
            get
            {
                return GetIntegerValue("ScoreEmailAtScore", 0);
            }
            set
            {
                SetValue("ScoreEmailAtScore", value, value > 0);
            }
        }


        /// <summary>
        /// Gets or sets notification e-mail.
        /// </summary>
        public virtual string ScoreNotificationEmail
        {
            get
            {
                return GetStringValue("ScoreNotificationEmail", string.Empty);
            }
            set
            {
                SetValue("ScoreNotificationEmail", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the score was last modified.
        /// </summary>
        public virtual DateTime ScoreLastModified
        {
            get
            {
                return GetDateTimeValue("ScoreLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ScoreLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the score's unique identifier.
        /// </summary>
        public virtual Guid ScoreGUID
        {
            get
            {
                return GetGuidValue("ScoreGUID", Guid.Empty);
            }
            set
            {
                SetValue("ScoreGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of scheduled task for recalculation.
        /// </summary>
        public virtual int ScoreScheduledTaskID
        {
            get
            {
                return GetIntegerValue("ScoreScheduledTaskID", 0);
            }
            set
            {
                SetValue("ScoreScheduledTaskID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the ScoreBelongsToPersona flag.
        /// </summary>
        public virtual bool ScoreBelongsToPersona
        {
            get
            {
                return GetBooleanValue("ScoreBelongsToPersona", false);
            }
            set
            {
                SetValue("ScoreBelongsToPersona", value);
            }
        }


        /// <summary>
        /// Gets or sets status of the score.
        /// </summary>
        public virtual ScoreStatusEnum ScoreStatus
        {
            get
            {
                return (ScoreStatusEnum)GetIntegerValue("ScoreStatus", -1);
            }
            set
            {
                SetValue("ScoreStatus", (int)value, ((int)value >= 0));
            }
        }


        /// <summary>
        /// Collection of all contacts within the scoring group.
        /// </summary>
        public virtual IInfoObjectCollection Contacts
        {
            get
            {
                if (mContacts != null)
                {
                    return mContacts;
                }

                var col = new InfoObjectCollection(ContactInfo.OBJECT_TYPE);
                col.AddCacheDependencies(String.Format("om.score|byid|{0}|children|om.scorecontactlist", ObjectID));
                col.Where = new WhereCondition(String.Format("ContactID IN (SELECT DISTINCT ContactID FROM OM_ScoreContactRule WHERE ScoreID = {0})", ObjectID));
                col.ChangeParent(null, Generalized);

                mContacts = col;
                return mContacts;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (!ScoreBelongsToPersona)
                {
                    return TYPEINFO;
                }
                else
                {
                    return PERSONASCORETYPEINFO;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ScoreInfoProvider.DeleteScoreInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ScoreInfoProvider.SetScoreInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ScoreInfo object.
        /// </summary>
        public ScoreInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ScoreInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ScoreInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes object dependencies
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            using (var scope = new CMSConnectionScope())
            {
                scope.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                TaskInfoProvider.DeleteTaskInfo(ScoreScheduledTaskID);

                base.RemoveObjectDependencies(deleteAll, clearHashtables);
            }
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // 3-way binding types are not supported by cloning, so ScoreContactRule object have to be excluded from cloning. Points have to be recalculated later manually.
            settings.ExcludedOtherBindingTypes.Add(ScoreContactRuleInfo.OBJECT_TYPE);

            ScoreStatus = ScoreStatusEnum.RecalculationRequired;
            ScoreScheduledTaskID = 0;
            Insert();
        }
        
        #endregion
    }
}