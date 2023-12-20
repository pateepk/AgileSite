using System.Collections;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.CMSImportExport
{
    using TypedDataSet = InfoDataSet<WebTemplateInfo>;

    /// <summary>
    /// Provides access to information about web template.
    /// </summary>
    public class WebTemplateInfoProvider : AbstractInfoProvider<WebTemplateInfo, WebTemplateInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Sets specified web template data.
        /// </summary>
        /// <param name="webTemplate">Web template object</param>
        public static void SetWebTemplateInfo(WebTemplateInfo webTemplate)
        {
            ProviderObject.SetInfo(webTemplate);
        }


        /// <summary>
        /// Gets the web template.
        /// </summary>
        /// <param name="webTemplateId">Web template ID</param>
        public static WebTemplateInfo GetWebTemplateInfo(int webTemplateId)
        {
            return ProviderObject.GetInfoById(webTemplateId);
        }


        /// <summary>
        /// Gets the web template.
        /// </summary>
        /// <param name="templateName">Template code name</param>
        public static WebTemplateInfo GetWebTemplateInfo(string templateName)
        {
            return ProviderObject.GetInfoByCodeName(templateName);
        }


        /// <summary>
        /// Deletes specified web template.
        /// </summary>
        /// <param name="template">Web template object</param>
        public static void DeleteWebTemplateInfo(WebTemplateInfo template)
        {
            ProviderObject.DeleteInfo(template);
        }


        /// <summary>
        /// Deletes specified web template.
        /// </summary>
        /// <param name="webTemplateId">Id of web template to delete</param>
        public static void DeleteWebTemplateInfo(int webTemplateId)
        {
            WebTemplateInfo wti = GetWebTemplateInfo(webTemplateId);
            DeleteWebTemplateInfo(wti);
        }


        /// <summary>
        /// Returns all web templates.
        /// </summary>
        public static ObjectQuery<WebTemplateInfo> GetWebTemplates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets dataset with all web templates.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="onlyExisting">Return only templates that exist physically on the disc</param>
        /// <param name="topN">Select top N rows</param>
        public static TypedDataSet GetWebTemplates(string where, string orderBy, int topN = -1, string columns = null, bool onlyExisting = false)
        {
            return ProviderObject.GetWebTemplatesInternal(where, orderBy, topN, columns, onlyExisting);
        }


        /// <summary>
        /// Moves the template up in order.
        /// </summary>
        /// <param name="webTemplateId">Web template ID</param>
        public static void MoveTemplateUp(int webTemplateId)
        {
            ProviderObject.MoveTemplateUpInternal(webTemplateId);
        }


        /// <summary>
        /// Moves the template down in order.
        /// </summary>
        /// <param name="webTemplateId">Web template ID</param>
        public static void MoveTemplateDown(int webTemplateId)
        {
            ProviderObject.MoveTemplateDownInternal(webTemplateId);
        }


        /// <summary>
        /// Returns true if any template is physically present.
        /// </summary>
        public static bool IsAnyTemplatePresent()
        {
            return ProviderObject.IsAnyTemplatePresentInternal();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Indicates if any template is physically present.
        /// </summary>
        protected virtual bool IsAnyTemplatePresentInternal()
        {
            if (CMSHttpContext.Current != null)
            {
                // Get all web templates
                DataSet ds = GetWebTemplates().Column("WebTemplateFileName");

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string dir = ValidationHelper.GetString(dr["WebTemplateFileName"], string.Empty);
                        if ((dir != string.Empty) && (Directory.Exists(CMSHttpContext.Current.Server.MapPath(dir))))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Gets dataset with all web templates.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="topN">Get top N columns</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="onlyExisting">Indicates if only web templates with existing folder should be selected</param>
        protected virtual TypedDataSet GetWebTemplatesInternal(string where, string orderBy, int topN, string columns, bool onlyExisting)
        {
            TypedDataSet ds = GetWebTemplates().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;

            if (onlyExisting)
            {
                if (CMSHttpContext.Current != null)
                {
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        DataTable table = ds.Tables[0];
                        // Get templates to remove
                        ArrayList templatesToRemove = new ArrayList();
                        foreach (DataRow dr in table.Rows)
                        {
                            string dir = ValidationHelper.GetString(dr["WebTemplateFileName"], string.Empty);
                            if ((dir == string.Empty) || (!FileHelper.DirectoryExists(dir)))
                            {
                                templatesToRemove.Add(dr);
                            }
                        }

                        // Remove templates from data set
                        foreach (DataRow template in templatesToRemove)
                        {
                            table.Rows.Remove(template);
                        }
                    }
                }
            }

            return ds;
        }


        /// <summary>
        /// Moves the template up in order.
        /// </summary>
        /// <param name="webTemplateId">Web template ID</param>
        protected virtual void MoveTemplateUpInternal(int webTemplateId)
        {
            if (webTemplateId > 0)
            {
                var infoObj = GetInfoById(webTemplateId);
                if (infoObj != null)
                {
                    infoObj.Generalized.MoveObjectUp();
                }
            }
        }


        /// <summary>
        /// Moves the template down in order.
        /// </summary>
        /// <param name="webTemplateId">Web template ID</param>
        protected virtual void MoveTemplateDownInternal(int webTemplateId)
        {
            if (webTemplateId > 0)
            {
                var infoObj = GetInfoById(webTemplateId);
                if (infoObj != null)
                {
                    infoObj.Generalized.MoveObjectDown();
                }
            }
        }

        #endregion
    }
}