using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// BreadCrumbs control base class.
    /// </summary>
    public abstract class Breadcrumbs : CMSUserControl
    {
        #region "Variables"

        private List<BreadcrumbItem> mBreadcrumbItems;
        private bool mEncodeBreadCrumbs = true;
        private bool mChangeTargetFrame = true;
        private bool mHideBreadcrumbs = true;
        private bool mPropagateToMainNavigation = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Collection of breadcrumbs.
        /// </summary>
        public virtual List<BreadcrumbItem> Items
        {
            get
            {
                return mBreadcrumbItems ?? (mBreadcrumbItems = new List<BreadcrumbItem>());
            }
            set
            {
                mBreadcrumbItems = value;
            }
        }


        /// <summary>
        /// Returns count of breadcrumb items.
        /// </summary>
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }


        /// <summary>
        /// Enables or disables HTML encoding of breadcrumbs item. Default true.
        /// </summary>
        public bool EncodeBreadcrumbs
        {
            get
            {
                return mEncodeBreadCrumbs;
            }
            set
            {
                mEncodeBreadCrumbs = value;
            }
        }
        

        /// <summary>
        /// Indicates if breadcrumbs path should be hidden. Default true.
        /// </summary>
        public virtual bool HideBreadcrumbs
        {
            get
            {
                return mHideBreadcrumbs;
            }
            set
            {
                mHideBreadcrumbs = value;
            }
        }


        /// <summary>
        /// Change target frame on client. Default true.
        /// </summary>
        public bool ChangeTargetFrame
        {
            get
            {
                return mChangeTargetFrame;
            }
            set
            {
                mChangeTargetFrame = value;
            }
        }


        /// <summary>
        /// Indicates whether breadcrumbs items are propagated to main breadcrumbs navigation in administration. Default true.
        /// </summary>
        public bool PropagateToMainNavigation
        {
            get
            {
                return mPropagateToMainNavigation;
            }
            set
            {
                mPropagateToMainNavigation = value;
            }
        }


        /// <summary>
        /// Placeholder into which the breadcrumb items will be generated.
        /// </summary>
        protected abstract PlaceHolder BreadcrumbsContainer
        {
            get;
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// OnPreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Create the breadcrumbs
            var count = Count;
            if (count > 0)
            {
                EnsureBreadcrumbOrder();

                var items = Items;

                if (!HideBreadcrumbs)
                {
                    Visible = true;

                    // Generate the breadcrumbs controls
                    for (int i = 0; i < count; i++)
                    {
                        CreateBreadCrumbsItem(items[i], (i + 1 == count));
                    }
                }
                else
                {
                    Visible = false;
                }

                var last = items.Last();
                if (String.IsNullOrEmpty(last.RedirectUrl))
                {
                    last.RedirectUrl = URLHelper.UrlEncodeQueryString(RequestContext.CurrentURL);
                }

                if (PropagateToMainNavigation)
                {
                    // Register breadcrumbs data for client code and start BreadcrumbsDataSource JS module
                    RequestContext.ClientApplication.Add("breadcrumbs", new
                    {
                        Reframe = ChangeTargetFrame,
                        Data = items
                    });
                }
            }
            else
            {
                Visible = false;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds a breadcrumb.
        /// </summary>
        /// <param name="breadcrumb">Breadcrumb item</param>
        public void AddBreadcrumb(BreadcrumbItem breadcrumb)
        {
            if (breadcrumb != null)
            {
                // Ensure correct index
                if (breadcrumb.Index == -1)
                {
                    breadcrumb.Index = Items.Count;
                }
                else
                {
                    // Post processing of breadcrumb attribute
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Index == breadcrumb.Index)
                        {
                            // Replace breadcrumb with the same index
                            Items[i] = breadcrumb;

                            // Stop processing
                            return;
                        }
                    }
                }

                EnsureBreadcrumbOrder();

                // If breadcrumb with the same index was not found, add it to the list
                Items.Add(breadcrumb);
            }
        }


        /// <summary>
        /// Sorts breadcrumbs by index to be sure the order is ensured for multiple actions.
        /// </summary>
        private void EnsureBreadcrumbOrder()
        {
            if (Items.Count > 0)
            {
                // At least one action has index
                if (Items.Exists(t => (t.Index != -1)))
                {
                    // Sort the actions
                    Items.Sort((t1, t2) => t1.Index.CompareTo(t2.Index));
                }
                else
                {
                    // Initialize default indexes
                    for (int i = 0; i < Items.Count; i++)
                    {
                        Items[i].Index = i;
                    }
                }
            }
        }


        /// <summary>
        /// Creates the internal breadcrumb item representation.
        /// </summary>
        /// <param name="breadcrumb">Breadcrumb item from which internal breadcrumb will be created</param>
        /// <param name="isLast">Indicates whether the <paramref name="breadcrumb"/> is the last one</param>
        private void CreateBreadCrumbsItem(BreadcrumbItem breadcrumb, bool isLast)
        {
            // Make link if URL specified
            string text = ResHelper.LocalizeString(breadcrumb.Text);

            var li = new HtmlGenericControl("li");
            if (!(string.IsNullOrEmpty(breadcrumb.RedirectUrl) && string.IsNullOrEmpty(breadcrumb.OnClientClick)) && !isLast)
            {
                var newLink = new HyperLink
                {
                    Text = EncodeBreadcrumbs ? HTMLHelper.HTMLEncode(text) : text,
                    NavigateUrl = breadcrumb.RedirectUrl,
                    Target = breadcrumb.Target,
                    EnableViewState = false
                };

                // JavaScript is specified add on click
                if (!string.IsNullOrEmpty(breadcrumb.OnClientClick))
                {
                    newLink.Attributes.Add("onclick", breadcrumb.OnClientClick);
                    newLink.Attributes.Add("href", "javascript:void(0)");
                }
                li.Controls.Add(newLink);
            }
            else // Make label if last item or URL not specified
            {
                li.InnerText = text;
            }

            BreadcrumbsContainer.Controls.Add(li);
        }

        #endregion
    }
}