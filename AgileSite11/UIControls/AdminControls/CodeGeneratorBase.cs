using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for controls providing code generation functionality.
    /// </summary>
    public abstract class CodeGeneratorBase : CMSAdminControl
    {
        /// <summary>
        /// <para>
        /// Saves the generated code of <paramref name="dataClasses"/> to a directory identified by <paramref name="path"/>.
        /// Shows a message and logs to event log in case of an error.
        /// </para>
        /// <para>
        /// Does nothing on a precompiled website.
        /// </para>
        /// </summary>
        /// <param name="path">Directory where the code files are to be saved.</param>
        /// <param name="dataClasses">Enumeration od data classes for which to generate code files.</param>
        protected void SaveCode(string path, IEnumerable<DataClassInfo> dataClasses)
        {
            if (SystemContext.IsPrecompiledWebsite)
            {
                return;
            }

            try
            {
                var baseFolderPath = URLHelper.GetPhysicalPath(path);

                ContentItemCodeFileGenerator.Internal.GenerateFiles(dataClasses, baseFolderPath);

                var message = GetString("classes.code.filessavesuccess");
                ShowConfirmation(message);
            }
            catch (Exception exception)
            {
                CoreServices.EventLog.LogException("Content item code generator", "Save", exception);

                var message = GetString("classes.code.filessaveerror");
                ShowError(message);
            }
        }


        /// <summary>
        /// Shows a message explaining the inability to save the code on a precompiled website.
        /// </summary>
        protected void ShowCodeSaveDisabledMessage()
        {
            var message = GetString("classes.code.codesaveerror");
            ShowInformation(message);
        }
    }
}
