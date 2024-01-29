using System;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class for registering video link elements that show YouTube video in Magnific Popup lightbox.
    /// </summary>
    public class MagnificPopupYouTubeJavaScriptRegistrator
    {
        private const string SCRIPT =
@"cmsrequire(['CMS.OnlineMarketing/MagnificPopup/MagnificPopup', 'jQuery'], function (MagnificPopup, $) {{ 
    MagnificPopup.register('#{0}', {{ type: 'iframe', tClose: '{1}', tLoading: '{2}' }});
    // Register the popup when new elements are added to the DOM, dont worry about multiple initialization because the magnifier checks that
    $(document).on('DOMNodeInserted', function(e) {{
        var target = e.target;
        // Try find element only when target has some children
        if((target.childNodes.length > 0) && $(target).find('#{0}').length){{
            MagnificPopup.register('#{0}', {{ type: 'iframe', tClose: '{1}', tLoading: '{2}' }});
        }}
    }});
}});";
        

        /// <summary>
        /// Registers element with given <paramref name="elementID"/> to the Magnific popup module.
        /// </summary>
        /// <param name="page">Page that link will be used on</param>
        /// <param name="elementID">Unique client ID of the registered element</param>
        /// <exception cref="ArgumentException"><paramref name="elementID"/> is null or empty</exception>
        /// <exception cref="ArgumentNullException"><paramref name="page"/> is null</exception>
        public void RegisterMagnificPopupElement(Page page, string elementID)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }
            if (string.IsNullOrEmpty(elementID))
            {
                throw new ArgumentException("[MagnificPopupYouTubeLinkBuilder.RegisterMagnificPopupElement]: Cannot be empty or null", "elementID");
            }
            ScriptHelper.RegisterStartupScript(page, typeof(string), elementID, string.Format(SCRIPT, elementID, ResHelper.GetString("general.close"), ResHelper.GetString("general.loading")), true);
        }
    }
}
