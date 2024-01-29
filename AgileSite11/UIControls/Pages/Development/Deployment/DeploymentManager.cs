using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Compilation;
using System.Web.UI;

using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.PortalEngine;

namespace CMS.UIControls.Internal
{
    /// <summary>
    /// DeploymentManager contains deployment methods for virtual objects (Deployment, Source-control and Compilation test)
    /// </summary>
    /// <remarks>This class is not indented to be used in custom code.</remarks>
    public class DeploymentManager
    {
        #region "Events"

        /// <summary>
        /// Occurs when the info message should be displayed
        /// </summary>
        public event EventHandler<DeploymentManagerLogEventArgs> Log;

        /// <summary>
        /// Occurs when the some underlying action failed and error message should be displayed 
        /// </summary>
        public event EventHandler<DeploymentManagerLogEventArgs> Error;

        #endregion


        #region "Variables"

        private readonly List<string> deletePaths;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>This object is not indented to be used in custom code.</remarks>
        public DeploymentManager()
        {
            deletePaths = new List<string>();
        }

        #endregion


        #region "Log methods"

        private void AddLog(string message)
        {
            Log?.Invoke(this, new DeploymentManagerLogEventArgs
            {
                Message = message
            });
        }

        private void AddError(string message)
        {
            Error?.Invoke(this, new DeploymentManagerLogEventArgs
            {
                Message = message
            });
        }

        #endregion


        #region "Delete methods"

        private void DeleteDir(string path)
        {
            deletePaths?.Add(path);
        }

        private void DeleteDirs()
        {
            if (deletePaths != null)
            {
                foreach (var path in deletePaths)
                {
                    DirectoryInfo dir = DirectoryInfo.New(URLHelper.GetPhysicalPath(path));
                    if (dir.Exists)
                    {
                        DirectoryHelper.DeleteDirectory(dir.FullName, true);
                    }
                }
            }
        }

        #endregion


        #region "General Save methods (DB/FS)"

        private void SaveToExternalStorage(IEnumerable<BaseInfo> infos)
        {
            if (infos == null)
            {
                return;
            }

            foreach (BaseInfo info in infos)
            {
                AddLog(string.Format(ResHelper.GetString("Deployment.Deploying"), ResHelper.GetString("objecttype." + info.TypeInfo.ObjectType.Replace(".", "_")), info.Generalized.ObjectDisplayName));
                try
                {
                    info.Generalized.SaveExternalColumns();
                }
                catch (Exception ex)
                {
                    AddError(ex.Message);
                }
            }
        }


        private void SaveToDB(IEnumerable<BaseInfo> infos)
        {
            if (infos == null)
            {
                return;
            }

            foreach (BaseInfo info in infos)
            {
                AddLog(string.Format(ResHelper.GetString("Deployment.Deploying"), ResHelper.GetString("objecttype." + info.TypeInfo.ObjectType.Replace(".", "_")), info.Generalized.ObjectDisplayName));
                try
                {
                    info.Generalized.IgnoreExternalColumns = true;
                    info.Generalized.UpdateExternalColumns();
                    info.Generalized.IgnoreExternalColumns = false;
                }
                catch (Exception ex)
                {
                    AddError(ex.Message);
                }
            }
        }

        #endregion


        #region "Virtual object methods"

        #region "Layouts"

        private void ProcessLayouts(bool storeInDB, bool deleteFiles)
        {
            if (LayoutInfoProvider.StoreLayoutsInExternalStorage == !storeInDB)
            {
                return;
            }

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + LayoutInfo.OBJECT_TYPE.Replace(".", "_"))));
                }
                SaveToDB(LayoutInfoProvider.GetLayouts());
                if (deleteFiles)
                {
                    DeleteDir(LayoutInfoProvider.LayoutsDirectory);
                    LayoutInfoProvider.StoreLayoutsInExternalStorage = false;
                }
            }
            else
            {
                LayoutInfoProvider.StoreLayoutsInExternalStorage = true;
                SaveToExternalStorage(LayoutInfoProvider.GetLayouts());
            }
        }

        #endregion


        #region "Page templates"

        private void ProcessTemplates(bool storeInDB, bool deleteFiles)
        {
            if (PageTemplateInfoProvider.StorePageTemplatesInExternalStorage == !storeInDB)
            {
                return;
            }

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + PageTemplateInfo.OBJECT_TYPE.Replace(".", "_"))));
                }

                foreach (PageTemplateInfo info in PageTemplateInfoProvider.GetTemplates())
                {
                    ProcessTemplateToDB(info);
                }

                if (deleteFiles)
                {
                    DeleteDir("~/CMSVirtualFiles/Templates/");
                    PageTemplateInfoProvider.StorePageTemplatesInExternalStorage = false;
                }
            }
            else
            {
                PageTemplateInfoProvider.StorePageTemplatesInExternalStorage = true;

                foreach (PageTemplateInfo info in PageTemplateInfoProvider.GetTemplates())
                {
                    ProcessTemplateToFS(info);
                }
            }
        }


        private static void ProcessTemplateToFS(PageTemplateInfo info)
        {
            var devices = GetPageTemplateDeviceLayouts(info);
            if (devices != null)
            {
                foreach (PageTemplateDeviceLayoutInfo device in devices)
                {
                    device.Generalized.SaveExternalColumns();
                }
            }

            if (info.IsPortal || (info.PageTemplateType == PageTemplateTypeEnum.Dashboard) || (info.PageTemplateType == PageTemplateTypeEnum.UI))
            {
                info.Generalized.SaveExternalColumns();
            }
        }


        private static void ProcessTemplateToDB(PageTemplateInfo info)
        {
            var devices = GetPageTemplateDeviceLayouts(info);
            if (devices != null)
            {
                foreach (PageTemplateDeviceLayoutInfo device in devices)
                {
                    device.Generalized.IgnoreExternalColumns = true;
                    device.Generalized.UpdateExternalColumns();
                    device.Generalized.IgnoreExternalColumns = false;
                }
            }

            if (info.IsPortal || (info.PageTemplateType == PageTemplateTypeEnum.Dashboard) || (info.PageTemplateType == PageTemplateTypeEnum.UI))
            {
                info.Generalized.IgnoreExternalColumns = true;
                info.Generalized.UpdateExternalColumns();
                info.Generalized.IgnoreExternalColumns = false;
            }
        }


        private static ObjectQuery<PageTemplateDeviceLayoutInfo> GetPageTemplateDeviceLayouts(PageTemplateInfo info)
        {
            return PageTemplateDeviceLayoutInfoProvider.GetTemplateDeviceLayouts()
                                                       .WhereEquals("PageTemplateID", info.PageTemplateId);
        }

        #endregion


        #region "Transformations"

        private void ProcessTransformations(bool storeInDB, bool deleteFiles)
        {
            if (TransformationInfoProvider.StoreTransformationsInExternalStorage == !storeInDB)
            {
                return;
            }

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + TransformationInfo.OBJECT_TYPE.Replace(".", "_"))));
                }

                SaveToDB(TransformationInfoProvider.GetTransformations());

                if (deleteFiles)
                {
                    DeleteDir(TransformationInfoProvider.TransformationsDirectory);
                    TransformationInfoProvider.StoreTransformationsInExternalStorage = false;
                }
            }
            else
            {
                TransformationInfoProvider.StoreTransformationsInExternalStorage = true;
                SaveToExternalStorage(TransformationInfoProvider.GetTransformations());
            }
        }

        #endregion


        #region "WebPart layouts"

        private void ProcessWebpartLayouts(bool storeInDB, bool deleteFiles)
        {
            if (WebPartLayoutInfoProvider.StoreWebPartLayoutsInExternalStorage == !storeInDB)
            {
                return;
            }

            var webPartLayouts = WebPartLayoutInfoProvider.GetWebPartLayouts();

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + WebPartLayoutInfo.OBJECT_TYPE.Replace(".", "_"))));
                }
                SaveToDB(webPartLayouts);
                if (deleteFiles)
                {
                    DeleteDir(WebPartLayoutInfoProvider.WebPartLayoutsDirectory);
                    WebPartLayoutInfoProvider.StoreWebPartLayoutsInExternalStorage = false;
                }
            }
            else
            {
                WebPartLayoutInfoProvider.StoreWebPartLayoutsInExternalStorage = true;
                SaveToExternalStorage(webPartLayouts);
            }
        }

        #endregion


        #region "AlternativeFormLayouts"

        private void ProcessAltFormLayouts(bool storeInDB, bool deleteFiles)
        {
            if (AlternativeFormInfoProvider.StoreAlternativeFormsInExternalStorage == !storeInDB)
            {
                return;
            }

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + AlternativeFormInfo.OBJECT_TYPE.Replace(".", "_"))));
                }

                SaveToDB(AlternativeFormInfoProvider.GetAlternativeForms());

                if (deleteFiles)
                {
                    DeleteDir(AlternativeFormInfoProvider.FormLayoutsDirectory);
                    AlternativeFormInfoProvider.StoreAlternativeFormsInExternalStorage = false;
                }
            }
            else
            {
                AlternativeFormInfoProvider.StoreAlternativeFormsInExternalStorage = true;
                SaveToExternalStorage(AlternativeFormInfoProvider.GetAlternativeForms());
            }
        }

        #endregion


        #region "Form layouts"

        private void ProcessFormLayouts(bool storeInDB, bool deleteFiles)
        {
            if (DataClassInfoProvider.StoreFormLayoutsInExternalStorage == !storeInDB)
            {
                return;
            }

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + DataClassInfo.OBJECT_TYPE.Replace(".", "_"))));
                }

                SaveToDB(DataClassInfoProvider.GetClasses());

                if (deleteFiles)
                {
                    DeleteDir(DataClassInfoProvider.FormLayoutsDirectory);
                    DataClassInfoProvider.StoreFormLayoutsInExternalStorage = false;
                }
            }
            else
            {
                DataClassInfoProvider.StoreFormLayoutsInExternalStorage = true;
                SaveToExternalStorage(DataClassInfoProvider.GetClasses());
            }
        }

        #endregion


        #region "Web part containers"

        private void ProcessWebpartContainers(bool storeInDB, bool deleteFiles)
        {
            if (WebPartContainerInfoProvider.StoreWebPartContainersInExternalStorage == !storeInDB)
            {
                return;
            }

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + WebPartContainerInfo.OBJECT_TYPE.Replace(".", "_"))));
                }
                
                SaveToDB(WebPartContainerInfoProvider.GetContainers());

                if (deleteFiles)
                {
                    DeleteDir(WebPartContainerInfoProvider.WebPartContainersDirectory);
                    WebPartContainerInfoProvider.StoreWebPartContainersInExternalStorage = false;
                }
            }
            else
            {
                WebPartContainerInfoProvider.StoreWebPartContainersInExternalStorage = true;
                SaveToExternalStorage(WebPartContainerInfoProvider.GetContainers());
            }
        }

        #endregion


        #region "StyleSheets"

        private void ProcessCSS(bool storeInDB, bool deleteFiles)
        {
            if (CssStylesheetInfoProvider.StoreCSSStyleSheetsInExternalStorage == !storeInDB)
            {
                return;
            }

            if (storeInDB)
            {
                if (deleteFiles)
                {
                    AddLog(string.Format(ResHelper.GetString("Deployment.DeletingDeployedFiles"), ResHelper.GetString("objecttype." + CssStylesheetInfo.OBJECT_TYPE.Replace(".", "_"))));
                }

                SaveToDB(CssStylesheetInfoProvider.GetCssStylesheets());

                if (deleteFiles)
                {
                    DeleteDir(CssStylesheetInfoProvider.CSSStylesheetsDirectory);
                    CssStylesheetInfoProvider.StoreCSSStyleSheetsInExternalStorage = false;
                }
            }
            else
            {
                CssStylesheetInfoProvider.StoreCSSStyleSheetsInExternalStorage = true;
                SaveToExternalStorage(CssStylesheetInfoProvider.GetCssStylesheets());
            }
        }

        #endregion

        #endregion


        #region "Deployment"

        /// <summary>
        /// Does the deployment of the given object
        /// </summary>
        /// <param name="parameters">Deployment parameters</param>
        public void Deploy(DeploymentParameters parameters)
        {
            bool targetMode = SettingsKeyInfoProvider.DeploymentMode;

            // In the direction DB -> FS we need to set the deployment mode before processing
            if (!targetMode)
            {
                SettingsKeyInfoProvider.DeploymentMode = true;
            }

            ProcessLayouts(targetMode, true);
            ProcessTemplates(targetMode, true);
            ProcessTransformations(targetMode, true);
            ProcessWebpartLayouts(targetMode, true);
            ProcessAltFormLayouts(targetMode, true);
            ProcessFormLayouts(targetMode, true);

            // In the direction FS -> DB we need to set the deployment mode after processing
            if (targetMode)
            {
                SettingsKeyInfoProvider.DeploymentMode = false;
            }

            // Delete dirs at the end because of restart
            DeleteDirs();
        }

        #endregion


        #region "Source control mode"

        /// <summary>
        /// Does the deployment of the given object
        /// </summary>
        /// <param name="parameters">Deployment parameters</param>
        public void SaveExternally(DeploymentParameters parameters)
        {
            if (!SettingsKeyInfoProvider.DeploymentMode)
            {
                ProcessLayouts(!parameters.SaveLayout, true);
                ProcessTemplates(!parameters.SavePageTemplate, true);
                ProcessTransformations(!parameters.SaveTransformation, true);
                ProcessWebpartLayouts(!parameters.SaveWebPartLayout, true);
                ProcessAltFormLayouts(!parameters.SaveAlternativeFormLayout, true);
                ProcessFormLayouts(!parameters.SaveFormLayout, true);
            }

            ProcessWebpartContainers(!parameters.SaveWebPartContainer, true);
            ProcessCSS(!parameters.SaveCss, true);

            // Delete dirs at the end because of restart
            DeleteDirs();

        }

        #endregion


        #region "Synchronize"

        /// <summary>
        /// Synchronize virtual object in source control mode
        /// </summary>
        /// <param name="parameters">Deployment parameters</param>
        public void Synchronize(DeploymentParameters parameters)
        {
            if (parameters.SaveLayout)
            {
                ProcessLayouts(true, false);
            }

            if (parameters.SavePageTemplate)
            {
                ProcessTemplates(true, false);
            }

            if (parameters.SaveTransformation)
            {
                ProcessTransformations(true, false);
            }

            if (parameters.SaveWebPartLayout)
            {
                ProcessWebpartLayouts(true, false);
            }

            if (parameters.SaveWebPartContainer)
            {
                ProcessWebpartContainers(true, false);
            }

            if (parameters.SaveCss)
            {
                ProcessCSS(true, false);
            }

            if (parameters.SaveAlternativeFormLayout)
            {
                ProcessAltFormLayouts(true, false);
            }

            if (parameters.SaveFormLayout)
            {
                ProcessFormLayouts(true, false);
            }
        }


        #endregion


        #region "Virtual file compilation"

        /// <summary>
        /// Does the test of the given object
        /// </summary>
        /// <param name="parameters">Deployment parameters</param>
        public void CompileVirtualObjects(DeploymentParameters parameters)
        {
            TestCompilation(TransformationInfoProvider.GetTransformations());
            TestCompilation(AlternativeFormInfoProvider.GetAlternativeForms());
            TestCompilation(WebPartLayoutInfoProvider.GetWebPartLayouts());
            TestCompilation(LayoutInfoProvider.GetLayouts());
            TestPageTemplatesCompilation();
            TestCompilation(DataClassInfoProvider.GetClasses());
        }



        /// <summary>
        /// Tests compilation of the given list of objects
        /// </summary>
        /// <param name="infos">Collection of objects</param>
        private void TestCompilation(IEnumerable<BaseInfo> infos)
        {
            if (infos == null)
            {
                return;
            }

            foreach (BaseInfo info in infos)
            {
                var name = info.Generalized.ObjectFullName ?? info.Generalized.ObjectDisplayName;

                if (SystemContext.DevelopmentMode && (info.Generalized.ObjectCodeName.StartsWith("test", StringComparison.InvariantCultureIgnoreCase) || name.StartsWith("test", StringComparison.InvariantCultureIgnoreCase)))
                {
                    // Skip testing objects
                    continue;
                }

                AddLog(string.Format(ResHelper.GetString("Deployment.Testing"), ResHelper.GetString("objecttype." + info.TypeInfo.ObjectType.Replace(".", "_")), name));

                try
                {
                    var path = info.Generalized.GetVirtualFileRelativePath(info.TypeInfo.CodeColumn, info.Generalized.ObjectVersionGUID.ToString());
                    if (path.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
                    {
                        var c = (Control)BuildManager.CreateInstanceFromVirtualPath(path, typeof(UserControl));
                        c.Dispose();
                    }
                }
                catch (ThreadAbortException)
                {
                    // Cancel                
                }
                catch (Exception ex)
                {
                    string message = string.Format(ResHelper.GetString("Deployment.TestingFailed"), ResHelper.GetString("objecttype." + info.TypeInfo.ObjectType.Replace(".", "_")), name, ex.Message);

                    AddError(message);
                }
            }
        }


        /// <summary>
        /// Proceeds compilation of page templates.
        /// </summary>
        private void TestPageTemplatesCompilation()
        {
            // Test page templates compilation -- filter ASPX, ASPX+Portal and MVC templates
            var filteredPageTemplates = PageTemplateInfoProvider.GetTemplates()
                                            .WhereNotIn("PageTemplateType", new []
                                            {
                                                PageTemplateTypeEnum.Aspx.ToStringRepresentation(),
                                                PageTemplateTypeEnum.AspxPortal.ToStringRepresentation(),
                                                PageTemplateTypeEnum.MVC.ToStringRepresentation()
                                            });

            TestCompilation(filteredPageTemplates);
        }

        #endregion
    }
}
