namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;

    public class BasePage : System.Web.UI.Page
    {

        protected UserManager user = null;
        protected RequestManager request = null;

        protected void Page_PreInit(object sender, EventArgs e)
        {

            AppManager.PagePreInit(out user, out request);

            if (!user.isUserAdministrator)
            {
                string gamenotatepage = Utils.getScriptName().ToLower();
            }
        }

        public BasePage()
        {

        }


        /// <summary>
        ///  This function is called when request finish (page unload)
        /// </summary>
        protected void Page_Unload(object sender, EventArgs e)
        {
            AppManager.PageEnd(this);
        }

    }

}
