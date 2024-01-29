using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Summary description for ShoppingCart.
    /// </summary>
    public class ShoppingCart : CMSAdminControl
    {
        /// <summary>
        /// Default checkout process XML definition.
        /// </summary>
        public const string DEFAULT_CHECKOUT_PROCESS = "<checkout><step name=\"ShoppingCartCustomerSelection\" caption=\"{$Order_New.CustomerSelection.Title$}\" path=\"~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartCustomerSelection.ascx\" livesite=\"false\" cmsdeskorder=\"true\" cmsdeskcustomer=\"false\" cmsdeskorderitems=\"false\" /><step name=\"ShoppingCartContent\" caption=\"{$order_new.cartcontent.title$}\" path=\"~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartContent.ascx\" livesite=\"true\" cmsdeskorder=\"true\" cmsdeskcustomer=\"true\" cmsdeskorderitems=\"true\" icon=\"Content.gif\" /><step name=\"ShoppingCartCheckRegistration\" caption=\"{$Order_new.ShoppingCartCheckRegistration.Title$}\" path=\"~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartCheckRegistration.ascx\" livesite=\"true\" cmsdeskorder=\"false\" cmsdeskcustomer=\"false\" cmsdeskorderitems=\"false\" icon=\"CheckRegistration.gif\" /><step name=\"ShoppingCartOrderAddresses\" caption=\"{$Order_new.ShoppingCartOrderAddresses.Title$}\" path=\"~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartOrderAddresses.ascx\" livesite=\"true\" cmsdeskorder=\"true\" cmsdeskcustomer=\"true\" cmsdeskorderitems=\"false\" icon=\"Addresses.gif\" /><step name=\"ShoppingCartPaymentShipping\" caption=\"{$Order_new.PaymentShipping.Title$}\" path=\"~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartPaymentShipping.ascx\" livesite=\"true\" cmsdeskorder=\"true\" cmsdeskcustomer=\"true\" icon=\"PaymentShipping.gif\" cmsdeskorderitems=\"false\" /><step name=\"ShoppingCartPreview\" caption=\"{$Order_New.CartPreview.Title$}\" path=\"~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartPreview.ascx\" livesite=\"true\" cmsdeskorder=\"true\" cmsdeskcustomer=\"true\" icon=\"OrderPreview.gif\" cmsdeskorderitems=\"false\" /><step name=\"ShoppingCartPaymentGateway\" caption=\"{$Order_New.PaymentGateway.Title$}\" path=\"~/CMSModules/Ecommerce/Controls/ShoppingCart/ShoppingCartPaymentGateway.ascx\" livesite=\"true\" cmsdeskorder=\"true\" cmsdeskorderitems=\"true\" cmsdeskcustomer=\"true\" icon=\"PaymentGateway.gif\" /></checkout>";


        /// <summary>
        /// Default required field marker. Contains html code with styled asterisk.
        /// </summary>
        public const string DEFAULT_REQUIRED_FIELDS_MARK = "<span style=\"color:#eb6d24; vertical-align:top;font-weight:bold; padding-left:1px\">*</span>";


        #region "Protected variables"

        /// <summary>
        /// Local shopping cart object.
        /// </summary>
        protected ShoppingCartInfo mLocalShoppingCart;


        /// <summary>
        /// Shopping cart object.
        /// </summary>
        protected ShoppingCartInfo mShoppingCartObj;


        /// <summary>
        /// Shopping cart current step control.
        /// </summary>
        protected ShoppingCartStep mCurrentStepControl;


        /// <summary>
        /// Indicates whether current step control is loaded after postback.
        /// </summary>
        protected bool? mIsCurrentStepPostBack;


        /// <summary>
        /// Indicates if current step control is loaded.
        /// </summary>
        protected bool mIsControlLoaded;


        /// <summary>
        /// Shopping cart container.
        /// </summary>
        protected Panel mShoppingCartContainer;


        /// <summary>
        /// Checkout process.
        /// </summary>
        protected CheckoutProcessInfo mCheckoutProcess;


        /// <summary>
        /// Checkout process steps.
        /// </summary>
        protected List<CheckoutProcessStepInfo> mCheckoutProcessSteps;


        /// <summary>
        /// Type of the checkout process.
        /// </summary>
        protected CheckoutProcessEnum mCheckoutProcessType = CheckoutProcessEnum.Custom;


        /// <summary>
        /// Previous step index.
        /// </summary>
        protected int mPreviousStepIndex = CheckoutProcessInfo.STEP_INDEX_NOT_KNOWN;


        /// <summary>
        /// Next step index.
        /// </summary>
        protected int mNextStepIndex = CheckoutProcessInfo.STEP_INDEX_NOT_KNOWN;


        /// <summary>
        /// Indicates whether password retrieval is enabled.
        /// </summary>
        protected bool mEnablePasswordRetrieval;


        /// <summary>
        /// Conversion track name used after successful registration.
        /// </summary>
        protected string mRegistrationTrackConversionName = "";


        /// <summary>
        /// Conversion track value used after successful registration.
        /// </summary>
        protected string mRegistrationConversionValue = String.Empty;


        /// <summary>
        /// Conversion track name used after new order is saved.
        /// </summary>
        protected string mOrderTrackConversionName = "";


        /// <summary>
        /// Conversion track value used after new order is saved.
        /// </summary>
        protected string mOrderConversionValue = String.Empty;


        /// <summary>
        /// Conversion track name used after added to shopping cart.
        /// </summary>
        protected string mAddToShoppingCartTrackConversionName = "";


        /// <summary>
        /// Conversion track value used after added to shopping cart.
        /// </summary>
        protected string mAddToShoppingCartConversionValue = String.Empty;


        /// <summary>
        /// Email address the new registration notification should be sent to.
        /// </summary>
        protected string mSendNewRegNotificationToAddress = "";


        /// <summary>
        /// Roles the new user should be assign to after his registration.
        /// </summary>
        protected string mAssignToRoles = "";


        /// <summary>
        /// Sites the new user should be assign to after his registration.
        /// </summary>
        protected string mAssignToSites = "";


        /// <summary>
        /// Redirect after purchase.
        /// </summary>
        protected string mRedirectAfterPurchase = "";


        /// <summary>
        /// Indicates whether product price detail link is displayed.
        /// </summary>
        protected bool mEnableProductPriceDetail;


        /// <summary>
        /// Relative path to the folder with shopping cart step images.
        /// </summary>
        protected string mImageFolderPath = "";


        /// <summary>
        /// HTML code of the image step separator.
        /// </summary>
        protected string mImageStepSeparator = "";


        /// <summary>
        /// HTML code which is attached as postfix to the shopping cart step form required fields.
        /// </summary>
        protected string mRequiredFieldsMark = "";


        /// <summary>
        /// Payment gateway provider.
        /// </summary>
        protected CMSPaymentGatewayProvider mPaymentGatewayProvider;


        private readonly EcommerceActivityLogger mActivityLogger = new EcommerceActivityLogger();

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Fires after the order is completed - all its data are saved in database.
        /// </summary>
        public event EventHandler OnOrderCompleted;

        /// <summary>
        /// Fires after the order payment completed.
        /// </summary>
        public event EventHandler OnPaymentCompleted;


        /// <summary>
        /// Fires after the order payment is skipped.
        /// </summary>
        public event EventHandler OnPaymentSkipped;

        #endregion


        #region "Properties: Shopping cart"

        /// <summary>
        /// Local shopping cart object.
        /// </summary>
        public ShoppingCartInfo LocalShoppingCart
        {
            get
            {
                return mLocalShoppingCart;
            }
            set
            {
                mLocalShoppingCart = value;
            }
        }


        /// <summary>
        /// Shopping cart object.
        /// </summary>
        public ShoppingCartInfo ShoppingCartInfoObj
        {
            get
            {
                if (mLocalShoppingCart == null)
                {
                    // Get shopping cart from the context
                    return mShoppingCartObj ?? (mShoppingCartObj = ECommerceContext.CurrentShoppingCart);
                }

                return mLocalShoppingCart;
            }
            set
            {
                // Set shopping cart
                if (mLocalShoppingCart == null)
                {
                    mShoppingCartObj = value;
                    ECommerceContext.CurrentShoppingCart = value;
                }
                else
                {
                    mLocalShoppingCart = value;
                }
            }
        }

        #endregion


        #region "Properties: Checkout process"

        /// <summary>
        /// Checkout process.
        /// </summary>
        protected CheckoutProcessInfo CheckoutProcess
        {
            get
            {
                if (mCheckoutProcess == null)
                {
                    SiteInfo si = SiteContext.CurrentSite;
                    if (si != null)
                    {
                        string checkoutProcess = ECommerceSettings.CheckoutProcess(si.SiteName);
                        if (checkoutProcess != "")
                        {
                            // Load site checkout process
                            mCheckoutProcess = new CheckoutProcessInfo(checkoutProcess);
                        }
                        else
                        {
                            // Load default checkout process
                            mCheckoutProcess = new CheckoutProcessInfo(DEFAULT_CHECKOUT_PROCESS);
                        }
                    }
                    else
                    {
                        // Create empty checkout process
                        mCheckoutProcess = new CheckoutProcessInfo();
                    }
                }
                return mCheckoutProcess;
            }
        }


        /// <summary>
        /// Checkout process steps.
        /// </summary>
        public List<CheckoutProcessStepInfo> CheckoutProcessSteps
        {
            get
            {
                // Load checkout steps from site settings
                return mCheckoutProcessSteps ?? (mCheckoutProcessSteps = CheckoutProcess.GetCheckoutProcessSteps(CheckoutProcessType));
            }
        }


        /// <summary>
        /// Checkout process type.
        /// </summary>
        public CheckoutProcessEnum CheckoutProcessType
        {
            get
            {
                return mCheckoutProcessType;
            }
            set
            {
                mCheckoutProcessType = value;
            }
        }

        #endregion


        #region "Properties: Indexes"

        /// <summary>
        /// Shopping cart current step index.
        /// </summary>
        public int CurrentStepIndex
        {
            get
            {
                object obj = ViewState["CurrentStepIndex"];
                return (obj == null) ? 0 : Convert.ToInt32(obj);
            }
            set
            {
                ViewState["CurrentStepIndex"] = value;
            }
        }


        /// <summary>
        /// Previous step index. (Set it if you want to load another step with index different from CurrentStepIndex - 1).
        /// </summary>
        public int PreviousStepIndex
        {
            get
            {
                // Use default previous step
                if (mPreviousStepIndex == CheckoutProcessInfo.STEP_INDEX_NOT_KNOWN)
                {
                    if (CurrentStepIndex > 0)
                    {
                        return CurrentStepIndex - 1;
                    }

                    return 0;
                }

                // Use custom previous step
                return mPreviousStepIndex;
            }
            set
            {
                mPreviousStepIndex = value;
            }
        }


        /// <summary>
        /// Next step index. (Set it if you want to load another step with index different from CurrentStepIndex + 1).
        /// </summary>
        public int NextStepIndex
        {
            get
            {
                // Use default next step
                if (mNextStepIndex == CheckoutProcessInfo.STEP_INDEX_NOT_KNOWN)
                {
                    if (CurrentStepIndex < CheckoutProcessSteps.Count - 1)
                    {
                        return CurrentStepIndex + 1;
                    }

                    return CheckoutProcessSteps.Count - 1;
                }

                // Use custom next step
                return mNextStepIndex;
            }
            set
            {
                mNextStepIndex = value;
            }
        }

        #endregion


        #region "Other properties"

        /// <summary>
        /// Button 'Back' - you need to override this property to get access to shopping cart 'Back' button in shopping cart steps.
        /// </summary>
        public virtual CMSButton ButtonBack
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Button 'Next' - you need to override this property to get access to shopping cart 'Next' button in shopping cart steps.
        /// </summary>
        public virtual CMSButton ButtonNext
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Shopping cart container.
        /// </summary>
        public Panel ShoppingCartContainer
        {
            get
            {
                return mShoppingCartContainer;
            }
            set
            {
                mShoppingCartContainer = value;
            }
        }


        /// <summary>
        /// Page url, we should return to after "Continue shopping" button is clicked.
        /// </summary>
        public string PreviousPageUrl
        {
            get
            {
                object obj = ViewState["PreviousPageUrl"];
                return (obj != null) ? (string)obj : "~/";
            }
            set
            {
                ViewState["PreviousPageUrl"] = value;
            }
        }


        /// <summary>
        /// Shopping cart current step control.
        /// </summary>
        public ShoppingCartStep CurrentStepControl
        {
            get
            {
                // If neither external control nor control from standard checkout process is loaded
                if (!mIsControlLoaded)
                {
                    if (ExternalControlPath == "")
                    {
                        // Load current step control according to the current step index from checkout process definition
                        mCurrentStepControl = GetShoppingCartStepControl(CurrentStepIndex);
                        mIsControlLoaded = (mCurrentStepControl != null);
                    }
                    else
                    {
                        try
                        {
                            // Load current step control from external source
                            mCurrentStepControl = (ShoppingCartStep)Page.LoadUserControl(ExternalControlPath);
                            mCurrentStepControl.ID = "currentStep";
                            mCurrentStepControl.ShoppingCartControl = this;
                            mCurrentStepControl.IsLiveSite = IsLiveSite;
                            mIsControlLoaded = true;
                        }
                        catch
                        {
                            mIsControlLoaded = false;
                        }
                    }
                }
                return mCurrentStepControl;
            }
        }


        /// <summary>
        /// Payment gateway provider instance.
        /// </summary>
        public CMSPaymentGatewayProvider PaymentGatewayProvider
        {
            get
            {
                if (mPaymentGatewayProvider == null)
                {
                    // Get payment gateway provider instance
                    mPaymentGatewayProvider = CMSPaymentGatewayProvider.GetPaymentGatewayProvider<CMSPaymentGatewayProvider>(ShoppingCartInfoObj.ShoppingCartPaymentOptionID);

                    if (mPaymentGatewayProvider != null)
                    {
                        mPaymentGatewayProvider.OrderId = ShoppingCartInfoObj.OrderId;
                    }
                }
                return mPaymentGatewayProvider;
            }
            set
            {
                mPaymentGatewayProvider = value;
            }
        }


        /// <summary>
        /// Indicates whether current step control is loaded after postback.
        /// </summary>
        public bool IsCurrentStepPostBack
        {
            get
            {
                if (mIsCurrentStepPostBack == null)
                {
                    mIsCurrentStepPostBack = !((CurrentStepIndex == 0) && !RequestHelper.IsPostBack());
                }
                return mIsCurrentStepPostBack.Value;
            }
        }


        /// <summary>
        /// External control (control which is not defined in current checkout process) path.
        /// </summary>
        private string ExternalControlPath
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ExternalControlPath"], "");
            }
            set
            {
                ViewState["ExternalControlPath"] = value;
            }
        }


        /// <summary>
        /// Indicates whether it is an internal order made from administration.
        /// </summary>
        public bool IsInternalOrder
        {
            get
            {
                return ((CheckoutProcessType != CheckoutProcessEnum.LiveSite) && (CheckoutProcessType != CheckoutProcessEnum.Custom));
            }
        }


        /// <summary>
        /// Indicates whether password retrieval is enabled.
        /// </summary>
        public bool EnablePasswordRetrieval
        {
            get
            {
                return mEnablePasswordRetrieval;
            }
            set
            {
                mEnablePasswordRetrieval = value;
            }
        }


        /// <summary>
        /// Conversion track name used after successful registration.
        /// </summary>
        public string RegistrationTrackConversionName
        {
            get
            {
                return mRegistrationTrackConversionName;
            }
            set
            {
                mRegistrationTrackConversionName = value;
            }
        }


        /// <summary>
        /// Conversion track name used after new order is saved.
        /// </summary>
        public string OrderTrackConversionName
        {
            get
            {
                return mOrderTrackConversionName;
            }
            set
            {
                mOrderTrackConversionName = value;
            }
        }


        /// <summary>
        /// Email address the new registration notification should be sent to.
        /// </summary>
        public string SendNewRegistrationNotificationToAddress
        {
            get
            {
                return mSendNewRegNotificationToAddress;
            }
            set
            {
                mSendNewRegNotificationToAddress = value;
            }
        }


        /// <summary>
        /// Roles the new user should be assign to after his registration.
        /// </summary>
        public string AssignToRoles
        {
            get
            {
                return mAssignToRoles;
            }
            set
            {
                mAssignToRoles = value;
            }
        }


        /// <summary>
        /// Sites the new user should be assign to after his registration.
        /// </summary>
        public string AssignToSites
        {
            get
            {
                return mAssignToSites;
            }
            set
            {
                mAssignToSites = value;
            }
        }


        /// <summary>
        /// Redirect after purchase.
        /// </summary>
        public string RedirectAfterPurchase
        {
            get
            {
                return mRedirectAfterPurchase;
            }
            set
            {
                mRedirectAfterPurchase = value;
            }
        }


        /// <summary>
        /// Indicates whether product price detail link is displayed.
        /// </summary>
        public bool EnableProductPriceDetail
        {
            get
            {
                return mEnableProductPriceDetail;
            }
            set
            {
                mEnableProductPriceDetail = value;
            }
        }


        /// <summary>
        /// Relative path to the folder with shopping cart step images.
        /// </summary>
        public string ImageFolderPath
        {
            get
            {
                if (mImageFolderPath == "")
                {
                    if (IsLiveSite)
                    {
                        mImageFolderPath = GetImageUrl("ShoppingCart/");
                    }
                    else
                    {
                        mImageFolderPath = GetImageUrl("CMSModules/CMS_Ecommerce/", false, true);
                    }
                }

                return mImageFolderPath;
            }
            set
            {
                mImageFolderPath = value;
            }
        }


        /// <summary>
        /// HTML code of the image step separator.
        /// </summary>
        public string ImageStepSeparator
        {
            get
            {
                return mImageStepSeparator;
            }
            set
            {
                mImageStepSeparator = value;
            }
        }


        /// <summary>
        /// HTML code which is attached as postfix to the shopping cart step form required fields.
        /// </summary>
        public string RequiredFieldsMark
        {
            get
            {
                return mRequiredFieldsMark;
            }
            set
            {
                mRequiredFieldsMark = value;
            }
        }


        /// <summary>
        /// User info - returns shopping cart user, if not specified returns current user.
        /// </summary>
        public UserInfo UserInfo
        {
            get
            {
                ShoppingCartInfo currentCart = ShoppingCartInfoObj;
                if (currentCart?.User == null)
                {
                    return MembershipContext.AuthenticatedUser;
                }

                return currentCart.User;
            }
        }


        /// <summary>
        /// Current contact ID. Shared across shopping cart steps.
        /// </summary>
        public int CurrentContactID
        {
            get;
            set;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns initialized shopping cart step control.
        /// </summary>
        /// <param name="processStepObj">Checkout process step information</param>
        private ShoppingCartStep GetShoppingCartStepControl(CheckoutProcessStepInfo processStepObj)
        {
            if (processStepObj != null)
            {
                try
                {
                    ShoppingCartStep stepControl = (ShoppingCartStep)Page.LoadUserControl(processStepObj.ControlPath);
                    if (stepControl != null)
                    {
                        // Set shopping cart step properties
                        stepControl.CheckoutProcessStep = processStepObj;
                        stepControl.ShoppingCartControl = this;
                        stepControl.IsLiveSite = IsLiveSite;
                        stepControl.ID = "wzdStep" + processStepObj.StepIndex;
                        return stepControl;
                    }
                }
                catch (Exception ex)
                {
                    // Log the event
                    LogErrorLoadingStep(EventLogProvider.GetExceptionLogMessage(ex));
                }
            }
            return null;
        }


        /// <summary>
        /// Logs error while loading shopping cart step.
        /// </summary>
        /// <param name="message">Error message</param>
        private void LogErrorLoadingStep(string message)
        {
            try
            {
                int siteId = 0;
                int userId = 0;
                string userName = "";

                // Get current user information
                UserInfo ui = MembershipContext.AuthenticatedUser;
                if (ui != null)
                {
                    userId = ui.UserID;
                    userName = ui.UserName;
                }

                // Get current site information
                SiteInfo si = SiteContext.CurrentSite;
                if (si != null)
                {
                    siteId = si.SiteID;
                }

                // Log the event
                EventLogProvider.LogEvent(EventType.ERROR, "ShoppingCart", "EXCEPTION", message, RequestContext.CurrentURL, userId, userName, 0, "", "", siteId);
            }
            catch
            {
                // Unable to log the event
            }
        }


        /// <summary>
        /// Returns CheckoutProcessStepInfo object from the checkout process step list.
        /// </summary>
        /// <param name="index">Index of the checkout process step to be returned</param>
        private CheckoutProcessStepInfo GetCheckoutProcessStepFromList(int index)
        {
            if ((index >= 0) && (index < CheckoutProcessSteps.Count))
            {
                return CheckoutProcessSteps[index];
            }
            return null;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns the value from the ShoppingCart viewstate.
        /// </summary>
        /// <param name="key">Key to viewstate value</param>
        public object GetTempValue(string key)
        {
            return ViewState[key];
        }


        /// <summary>
        /// Sets the value to the ShoppingCart viewstate.
        /// </summary>
        /// <param name="key">Key to viewstate value</param>
        /// <param name="value">Value to store</param>
        public void SetTempValue(string key, object value)
        {
            if (ViewState[key] != null)
            {
                if (value != null)
                {
                    ViewState[key] = value;
                }
                else
                {
                    ViewState.Remove(key);
                }
            }
            else if (value != null)
            {
                ViewState[key] = value;
            }
        }


        /// <summary>
        /// Loads checkout process from external source.
        /// </summary>
        public void LoadCheckoutProcess(CheckoutProcessInfo process)
        {
            mCheckoutProcess = process;
        }


        /// <summary>
        /// Returns initialized shopping cart step control.
        /// </summary>
        /// <param name="stepCodeName">Step code name</param>
        public ShoppingCartStep GetShoppingCartStepControl(string stepCodeName)
        {
            // Get checkout process step information
            var processStepObj = CheckoutProcessSteps.FirstOrDefault(step => step.Name.Equals(stepCodeName, StringComparison.OrdinalIgnoreCase));

            // Get checkout process step control
            return GetShoppingCartStepControl(processStepObj);
        }


        /// <summary>
        /// Returns initialized shopping cart step control.
        /// </summary>
        /// <param name="index">Step index</param>
        public ShoppingCartStep GetShoppingCartStepControl(int index)
        {
            // Get checkout process step information
            CheckoutProcessStepInfo processStepObj = GetCheckoutProcessStepFromList(index);

            // Get checkout process step control
            return GetShoppingCartStepControl(processStepObj);
        }


        /// <summary>
        /// Loads specified shopping cart step control and set current step index to control index (step is included in current checkout process) or leave current step index as is (when loading external control which is not defined in current checkout process).
        /// </summary>
        /// <param name="stepControl">Shopping cart step control to be loaded</param>
        public void LoadStep(ShoppingCartStep stepControl)
        {
            // Save current contact ID from current step
            if (mCurrentStepControl != null)
            {
                CurrentContactID = mCurrentStepControl.ContactID;
            }

            // Control is included in current checkout process
            if ((stepControl?.CheckoutProcessStep != null)
                && (stepControl.CheckoutProcessStep.StepIndex >= 0)
                && (stepControl.CheckoutProcessStep.StepIndex < CheckoutProcessSteps.Count))
            {
                // Set current step index to current checkout process step index
                CurrentStepIndex = stepControl.CheckoutProcessStep.StepIndex;
                // Clear external control path
                ExternalControlPath = "";
                // Force loading checkout process step control
                mIsControlLoaded = false;
            }
            // Control is not included in current checkout process, it is loaded from external source
            else
            {
                // Leave current step index as is and set current step control
                stepControl.ShoppingCartControl = this;
                mCurrentStepControl = stepControl;

                // Remember external control path in viewstate
                ExternalControlPath = stepControl.AppRelativeVirtualPath;
                mIsControlLoaded = true;
            }

            mIsCurrentStepPostBack = false;
            LoadCurrentStep();

            mCurrentStepControl.ContactID = CurrentContactID;
        }


        /// <summary>
        /// Loads step control with specified index and set current step index to control index.
        /// </summary>
        /// <param name="index">Checkout process step index</param>
        public void LoadStep(int index)
        {
            ShoppingCartStep stepControl = GetShoppingCartStepControl(index);
            LoadStep(stepControl);
        }


        /// <summary>
        /// Loads step control with specified name and set current step index to control index.
        /// </summary>
        /// <param name="stepName">Checkout process step code name</param>
        public void LoadStep(string stepName)
        {
            ShoppingCartStep stepControl = GetShoppingCartStepControl(stepName);
            LoadStep(stepControl);
        }


        /// <summary>
        /// Loads current step - you need to override this method to load current step control.
        /// </summary>
        public virtual void LoadCurrentStep()
        {
        }

        #endregion


        #region "Common methods"

        /// <summary>
        /// Removes current shopping cart data from database and from session.
        /// </summary>
        public virtual void CleanUpShoppingCart()
        {
            if (ShoppingCartInfoObj != null)
            {
                ShoppingCartInfoProvider.DeleteShoppingCartInfo(ShoppingCartInfoObj.ShoppingCartID);
                ShoppingCartInfoObj = null;
            }
        }


        /// <summary>
        /// Returns url the customer should be redirected to after purchase.
        /// </summary>
        public virtual string GetRedirectAfterPurchaseUrl()
        {
            // Get payment gateway url
            string paymentUrl = PaymentOptionInfoProvider.GetPaymentURL(ShoppingCartInfoObj);
            if (paymentUrl != "")
            {
                RedirectAfterPurchase = paymentUrl;
            }
            // Get 'Url after purchase' from shopping cart control
            else if (RedirectAfterPurchase != "")
            {
                RedirectAfterPurchase = DocumentURLProvider.GetUrl(RedirectAfterPurchase);
            }

            // Add 'orderid' parameter to url
            if ((RedirectAfterPurchase != "") && (ShoppingCartInfoObj.OrderId > 0))
            {
                if (RedirectAfterPurchase.StartsWith("~", StringComparison.Ordinal))
                {
                    RedirectAfterPurchase = ResolveUrl(RedirectAfterPurchase);
                }
                RedirectAfterPurchase = URLHelper.AddParameterToUrl(RedirectAfterPurchase, "orderid", ShoppingCartInfoObj.OrderId.ToString());
            }

            return RedirectAfterPurchase;
        }


        /// <summary>
        /// Raises order completed event - after the order is completed (all its data are saved in database).
        /// </summary>
        public virtual void RaiseOrderCompletedEvent()
        {
            OnOrderCompleted?.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// Raises payment completed event - after the order payments is completed.
        /// </summary>
        public virtual void RaisePaymentCompletedEvent()
        {
            OnPaymentCompleted?.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// Raises payment skipped event - after the order payments is skipped.
        /// </summary>
        public virtual void RaisePaymentSkippedEvent()
        {
            OnPaymentSkipped?.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// Reloads shopping cart items and settings from database.
        /// </summary>
        /// <param name="cartGuid">GUID of the shopping cart to be reloaded</param>
        public virtual void ReloadShoppingCartInfo(Guid cartGuid)
        {
            ShoppingCartInfoObj = ShoppingCartInfoProvider.GetShoppingCartInfo(cartGuid);

            // if shopping cart is not in database -> create empty one
            if ((ShoppingCartInfoObj == null) && (SiteContext.CurrentSite != null))
            {
                ShoppingCartInfoObj = ShoppingCartFactory.CreateCart(SiteContext.CurrentSite.SiteID);
            }
        }

        #endregion


        #region "On-line marketing - Activities"

        /// <summary>
        /// Logs activity "product added to shopping cart".
        /// </summary>
        /// <param name="sku">SKU info object.</param>
        /// <param name="quantity">Quantity</param>
        public virtual void TrackActivityProductAddedToShoppingCart(SKUInfo sku, int quantity)
        {
            mActivityLogger.LogProductAddedToShoppingCartActivity(sku, quantity);
        }


        /// <summary>
        /// Logs activity "product removed from shopping cart" for all items in shopping cart.
        /// </summary>
        /// <param name="shoppingCartInfoObj">Shopping cart</param>
        /// <param name="siteName">Site name</param>
        /// <param name="contactId">ContactID</param>
        public virtual void TrackActivityAllProductsRemovedFromShoppingCart(ShoppingCartInfo shoppingCartInfoObj, string siteName, int contactId)
        {
            // Check if shopping contains any items
            if ((shoppingCartInfoObj != null) && !shoppingCartInfoObj.IsEmpty)
            {
                foreach (ShoppingCartItemInfo cartItem in shoppingCartInfoObj.CartProducts)
                {
                    mActivityLogger.LogProductRemovedFromShoppingCartActivity(cartItem.SKU, cartItem.CartItemUnits, contactId);
                }
            }
        }


        /// <summary>
        /// Logs activity "purchase" for all items.
        /// </summary>
        /// <param name="shoppingCartInfoObj">Shopping cart</param>
        /// <param name="siteName">Site name</param>
        /// <param name="contactId">Contact ID</param>
        public virtual void TrackActivityPurchasedProducts(ShoppingCartInfo shoppingCartInfoObj, string siteName, int contactId)
        {
            // Check if shopping contains any items
            if ((shoppingCartInfoObj == null) || shoppingCartInfoObj.IsEmpty)
            {
                return;
            }
            // Loop through all products and log activity
            foreach (ShoppingCartItemInfo cartItem in shoppingCartInfoObj.CartProducts)
            {
                mActivityLogger.LogPurchasedProductActivity(cartItem.SKU, cartItem.CartItemUnits, contactId);
            }
        }


        /// <summary>
        /// Logs activity "purchase".
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="url">URL</param>
        /// <param name="totalPrice">Total price in main currency</param>
        /// <param name="totalPriceAsString">Total price in main currency user friendly formatted</param>
        public virtual void TrackActivityPurchase(int orderId, int contactId, string siteName, string url, decimal totalPrice, string totalPriceAsString)
        {
            mActivityLogger.LogPurchaseActivity(orderId, totalPrice, totalPriceAsString, false, contactId);
        }

        #endregion


        #region "Button actions"

        /// <summary>
        /// Standard action after the 'Next button' is clicked - 1. Validates current step, 2. Process current step information, 3. Loads next step.
        /// </summary>
        public virtual void ButtonNextClickAction()
        {
            if (IsInternalOrder)
            {
                CheckPermissions(ModuleName.ECOMMERCE, EcommercePermissions.ORDERS_MODIFY);
            }
            // Ensure user still exists
            if (ShoppingCartInfoObj.ShoppingCartUserID != 0 && UserInfoProvider.GetUserInfo(ShoppingCartInfoObj.ShoppingCartUserID) == null)
            {
                ShoppingCartInfoObj.User = null;
            }

            // Validate current step
            if (CurrentStepControl.IsValid())
            {
                // Process information from the current step
                if (CurrentStepControl.ProcessStep())
                {
                    // Checkout process is not completed 
                    if (CurrentStepIndex < CheckoutProcessSteps.Count - 1)
                    {
                        if (NextStepIndex < CheckoutProcessSteps.Count)
                        {
                            // Load next standard checkout process step
                            CurrentContactID = mCurrentStepControl.ContactID;
                            CurrentStepIndex = NextStepIndex;
                            ExternalControlPath = "";

                            mIsControlLoaded = false;
                            mIsCurrentStepPostBack = false;
                            LoadCurrentStep();
                            mCurrentStepControl.ContactID = CurrentContactID;
                        }
                    }
                    // Last step was processed
                    else if (CurrentStepIndex == CheckoutProcessSteps.Count - 1)
                    {
                        // Final actions after purchase are defined in final step of the checkout process
                    }
                }
            }
        }


        /// <summary>
        /// Standard action after the 'Back button' is clicked - Loads previous step.
        /// </summary>
        public virtual void ButtonBackClickAction()
        {
            if (CurrentStepIndex > 0)
            {
                if (PreviousStepIndex >= 0)
                {
                    // If external control is loaded
                    if (ExternalControlPath != "")
                    {
                        // Do not change current step index - go backwards to its parent step from checkout process definition
                        ExternalControlPath = "";
                    }
                    else
                    {
                        // Load previous standard checkout process step
                        CurrentContactID = mCurrentStepControl.ContactID;
                        CurrentStepIndex = PreviousStepIndex;
                    }

                    mIsControlLoaded = false;
                    mIsCurrentStepPostBack = false;
                    LoadCurrentStep();
                    mCurrentStepControl.ContactID = CurrentContactID;
                }
            }
        }

        #endregion
    }
}