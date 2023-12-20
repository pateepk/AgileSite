using System;

using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.OnlineMarketing.Web.UI.Internal
{
    /// <summary>
    /// Url input for selecting page url in A/B testing application.
    /// </summary>
    public sealed class ABTestingUrlInput : FormEngineUserControl
    {
        private CMSTextArea mTextArea;
        private string mRelativeUrl;


        private string Url
        {
            get => mTextArea.Text.Trim();
            set => mTextArea.Text = value.Trim();
        }


        /// <summary>
        /// Value representing the control.
        /// </summary>
        public override object Value
        {
            get
            {
                if (IsValid())
                {
                    return mRelativeUrl;
                }

                return Url;
            }
            set => Url = URLHelper.GetAbsoluteUrl(value as string, CurrentSite.SitePresentationURL);
        }


        /// <summary>
        /// Gets the display name of the value item.
        /// </summary>
        public override string ValueDisplayName => Value as string;


        /// <summary>
        /// OnInit event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnableViewState = false;

            mTextArea = new CMSTextArea
            {
                ID = "textArea",
                WatermarkText = "https://example.com/page"
            };

            Controls.Add(mTextArea);
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        public override bool IsValid()
        {
            if (!base.IsValid() || !ABTestURLHelper.TryRemovePresentationURLAnchorAndQuery(Url, CurrentSite, out mRelativeUrl))
            {
                ValidationError = ResHelper.GetString("abtesting.pagevisit.incorrecturl");

                return false;
            }

            var normalized = AlternativeUrlHelper.NormalizeAlternativeUrl(mRelativeUrl);
            var alternativeUrlInfo = AlternativeUrlInfoProvider.GetAlternativeUrl(normalized, CurrentSite.SiteID);
            if (alternativeUrlInfo == null)
            {
                return true;
            }

            ValidationError = ResHelper.GetString("abtesting.pagevisit.existingalternativeurl");

            if (TryGetMainDocumentUrl(alternativeUrlInfo, out var mainUrl))
            {
                ValidationError += $" {String.Format(ResHelper.GetString("abtesting.pagevisit.mainurlhint"), mainUrl)}";
            }

            return false;
        }


        private bool TryGetMainDocumentUrl(AlternativeUrlInfo alternativeUrlInfo, out string mainUrl)
        {
            mainUrl = String.Empty;

            var document = AlternativeUrlHelper.GetMainDocument(alternativeUrlInfo);
            if (document == null)
            {
                return false;
            }

            mainUrl = URLHelper.CombinePath(DocumentURLProvider.GetUrl(document), '/', CurrentSite.SitePresentationURL, null);

            return true;
        }
    }
}
