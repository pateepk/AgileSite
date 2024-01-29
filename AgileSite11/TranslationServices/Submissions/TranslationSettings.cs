using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class encapsulating settings for manipulating with Translation Services (XLIFF format).
    /// </summary>
    public class TranslationSettings
    {
        #region "Variables"

        private bool mTranslateEditableItems = true;
        private bool? mTranslateWebpartProperties;
        private bool mTranslateDocCoupledData = true;
        private bool? mGenerateTargetTag;
        private DateTime mTranslationDeadline = DateTimeHelper.ZERO_TIME;
        private HashSet<string> mTargetLanguages;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets code name of the translation service used to submit a translation.
        /// </summary>
        public string TranslationServiceName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value which indicates whether to include editable regions into translation.
        /// </summary>
        public bool TranslateEditableItems
        {
            get
            {
                return mTranslateEditableItems;
            }
            set
            {
                mTranslateEditableItems = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which indicates whether to include webpart properties into translation.
        /// </summary>
        public bool TranslateWebpartProperties
        {
            get
            {
                if (mTranslateWebpartProperties == null)
                {
                    mTranslateWebpartProperties = SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSTranslateWebpartProperties");
                }
                return mTranslateWebpartProperties.Value;
            }
            set
            {
                mTranslateWebpartProperties = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which indicates whether to include coupled data fields into translation.
        /// </summary>
        public bool TranslateDocCoupledData
        {
            get
            {
                return mTranslateDocCoupledData;
            }
            set
            {
                mTranslateDocCoupledData = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which indicates whether to include attachments binary data fields into translation.
        /// </summary>
        public bool TranslateAttachments
        {
            get;
            set;
        }


        /// <summary>
        /// If true, target tag is generated and filled with source tag content while exporting to XLIFF (needed for Translations.com).
        /// If not set, the setting is initialized based on translation service setting.
        /// </summary>
        public bool GenerateTargetTag
        {
            get
            {
                if (mGenerateTargetTag == null)
                {
                    // Get settings from service if available
                    var service = TranslationServiceInfoProvider.GetTranslationServiceInfo(TranslationServiceName);
                    mGenerateTargetTag = (service != null) && service.TranslationServiceGenerateTargetTag;
                }

                return mGenerateTargetTag.Value;
            }
            set
            {
                mGenerateTargetTag = value;
            }
        }


        /// <summary>
        /// Gets or sets the target languages of the object/document.
        /// </summary>
        public HashSet<string> TargetLanguages
        {
            get
            {
                return mTargetLanguages ?? (mTargetLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
        }


        /// <summary>
        /// Gets or sets the source language of the object/document.
        /// </summary>
        public string SourceLanguage
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets unique identifier of a file within a submission.
        /// </summary>
        public string ItemIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the data type of the translation - constant according to XLIFF specification (htmlbody, plaintext, ...).
        /// </summary>
        public string DataType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the priority with which the translation will be submitted to the translation service (the higher the number the higher the priority).
        /// </summary>
        public int Priority
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the instructions which will be added to the translation submission.
        /// </summary>
        public string Instructions
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the deadline proposed for the translation.
        /// </summary>
        public DateTime TranslationDeadline
        {
            get
            {
                return mTranslationDeadline;
            }
            set
            {
                mTranslationDeadline = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates clone of translation settings
        /// </summary>
        public TranslationSettings Clone()
        {
            var settings = new TranslationSettings
            {
                TranslationServiceName = TranslationServiceName,
                TranslateEditableItems = TranslateEditableItems,
                TranslateWebpartProperties = TranslateWebpartProperties,
                TranslateDocCoupledData = TranslateDocCoupledData,
                TranslateAttachments = TranslateAttachments,
                mGenerateTargetTag = mGenerateTargetTag,
                SourceLanguage = SourceLanguage,
                ItemIdentifier = ItemIdentifier,
                DataType = DataType,
                Priority = Priority,
                Instructions = Instructions,
                TranslationDeadline = TranslationDeadline
            };
            settings.TargetLanguages.AddRange(TargetLanguages);

            return settings;
        }

        #endregion
    }
}
