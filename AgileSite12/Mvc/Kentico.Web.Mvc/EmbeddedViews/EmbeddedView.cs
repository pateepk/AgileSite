using System;
using System.IO;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Kentico.Web.Mvc
{
    internal class EmbeddedView : IView
    {
        internal string VirtualPath
        {
            get;
        }


        internal Type ViewType
        {
            get;
        }


        public EmbeddedView(string path, Type type)
        {
            VirtualPath = path;
            ViewType = type;
        }


#pragma warning disable BH1014 // Do not use System.IO
        public void Render(ViewContext viewContext, TextWriter writer)
#pragma warning restore BH1014 // Do not use System.IO
        {
            var webViewPage = Activator.CreateInstance(ViewType) as WebViewPage;

            webViewPage.VirtualPath = VirtualPath;
            webViewPage.ViewContext = viewContext;
            webViewPage.ViewData = viewContext.ViewData;
            webViewPage.InitHelpers();

            // Explicitly enable client validation to ensure form validation messages in system forms (such as widget properties).
            webViewPage.Html.EnableClientValidation();

            var pageContext = new WebPageContext(viewContext.HttpContext, webViewPage, null);
            webViewPage.ExecutePageHierarchy(pageContext, writer, null);
        }
    }
}
