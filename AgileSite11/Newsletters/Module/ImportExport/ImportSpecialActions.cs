using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.CMSImportExport;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetObjectWhereCondition.Execute += GetObjectWhereCondition_Execute;

            ImportExportEvents.GetImportData.After += InformAboutDatalossEmailTemplates;

            ImportExportEvents.GetImportData.After += UpdateNewsletterIssueImportDataFromVersion10;

            ImportExportEvents.GetImportData.After += UpdateNewsletterIssueWithIncorrectlyNamedVariants;

            ImportExportEvents.ImportObject.Before += SetEmailTemplateInlineCSSValue;

            ImportExportEvents.ImportChild.Before += ImportChildOnBefore;
        }


        private static void ImportChildOnBefore(object sender, ImportEventArgs e)
        {
            var parent = e.ParentObject;
            var child = e.Object;

            if (parent.TypeInfo.ObjectType == NewsletterInfo.OBJECT_TYPE && child.TypeInfo.ObjectType == IssueInfo.OBJECT_TYPE_VARIANT)
            {
                e.Cancel();
            }
        }


        private static void GetObjectWhereCondition_Execute(object sender, GetObjectWhereConditionArgs e)
        {
            var objectType = e.ObjectType;
            var settings = e.Settings;
            if (objectType == IssueInfo.OBJECT_TYPE)
            {
                string newsletterWhere = settings.GetObjectWhereCondition(NewsletterInfo.OBJECT_TYPE, true).ToString(true);

                e.Where.Where("IssueNewsletterID IN (SELECT NewsletterID FROM Newsletter_Newsletter" + ((newsletterWhere != null) ? " WHERE " + newsletterWhere : "") + ")");
                e.CombineWhereCondition = false;
            }
        }


        /// <summary>
        /// Inform about dataloss when importing email templates from older versions.
        /// </summary>
        private static void InformAboutDatalossEmailTemplates(object sender, ImportGetDataEventArgs e)
        {
            if (!e.SelectionOnly && e.Settings.IsLowerVersion("11.0") && (e.Data.Tables["Newsletter_EmailTemplate"] != null)
                && (e.Settings.GetSelectedObjects(EmailTemplateInfo.OBJECT_TYPE, true) != null))
            {
                e.Settings.LogProgressState(LogStatusEnum.Warning, "Marketing email templates were imported, but the data from the 'TemplateBody', 'TemplateStylesheetText', 'TemplateHeader' and 'TemplateFooter' database columns was deleted. These columns were removed from the 'Newsletter_EmailTemplate' database table in Kentico 11 and replaced by the new 'TemplateCode'.");
            }
        }


        /// <summary>
        /// Adds content from <see cref="IssueInfo.IssueSubject"/> to <see cref="IssueInfo.IssueDisplayName"/> for objects imported from version 10.
        /// </summary>
        private static void UpdateNewsletterIssueImportDataFromVersion10(object sender, ImportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == NewsletterInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var data = e.Data;

                if (!settings.IsOlderVersion || DataHelper.DataSourceIsEmpty(data))
                {
                    return;
                }

                var issues = data.Tables["Newsletter_NewsletterIssue"];
                PopulateIssueDisplayName(issues, settings);
            }
        }


        /// <summary>
        /// Populates <see cref="IssueInfo.IssueDisplayName"/>s stored in <paramref name="issues"/> with <see cref="IssueInfo.IssueSubject"/>.
        /// </summary>
        /// <param name="issues">Issues to update.</param>
        /// <param name="settings">Import settings.</param>
        private static void PopulateIssueDisplayName(DataTable issues, SiteImportSettings settings)
        {
            if (DataHelper.DataSourceIsEmpty(issues))
            {
                return;
            }

            foreach (DataRow dr in issues.Rows)
            {
                // Import process canceled
                if (settings.ProcessCanceled)
                {
                    ImportProvider.ImportCanceled();
                }

                // Prepare the data
                string subject = DataHelper.GetStringValue(dr, "IssueSubject");
                subject = TextHelper.LimitLength(subject, 200, wholeWords: true);

                dr["IssueDisplayName"] = subject;
            }
        }


        /// <summary>
        /// Sets the required default value for the <see cref="EmailTemplateInfo.TemplateInlineCSS" /> column.
        /// </summary>
        private static void SetEmailTemplateInlineCSSValue(object sender, ImportEventArgs e)
        {
            if (e.Settings.IsLowerVersion("11.0"))
            {
                var infoObj = e.Object;

                if (infoObj.TypeInfo.ObjectType == EmailTemplateInfo.OBJECT_TYPE)
                {
                    var emailTemplate = (EmailTemplateInfo) infoObj;
                    emailTemplate.TemplateInlineCSS = false;
                }
            }
        }


        private static void UpdateNewsletterIssueWithIncorrectlyNamedVariants(object sender, ImportGetDataEventArgs e)
        {
            if (e.ObjectType != NewsletterInfo.OBJECT_TYPE)
            {
                return;
            }

            var data = e.Data;

            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            var issues = data.Tables["Newsletter_NewsletterIssue"];

            new OriginalVariantNameCleaner(issues).Clear();
        }

        #endregion
    }


    /// <summary>
    /// Ensures that original issue variant has not set a name.
    /// </summary>
    internal class OriginalVariantNameCleaner
    {
        private readonly DataTable issues;


        /// <summary>
        /// Creates an instance of the <see cref="OriginalVariantNameCleaner"/> class.
        /// </summary>
        /// <param name="issues">Issues which need to clear the name of original variant.</param>
        public OriginalVariantNameCleaner(DataTable issues)
        {
            this.issues = issues;
        }


        /// <summary>
        /// Clears the name for issue variant representing the original variant.
        /// </summary>
        public void Clear()
        {
            if (DataHelper.DataSourceIsEmpty(issues))
            {
                return;
            }

            var variantsGroup = GetVariantsGroups();
            foreach (var variants in variantsGroup)
            {
                if (ExistsVariantWithoutName(variants))
                {
                    continue;
                }

                var variantNamedOriginal = GetVariantWithOriginalName(variants);
                if (variantNamedOriginal != null)
                {
                    variantNamedOriginal["IssueVariantName"] = DBNull.Value;
                    continue;
                }

                var first = GetFirstVariantWithNameSet(variants);
                if (first != null)
                {
                    first["IssueVariantName"] = DBNull.Value;
                }
            }
        }


        private IEnumerable<IGrouping<int, DataRow>> GetVariantsGroups()
        {
            return issues.AsEnumerable()
                         .Where(row => row.Field<bool?>("IssueIsABTest") ?? false)
                         .Where(row => row.Field<int?>("IssueVariantOfIssueID") != null)
                         .GroupBy(row => row.Field<int>("IssueVariantOfIssueID"), row => row);
        }


        private static DataRow GetFirstVariantWithNameSet(IGrouping<int, DataRow> variants)
        {
            return variants.FirstOrDefault(row => !string.IsNullOrEmpty(DataHelper.GetStringValue(row, "IssueVariantName")));
        }


        private static DataRow GetVariantWithOriginalName(IGrouping<int, DataRow> variants)
        {
            return variants.FirstOrDefault(variant => DataHelper.GetStringValue(variant, "IssueVariantName").Equals("Original", StringComparison.InvariantCulture));
        }


        private static bool ExistsVariantWithoutName(IGrouping<int, DataRow> variants)
        {
            return variants.Any(variant => string.IsNullOrEmpty(DataHelper.GetStringValue(variant, "IssueVariantName")));
        }
    }


}