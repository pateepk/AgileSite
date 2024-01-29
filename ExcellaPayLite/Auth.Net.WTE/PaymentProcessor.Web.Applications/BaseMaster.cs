namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;

    public partial class BaseMaster : MasterPage
    {

        protected UserManager user = null;
        protected RequestManager request = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (AppManager.userManager == null)
            {
                AppManager.loadUserManager();
            }

            user = AppManager.userManager;
            request = AppManager.requestManager;
            user.AuthorizeUser();
        }

        private string _title = string.Empty;         // title of the page. used to put title in the page
        private string _css = string.Empty;           // hold css file names separated by commas, used to build header
        private string _javascript = string.Empty;    // hold javascript file names separated by commas, used to build header
        private string _jsonload = string.Empty;      // hold all function of javascript need to be executed on bodyload


        // public method for outside to set the title of the page
        public void setTitle(string title)
        {
            _title = title;
        }

        // make it private
        // if you want to add from other class, use Utils.addCSS
        private void addCSS(string cssFile)
        {
            _css += "<link href=\"" + cssFile + "\" rel=\"stylesheet\" type=\"text/css\" /> \r\n";
        }

        // make it private
        // if you want to add from other class, use Utils.addJavascript
        private void addJavascript(string jsFile)
        {
            _javascript += "<script type=\"text/javascript\" src=\"" + jsFile + "\"></script> \r\n";
        }

        // public method to pust a function that will be executed on body onload of javascript
        public void addBodyOnload(string jsFile)
        {
            if (jsFile.EndsWith(";"))
            {
                jsFile += ";";
            }
            _jsonload += jsFile;
        }

        /// <summary>
        /// Render the header! Put Title, CSS include, Javascript include, and JS load on body
        /// </summary>
        private void headerRender()
        {
            this.Page.Title = _title;
            string htmlHeader = string.Empty;

            if (_css.Length > 0)
            {
                htmlHeader += _css;
            }

            if (_javascript.Length > 0)
            {
                htmlHeader += _javascript;
            }

        }

        private void formRender()
        {
        }

        private void loadJSFromControls()
        {
        }

        private void loadCSSFromControls()
        {
        }

        public BaseMaster() // constructor of BasePage
        {

        }

        /// <summary>
        ///  This function is called when request finish (page unload)
        /// </summary>
        protected void Page_Unload(object sender, EventArgs e)
        {
            AppManager.PageEnd(this);
        }


        /// <summary>
        /// Finding Generic Tag on the page, if not found, return new one for dummy
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private HtmlGenericControl GenericTag(string key)
        {
            HtmlGenericControl C = (HtmlGenericControl)this.FindControl(key);

            if (C == null)
            {
                HtmlGenericControl X = new HtmlGenericControl();
                return X;
            }
            return C;
        }

        /// <summary>
        /// Finding PlaceHolder Tag on the page, if not found, return new one for dummy
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private PlaceHolder PlaceHolderTag(string key)
        {
            PlaceHolder P = (PlaceHolder)this.FindControl(key);
            if (P == null)
            {
                PlaceHolder X = new PlaceHolder();
                return X;
            }
            return P;
        }

    }

}
