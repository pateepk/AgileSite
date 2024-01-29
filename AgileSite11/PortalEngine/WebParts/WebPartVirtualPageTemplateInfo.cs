using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Web part virtual page template info
    /// </summary>
    public class WebPartVirtualPageTemplateInfo : PageTemplateInfo
    {
        /// <summary>
        /// Parent web part info
        /// </summary>
        public WebPartInfo WebPartInfo
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="webPartInfo">Web part info</param>
        public WebPartVirtualPageTemplateInfo(WebPartInfo webPartInfo)
        {
            WebPartInfo = webPartInfo;

            EnsureData();

            DisplayName = webPartInfo.WebPartDisplayName;
            CodeName = webPartInfo.WebPartName;
        }


        /// <summary>
        /// Inserts the data
        /// </summary>
        protected override void InsertData()
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Deletes the data
        /// </summary>
        protected override void DeleteData()
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Updates the data
        /// </summary>
        protected override void UpdateData()
        {
            // Update the parent web part instead of the template
            WebPartInfo.DefaultConfiguration = TemplateInstance;

            WebPartInfoProvider.SetWebPartInfo(WebPartInfo);
        }


        /// <summary>
        /// Indicates if the object is checked out by given user.
        /// </summary>
        protected override bool IsCheckedOutByUser(IUserInfo user)
        {
            return WebPartInfo.Generalized.IsCheckedOutByUser(user);
        }
    }
}
