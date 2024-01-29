using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.MacroEngine;

[assembly: RegisterObjectType(typeof(MacroRuleInfo), MacroRuleInfo.OBJECT_TYPE)]

namespace CMS.MacroEngine
{
    /// <summary>
    /// MacroRuleInfo data container class.
    /// </summary>
    [Serializable]
    public class MacroRuleInfo : AbstractInfo<MacroRuleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.macrorule";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MacroRuleInfoProvider), OBJECT_TYPE, "CMS.MacroRule", "MacroRuleID", "MacroRuleLastModified", "MacroRuleGUID", "MacroRuleName", "MacroRuleDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                    {
                        new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                    }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            EnabledColumn = "MacroRuleEnabled",
            FormDefinitionColumn = "MacroRuleParameters",
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("MacroRuleParameters")
                }
            },
            DefaultData = new DefaultDataSettings()
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the Macro rule.
        /// </summary>
        public virtual int MacroRuleID
        {
            get
            {
                return GetIntegerValue("MacroRuleID", 0);
            }
            set
            {
                SetValue("MacroRuleID", value);
            }
        }


        /// <summary>
        /// Indicates whether the Macro rule is enabled or not (determines visibility in macro rule designer).
        /// </summary>
        public virtual bool MacroRuleEnabled
        {
            get
            {
                return GetBooleanValue("MacroRuleEnabled", false);
            }
            set
            {
                SetValue("MacroRuleEnabled", value);
            }
        }


        /// <summary>
        /// Indicates whether the Macro rule is custom or defined by CMS (for upgrade purposes).
        /// </summary>
        public virtual bool MacroRuleIsCustom
        {
            get
            {
                return GetBooleanValue("MacroRuleIsCustom", false);
            }
            set
            {
                SetValue("MacroRuleIsCustom", value);
            }
        }


        /// <summary>
        /// Code name of the Macro rule.
        /// </summary>
        public virtual string MacroRuleName
        {
            get
            {
                return GetStringValue("MacroRuleName", "");
            }
            set
            {
                SetValue("MacroRuleName", value);
            }
        }


        /// <summary>
        /// Human readable text of the Macro rule (including special macros for parameters).
        /// </summary>
        public virtual string MacroRuleText
        {
            get
            {
                return GetStringValue("MacroRuleText", "");
            }
            set
            {
                SetValue("MacroRuleText", value);
            }
        }


        /// <summary>
        /// Human readable text of the Macro rule (without parameters) - displayed in the list of available rules.
        /// </summary>
        public virtual string MacroRuleDisplayName
        {
            get
            {
                return GetStringValue("MacroRuleDisplayName", "");
            }
            set
            {
                SetValue("MacroRuleDisplayName", value);
            }
        }


        /// <summary>
        /// Name of the resource the rule belongs to.
        /// </summary>
        public virtual string MacroRuleResourceName
        {
            get
            {
                return GetStringValue("MacroRuleResourceName", "");
            }
            set
            {
                SetValue("MacroRuleResourceName", value);
            }
        }


        /// <summary>
        /// List of data items (separated with semicolon) required for the rule to be displayed.
        /// </summary>
        public virtual string MacroRuleRequiredData
        {
            get
            {
                return GetStringValue("MacroRuleRequiredData", "");
            }
            set
            {
                SetValue("MacroRuleRequiredData", value);
            }
        }



        /// <summary>
        /// Condition of the rule in K# language (with macros for parameters).
        /// </summary>
        public virtual string MacroRuleCondition
        {
            get
            {
                return GetStringValue("MacroRuleCondition", "");
            }
            set
            {
                SetValue("MacroRuleCondition", value);
            }
        }


        /// <summary>
        /// Description of the macro rule.
        /// </summary>
        public virtual string MacroRuleDescription
        {
            get
            {
                return GetStringValue("MacroRuleDescription", "");
            }
            set
            {
                SetValue("MacroRuleDescription", value);
            }
        }


        /// <summary>
        /// XML defining parameters of the Macro rule.
        /// </summary>
        public virtual string MacroRuleParameters
        {
            get
            {
                return GetStringValue("MacroRuleParameters", "");
            }
            set
            {
                SetValue("MacroRuleParameters", value);
            }
        }


        /// <summary>
        /// Macor rule last modified.
        /// </summary>
        public virtual DateTime MacroRuleLastModified
        {
            get
            {
                return GetDateTimeValue("MacroRuleLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MacroRuleLastModified", value);
            }
        }


        /// <summary>
        /// Macor rule GUID.
        /// </summary>
        public virtual Guid MacroRuleGUID
        {
            get
            {
                return GetGuidValue("MacroRuleGUID", Guid.Empty);
            }
            set
            {
                SetValue("MacroRuleGUID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MacroRuleInfoProvider.DeleteMacroRuleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MacroRuleInfoProvider.SetMacroRuleInfo(this);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // All cloned rules are custom
            if (!SystemContext.DevelopmentMode)
            {
                this.MacroRuleIsCustom = true;
            }

            this.Insert();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MacroRuleInfo object.
        /// </summary>
        public MacroRuleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MacroRuleInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public MacroRuleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
