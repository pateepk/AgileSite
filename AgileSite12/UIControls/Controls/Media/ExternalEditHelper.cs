using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.UIControls
{
    using RegisteredControlList = List<RegisteredExternalEditControl>;

    /// <summary>
    /// Helper methods for editing of items in external application
    /// </summary>
    public static class ExternalEditHelper
    {
        /// <summary>
        /// Collection of registered controls
        /// </summary>
        private static readonly SafeDictionary<FileTypeEnum, RegisteredControlList> mControls = new SafeDictionary<FileTypeEnum, RegisteredControlList>();


        /// <summary>
        /// Collection of registered scripts
        /// </summary>
        private static readonly List<string> mScripts = new List<string>();



        /// <summary>
        /// Registers the given control for editing of items in external application
        /// </summary>
        /// <param name="type">Source type</param>
        /// <param name="control">Control definition. Control path or control type.</param>
        /// <param name="condition">Control condition. Condition(extension, siteName)</param>
        public static void RegisterControl(FileTypeEnum type, ControlDefinition control, Func<string, string, bool> condition)
        {
            var controls = mControls[type];
            if (controls == null)
            {
                controls = new RegisteredControlList();
                mControls[type] = controls;
            }

            controls.Add(new RegisteredExternalEditControl(control, condition));
        }


        /// <summary>
        /// Registers the given script file for editing of items in external application
        /// </summary>
        /// <param name="path">Script path to register</param>
        public static void RegisterScript(string path)
        {
            mScripts.Add(path);
        }


        /// <summary>
        /// Registers the given script file for editing of items in external application
        /// </summary>
        /// <param name="element">Page element</param>
        public static void RenderScripts(PageElement element)
        {
            foreach (var script in mScripts)
            {
                ScriptHelper.RegisterScriptFile(element, script);
            }
        }


        /// <summary>
        /// Loads the control to the given control.
        /// </summary>
        /// <param name="ctrl">Control to load into</param>
        /// <param name="type">Source type</param>
        /// <param name="siteName">Site name</param>
        /// <param name="data">Media data</param>
        /// <param name="isLiveSite">If true, the control is displayed on live site</param>
        /// <param name="node">Attachment parent document</param>
        /// <param name="clearControls">If true, the controls collection of the target control is cleared</param>
        public static ExternalEditControl LoadExternalEditControl(Control ctrl, FileTypeEnum type, string siteName, IDataContainer data, bool isLiveSite, TreeNode node = null, bool clearControls = false)
        {
            if (ctrl != null)
            {
                // Get the extension from the data item
                string extensionField = null;

                switch (type)
                {
                    case FileTypeEnum.Attachment:
                        extensionField = "AttachmentExtension";
                        break;

                    case FileTypeEnum.MediaFile:
                        extensionField = "FileExtension";
                        break;
                        
                    case FileTypeEnum.MetaFile:
                        extensionField = "MetaFileExtension";
                        break;
                }

                string extension = ValidationHelper.GetString(data.GetValue(extensionField), "");
                
                // Get the control path
                var control = GetEditControl(type, extension, siteName);
                if (control == null)
                {
                    return null;
                }

                // Dynamically load control
                var ctrlEdit = control.Load(ctrl.Page) as ExternalEditControl;
                if (ctrlEdit != null)
                {
                    ctrl.Visible = true;
                    ctrlEdit.Visible = false;

                    // Set correct current site name
                    ctrlEdit.SiteName = siteName;

                    if (clearControls)
                    {
                        ctrl.Controls.Clear();
                    }
                    ctrl.Controls.Add(ctrlEdit);

                    // Initialize with the data
                    ctrlEdit.InitFrom(data, isLiveSite, node);
                }

                return ctrlEdit;
            }

            return null;
        }


        /// <summary>
        /// Gets the editing control path based on type
        /// </summary>
        /// <param name="type">Editing control type</param>
        /// <param name="extension">Item extension</param>
        /// <param name="siteName">Site name</param>
        private static ControlDefinition GetEditControl(FileTypeEnum type, string extension, string siteName)
        {
            // Get the list of registered controls
            var controls = mControls[type];
            if (controls == null)
            {
                return null;
            }

            // Process all controls
            foreach (var ctrl in controls)
            {
                // If control condition matches, return the control
                if ((ctrl.Condition == null) || ctrl.Condition(extension, siteName))
                {
                    return ctrl.Control;
                }
            }

            return null;
        }
    }
}
