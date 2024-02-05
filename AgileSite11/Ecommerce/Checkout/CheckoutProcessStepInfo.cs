using System.Xml;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Checkout process step object.
    /// </summary>
    public class CheckoutProcessStepInfo
    {
        private string mName = "";
        private string mCaption = "";
        private string mIcon = "";
        private string mControlPath = "";
        private bool mShowInCMSDeskOrder = false;
        private bool mShowInCMSDeskOrderItems = false;
        private bool mShowInCMSDeskCustomer = false;
        private bool mShowOnLiveSite = false;
        private int mStepIndex = CheckoutProcessInfo.STEP_INDEX_NOT_KNOWN;
        private bool mIsExternal = false;


        #region "Public properties"

        /// <summary>
        /// Step caption.
        /// </summary>
        public string Caption
        {
            get
            {
                return mCaption;
            }
            set
            {
                mCaption = value;
            }
        }


        /// <summary>
        /// Step code name.
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }


        /// <summary>
        /// Step icon.
        /// </summary>
        public string Icon
        {
            get
            {
                return mIcon;
            }
            set
            {
                mIcon = value;
            }
        }


        /// <summary>
        /// ASCX control path.
        /// </summary>
        public string ControlPath
        {
            get
            {
                return mControlPath;
            }
            set
            {
                mControlPath = value;
            }
        }


        /// <summary>
        /// Indicates whether step should be included in checkout process in CMSDesk Order section.
        /// </summary>
        public bool ShowInCMSDeskOrder
        {
            get
            {
                return mShowInCMSDeskOrder;
            }
            set
            {
                mShowInCMSDeskOrder = value;
            }
        }


        /// <summary>
        /// Indicates whether step should be included in checkout process in CMSDesk Order/Items section
        /// </summary>
        public bool ShowInCMSDeskOrderItems
        {
            get
            {
                return mShowInCMSDeskOrderItems;
            }
            set
            {
                mShowInCMSDeskOrderItems = value;
            }
        }


        /// <summary>
        /// Indicates whether step should be included in checkout process in CMSDesk Customer section.
        /// </summary>
        public bool ShowInCMSDeskCustomer
        {
            get
            {
                return mShowInCMSDeskCustomer;
            }
            set
            {
                mShowInCMSDeskCustomer = value;
            }
        }


        /// <summary>
        /// Indicates whether step should be included in checkout process on the live site.
        /// </summary>
        public bool ShowOnLiveSite
        {
            get
            {
                return mShowOnLiveSite;
            }
            set
            {
                mShowOnLiveSite = value;
            }
        }


        /// <summary>
        /// Zero based step index - initialized when creating list of checkout process steps.
        /// </summary>
        public int StepIndex
        {
            get
            {
                return mStepIndex;
            }
            set
            {
                mStepIndex = value;
            }
        }


        /// <summary>
        /// True - step is not defined in standard checkout process, otherwise False.
        /// </summary>
        public bool IsExternal
        {
            get
            {
                return mIsExternal;
            }
            set
            {
                mIsExternal = value;
            }
        }

        #endregion


        /// <summary>
        /// Constructor - creates empty checkout proces step object.
        /// </summary>
        public CheckoutProcessStepInfo()
        {
        }


        /// <summary>
        /// Constructor - creates checkout proces step object from XML node data.
        /// </summary>
        public CheckoutProcessStepInfo(XmlNode node)
        {
            if ((node != null) && (node.Name.ToLowerCSafe() == "step"))
            {
                Name = XmlHelper.GetXmlAttributeValue(node.Attributes["name"], "");
                Caption = XmlHelper.GetXmlAttributeValue(node.Attributes["caption"], "");
                Icon = XmlHelper.GetXmlAttributeValue(node.Attributes["icon"], "");
                ControlPath = XmlHelper.GetXmlAttributeValue(node.Attributes["path"], "");
                ShowOnLiveSite = ValidationHelper.GetBoolean(XmlHelper.GetXmlAttributeValue(node.Attributes["livesite"], ""), false);
                ShowInCMSDeskOrder = ValidationHelper.GetBoolean(XmlHelper.GetXmlAttributeValue(node.Attributes["cmsdeskorder"], ""), false);
                ShowInCMSDeskOrderItems = ValidationHelper.GetBoolean(XmlHelper.GetXmlAttributeValue(node.Attributes["cmsdeskorderitems"], ""), false);
                ShowInCMSDeskCustomer = ValidationHelper.GetBoolean(XmlHelper.GetXmlAttributeValue(node.Attributes["cmsdeskcustomer"], ""), false);
                StepIndex = CheckoutProcessInfo.STEP_INDEX_NOT_KNOWN;
                IsExternal = false;
            }
        }
    }
}