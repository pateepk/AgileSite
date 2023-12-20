using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;


namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class providing <see cref="ABVariantDataInfo"/> management.
    /// </summary>
    public class ABVariantDataInfoProvider : AbstractInfoProvider<ABVariantDataInfo, ABVariantDataInfoProvider>
    {
        /// <summary>
        /// Returns <see cref="ABVariantDataInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ABVariantDataInfo"/> ID.</param>
        public static ABVariantDataInfo GetABVariantDataInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns a query for all the A/B variants data.
        /// </summary>
        public static ObjectQuery<ABVariantDataInfo> GetVariantsData()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="ABVariantDataInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ABVariantDataInfo"/> to be set.</param>
        public static void SetABVariantDataInfo(ABVariantDataInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="ABVariantDataInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ABVariantDataInfo"/> to be deleted.</param>
        public static void DeleteABVariantDataInfo(ABVariantDataInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="ABVariantDataInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ABVariantDataInfo"/> ID.</param>
        public static void DeleteABVariantDataInfo(int id)
        {
            ABVariantDataInfo infoObj = GetABVariantDataInfo(id);
            DeleteABVariantDataInfo(infoObj);
        }


        /// <summary>
        /// Deletes the object from the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ABVariantDataInfo info)
        {
            if (info == null)
            {
                return;
            }

            base.DeleteInfo(info);

            ABVariantColorAssigner.RemoveVariantData(info);
        }


        internal static void MaterializeVariants(ABTestInfo abTest, IList<IABTestVariant> variants)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException(nameof(abTest));
            }

            if (variants == null)
            {
                throw new ArgumentNullException(nameof(variants));
            }

            var site = SiteInfoProvider.GetSiteInfo(abTest.ABTestSiteID);
            if (!site.SiteIsContentOnly)
            {
                throw new InvalidOperationException("Cannot materialize A/B variants on non-ContentOnly site");
            }

            if (variants.Count > 0 && variants.Count(x => x.IsOriginal) != 1)
            {
                throw new InvalidOperationException("Cannot materialize A/B variants without original variant specified or with two or more variants marked as original.");
            }

            ABVariantColorAssigner.RemoveTest(abTest);

            var parameters = new QueryDataParameters();
            parameters.Add("@ABTestID", abTest.ABTestID);
            parameters.Add("@VariantsTable", BuildABVariantDataTable(variants), typeof(DataTable));

            ConnectionHelper.ExecuteQuery("Proc_OM_UpsertABVariantData", parameters, QueryTypeEnum.StoredProcedure);
           
            ProviderObject.ClearHashtables(true);
            CacheHelper.TouchKey(ABVariantDataInfo.OBJECT_TYPE + "|all");
        }


        private static DataTable BuildABVariantDataTable(IEnumerable<IABTestVariant> variants)
        {
            DataTable dt = new DataTable("Type_OM_ABVariantDataTable");
            dt.Columns.Add("ABVariantGUID", typeof(Guid));
            dt.Columns.Add("ABVariantDisplayName", typeof(string));
            dt.Columns.Add("ABVariantIsOriginal", typeof(bool));

            foreach (var variant in variants.OrderBy(x => x.Guid))
            {
                dt.Rows.Add(variant.Guid, variant.Name, variant.IsOriginal);
            }

            return dt;
        }
    }
}