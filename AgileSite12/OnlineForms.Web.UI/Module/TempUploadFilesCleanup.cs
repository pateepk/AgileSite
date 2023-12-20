using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.FormEngine;
using CMS.IO;

namespace CMS.OnlineForms.Web.UI
{
    internal static class TempUploadFilesCleanup
    {
        private static IBizFormFileService mBizFormFileService;


        private static IBizFormFileService BizFormFileService
        {
            get
            {
                return mBizFormFileService ?? (mBizFormFileService = Service.Resolve<IBizFormFileService>());
            }
        }


        public static void RegisterUploadedFilesCleanupAction()
        {
            TemporaryUploadsCleaner.CleanupActions.Add(new CleanupAction
            {
                FolderPath = FormHelper.GetBizFormTempFilesFolderPath(),
                DeleteFileCallback = DeleteTempFile
            });
        }


        private static bool DeleteTempFile(string fileFolderPath, string fileName)
        {
            var tempFileIdentifier = Path.GetFileNameWithoutExtension(fileName);

            BizFormFileService.DeleteTempFile(tempFileIdentifier, fileFolderPath);

            return false;
        }
    }
}
