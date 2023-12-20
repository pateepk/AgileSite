using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.TranslationServices;

[assembly: RegisterObjectType(typeof(TranslationServiceInfo), TranslationServiceInfo.OBJECT_TYPE)]

namespace CMS.TranslationServices
{
    /// <summary>
    /// TranslationServiceInfo data container class.
    /// </summary>
    public class TranslationServiceInfo : AbstractInfo<TranslationServiceInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.translationservice";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TranslationServiceInfoProvider), OBJECT_TYPE, "CMS.TranslationService", "TranslationServiceID", "TranslationServiceLastModified", "TranslationServiceGUID", "TranslationServiceName", "TranslationServiceDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            AssemblyNameColumn = "TranslationServiceAssemblyName",
            EnabledColumn = "TranslationServiceEnabled",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled= true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Code name of the translation service.
        /// </summary>
        public virtual string TranslationServiceName
        {
            get
            {
                return GetStringValue("TranslationServiceName", "");
            }
            set
            {
                SetValue("TranslationServiceName", value);
            }
        }


        /// <summary>
        /// Custom parameter of the service.
        /// </summary>
        public virtual string TranslationServiceParameter
        {
            get
            {
                return GetStringValue("TranslationServiceParameter", "");
            }
            set
            {
                SetValue("TranslationServiceParameter", value);
            }
        }


        /// <summary>
        /// Display name of the translation service.
        /// </summary>
        public virtual string TranslationServiceDisplayName
        {
            get
            {
                return GetStringValue("TranslationServiceDisplayName", "");
            }
            set
            {
                SetValue("TranslationServiceDisplayName", value);
            }
        }


        /// <summary>
        /// Name of the assembly where the provider is implemented.
        /// </summary>
        public virtual string TranslationServiceAssemblyName
        {
            get
            {
                return GetStringValue("TranslationServiceAssemblyName", "");
            }
            set
            {
                SetValue("TranslationServiceAssemblyName", value);
            }
        }


        /// <summary>
        /// GUID of the translation service.
        /// </summary>
        public virtual Guid TranslationServiceGUID
        {
            get
            {
                return GetGuidValue("TranslationServiceGUID", Guid.Empty);
            }
            set
            {
                SetValue("TranslationServiceGUID", value);
            }
        }


        /// <summary>
        /// Name of the class where the provider is implemented.
        /// </summary>
        public virtual string TranslationServiceClassName
        {
            get
            {
                return GetStringValue("TranslationServiceClassName", "");
            }
            set
            {
                SetValue("TranslationServiceClassName", value);
            }
        }


        /// <summary>
        /// Determines whether the service requires to generate target tag in XLIFF in advance.
        /// </summary>
        public virtual bool TranslationServiceGenerateTargetTag
        {
            get
            {
                return GetBooleanValue("TranslationServiceGenerateTargetTag", false);
            }
            set
            {
                SetValue("TranslationServiceGenerateTargetTag", value);
            }
        }


        /// <summary>
        /// Determines whether the service is machine translation service (for example Google Translator) or classical translation service (for example Translations.com).
        /// </summary>
        public virtual bool TranslationServiceIsMachine
        {
            get
            {
                return GetBooleanValue("TranslationServiceIsMachine", false);
            }
            set
            {
                SetValue("TranslationServiceIsMachine", value);
            }
        }


        /// <summary>
        /// Determines whether the service is enabled or not.
        /// </summary>
        public virtual bool TranslationServiceEnabled
        {
            get
            {
                return GetBooleanValue("TranslationServiceEnabled", false);
            }
            set
            {
                SetValue("TranslationServiceEnabled", value);
            }
        }


        /// <summary>
        /// Determines whether the service supports instructions for submissions.
        /// </summary>
        public virtual bool TranslationServiceSupportsInstructions
        {
            get
            {
                return GetBooleanValue("TranslationServiceSupportsInstructions", false);
            }
            set
            {
                SetValue("TranslationServiceSupportsInstructions", value);
            }
        }


        /// <summary>
        /// Determines whether the service supports deadline for submissions.
        /// </summary>
        public virtual bool TranslationServiceSupportsDeadline
        {
            get
            {
                return GetBooleanValue("TranslationServiceSupportsDeadline", false);
            }
            set
            {
                SetValue("TranslationServiceSupportsDeadline", value);
            }
        }


        /// <summary>
        /// Determines whether the service supports prioritizing of submissions.
        /// </summary>
        public virtual bool TranslationServiceSupportsPriority
        {
            get
            {
                return GetBooleanValue("TranslationServiceSupportsPriority", false);
            }
            set
            {
                SetValue("TranslationServiceSupportsPriority", value);
            }
        }


        /// <summary>
        /// Determines whether the service supports canceling submissions.
        /// </summary>
        public virtual bool TranslationServiceSupportsCancel
        {
            get
            {
                return GetBooleanValue("TranslationServiceSupportsCancel", false);
            }
            set
            {
                SetValue("TranslationServiceSupportsCancel", value);
            }
        }


        /// <summary>
        /// Determines whether is allowed manual updating of submission status .
        /// </summary>
        public virtual bool TranslationServiceSupportsStatusUpdate
        {
            get
            {
                return GetBooleanValue("TranslationServiceSupportsStatusUpdate", false);
            }
            set
            {
                SetValue("TranslationServiceSupportsStatusUpdate", value);
            }
        }


        /// <summary>
        /// Last modified date of the translation service.
        /// </summary>
        public virtual DateTime TranslationServiceLastModified
        {
            get
            {
                return GetDateTimeValue("TranslationServiceLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TranslationServiceLastModified", value);
            }
        }


        /// <summary>
        /// ID of the translation service.
        /// </summary>
        public virtual int TranslationServiceID
        {
            get
            {
                return GetIntegerValue("TranslationServiceID", 0);
            }
            set
            {
                SetValue("TranslationServiceID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TranslationServiceInfoProvider.DeleteTranslationServiceInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TranslationServiceInfoProvider.SetTranslationServiceInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TranslationServiceInfo object.
        /// </summary>
        public TranslationServiceInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TranslationServiceInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TranslationServiceInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
