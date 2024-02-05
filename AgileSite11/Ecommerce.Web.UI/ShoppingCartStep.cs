using System;
using System.Web.UI.WebControls;

using CMS.Activities;
using CMS.DataEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Class describing shopping cart base functionality.
    /// </summary>
    public class ShoppingCartStep : CMSUserControl
    {
        #region "Variables"

        private int mContactID;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Parent shopping cart control.
        /// </summary>
        public ShoppingCart ShoppingCartControl
        {
            get;
            set;
        }


        /// <summary>
        /// Checkout process step information.
        /// </summary>
        public CheckoutProcessStepInfo CheckoutProcessStep
        {
            get;
            set;
        }


        /// <summary>
        /// Shopping cart info object of the parent shopping cart.
        /// </summary>
        public ShoppingCartInfo ShoppingCart
        {
            get
            {
                return ShoppingCartControl?.ShoppingCartInfoObj;
            }
            set
            {
                if (ShoppingCartControl != null)
                {
                    ShoppingCartControl.ShoppingCartInfoObj = value;
                }
            }
        }


        /// <summary>
        /// Shopping cart step container.
        /// </summary>
        public Panel StepContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is enabled.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Contact ID passed between shopping cart steps.
        /// </summary>
        public int ContactID
        {
            get
            {
                if ((mContactID <= 0) && !ShoppingCartControl.IsInternalOrder)
                {
                    mContactID = ModuleCommands.OnlineMarketingGetCurrentContactID();
                }
                return mContactID;
            }
            set
            {
                mContactID = value;
            }
        }


        /// <summary>
        /// Indicates if logging activity is enabled for current customer.
        /// </summary>
        public bool LogActivityForCustomer
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Process current step information. If succeeded returns true, otherwise returns false.
        /// </summary>
        public virtual bool ProcessStep()
        {
            return true;
        }


        /// <summary>
        /// Validates current step information before processing. If succeeded returns true, otherwise returns false.
        /// </summary>
        public virtual bool IsValid()
        {
            return true;
        }


        /// <summary>
        /// Standard action after the 'Back button' is clicked - calls parent shopping cart control ButtonBackClickAction() by default.
        /// </summary>
        public virtual void ButtonBackClickAction()
        {
            ShoppingCartControl.ButtonBackClickAction();
        }


        /// <summary>
        /// Standard action after the 'Next button' is clicked - calls parent shopping cart control ButtonNextClickAction() by default.
        /// </summary>
        public virtual void ButtonNextClickAction()
        {
            ShoppingCartControl.ButtonNextClickAction();
        }


        /// <summary>
        /// Default OnLoad event.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitCurrentContact();
        }


        /// <summary>
        /// Initializes current contact ID.
        /// </summary>
        private void InitCurrentContact()
        {
            LogActivityForCustomer = true;

            if (ContactID <= 0)
            {
                if (ShoppingCartControl.IsInternalOrder) // Is it an internal order (i.e. order made in CMSDesk)?
                {
                    // Retrieve all contacts for current customer
                    ContactID = ActivityTrackingHelper.GetContactID(ShoppingCart.Customer, SiteContext.CurrentSiteID);
                }
                else
                {
                    // Get contact ID for current user
                    LogActivityForCustomer = ActivitySettingsHelper.ActivitiesEnabledForThisUser(MembershipContext.AuthenticatedUser);
                    if (LogActivityForCustomer)
                    {
                        ContactID = ModuleCommands.OnlineMarketingGetCurrentContactID();
                    }
                }
            }
        }

        #endregion
    }
}