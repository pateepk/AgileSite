using System.ComponentModel.Design;
using System.Web.UI.Design;

namespace CMS.UIControls
{
    /// <summary>
    /// This class provide designer support code for <see cref="RecaptchaControl"/>.
    /// </summary>
    public class RecaptchaControlDesigner : ControlDesigner
    {
        #region "Control action list class"

        /// <summary>
        /// Class representing control actions list.
        /// </summary>
        public class ActionList : DesignerActionList
        {
            #region "Variables"

            private RecaptchaControlDesigner mParent;

            #endregion


            #region "Constructors"

            /// <summary>
            /// Initializes a new instance of the <see cref="ActionList"/> class.
            /// </summary>
            public ActionList(RecaptchaControlDesigner parent)
                : base(parent.Component)
            {
                mParent = parent;
            }

            #endregion


            #region "Public methods"

            /// <summary>
            /// Create the ActionItem collection and add one command
            /// </summary>
            public override DesignerActionItemCollection GetSortedActionItems()
            {
                // fixme -- I can't get this to open up automatically (
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionHeaderItem("API Key"));
                items.Add(new DesignerActionTextItem("To use reCAPTCHA, you need an API key from https://www.google.com/recaptcha/admin/create", string.Empty));

                return items;
            }

            #endregion
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Allow control resize
        /// </summary>
        public override bool AllowResize
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        ///  Return a custom ActionList collection
        /// </summary>
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection _actionLists = new DesignerActionListCollection();
                _actionLists.AddRange(base.ActionLists);

                // Add a custom DesignerActionList
                _actionLists.Add(new ActionList(this));
                return _actionLists;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets design time HTML code.
        /// </summary>
        public override string GetDesignTimeHtml()
        {
            return CreatePlaceHolderDesignTimeHtml("reCAPTCHA Validator");
        }

        #endregion
    }
}
