using System;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Loads the <see cref="FormEngineUserControl"/>  based on <see cref="FormUserControlInfo"/> values.
    /// </summary>
    public static class FormUserControlLoader
    {
        /// <summary>
        /// Loads the given form control from its definition.
        /// </summary>
        /// <param name="page">Page where the form control should be loaded</param>
        /// <param name="controlName">Form control code name</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="form">Form</param>
        /// <param name="loadDefaultProperties">If true, the default properties for the control are set</param>
        /// <exception cref="InvalidOperationException">Thrown when control not found.</exception>
        public static FormEngineUserControl LoadFormControl(Page page, string controlName, string fieldName, BasicForm form = null, bool loadDefaultProperties = true)
        {
            var ci = FormUserControlInfoProvider.GetFormUserControlInfo(controlName);
            if (ci == null)
            {
                throw new InvalidOperationException($"Form control '{controlName}' not found.");
            }

            FormEngineUserControl ctrl;

            // Inherited form control
            if (ci.UserControlParentID > 0)
            {
                var parentFormControl = FormUserControlInfoProvider.GetFormUserControlInfo(ci.UserControlParentID);

                ctrl = LoadUserControlInfo(page, parentFormControl);
                LoadExtender(ci, ctrl);
            }
            // Web or assembly based form user control
            else
            {
                ctrl = LoadUserControlInfo(page, ci);
            }

            var properties = FormHelper.GetFormControlParameters(controlName, ci.UserControlMergedParameters, false);
            SetupFormControl(fieldName, form, loadDefaultProperties, properties, ctrl);

            return ctrl;
        }


        /// <summary>
        /// Loads the user control from ASCX file or from assembly.
        /// </summary>
        private static FormEngineUserControl LoadUserControlInfo(Page page, FormUserControlInfo ci)
        {
            FormEngineUserControl ctrl;

            if (!String.IsNullOrEmpty(ci.UserControlFileName))
            {
                ctrl = LoadAscxControl(page, ci);
            }
            else
            {
                ctrl = ClassHelper.GetClass(ci.UserControlAssemblyName, ci.UserControlClassName) as FormEngineUserControl;
            }

            return ctrl;
        }


        /// <summary>
        /// Loads the extender if defined.
        /// </summary>
        private static void LoadExtender(FormUserControlInfo ci, FormEngineUserControl ctrl)
        {
            if (!String.IsNullOrEmpty(ci.UserControlAssemblyName))
            {
                ControlsHelper.LoadExtender(ci.UserControlAssemblyName, ci.UserControlClassName, ctrl);
            }
        }


        /// <summary>
        /// Loads ASCX-based user control.
        /// </summary>
        private static FormEngineUserControl LoadAscxControl(Page page, FormUserControlInfo ci)
        {
            var url = GetFormControlUrl(ci);
            return page.LoadUserControl(url) as FormEngineUserControl;
        }


        /// <summary>
        /// Sets the default <see cref="FormUserControlInfo"/> properties based on current settings.
        /// </summary>
        private static void SetupFormControl(string fieldName, BasicForm form, bool loadDefaultProperties, FormInfo properties, FormEngineUserControl ctrl)
        {
            if (ctrl != null)
            {
                ctrl.Form = form;
                ctrl.DefaultProperties = properties;

                if (loadDefaultProperties)
                {
                    ctrl.LoadDefaultProperties();
                }

                ctrl.ID = "fc" + fieldName;
                ctrl.Field = fieldName;
            }
        }


        /// <summary>
        /// Gets the ASCX based form control URL.
        /// </summary>
        private static string GetFormControlUrl(FormUserControlInfo controlInfo)
        {
            string url = controlInfo.UserControlFileName;
            if (url.StartsWith("~/", StringComparison.Ordinal))
            {
                return SystemContext.ApplicationPath.TrimEnd('/') + url.TrimStart('~');
            }

            return FormUserControlInfoProvider.FormUserControlsDirectory + "/" + url;
        }
    }
}
