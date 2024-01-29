using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{

    /// <summary>
    /// Represents base control for displaying and handling the UniGrid mass actions.
    /// </summary>
    public abstract class MassActions : CMSUserControl, ICallbackEventHandler, IExtensibleMassActions
    {
        #region "Fields and Properties"

        private readonly List<MassActionItem> mMassActions = new List<MassActionItem>();


        /// <summary>
        /// Resource string for error message shown when no item was selected by user.
        /// </summary>
        public string NoItemsSelectedMessageResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string for error message shown when no action was selected by user.
        /// </summary>
        public string NoActionSelectedMessageResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Client ID of hidden input containing the selected items.
        /// </summary>
        /// <remarks>
        /// Items are separated by pipe ('|'). UniGrid automatically creates this hidden field. Its ClientID can be obtained by <see cref="UniGrid.GetSelectionFieldClientID"/> method.
        /// </remarks>
        public string SelectedItemsClientID
        {
            get;
            set;
        }


        /// <summary>
        /// If mass action is called, Value of this Lazy object will be passed to the CreateUrl callback. If this property is null, nothing is passed to the callback.
        /// </summary>
        public Lazy<object> AdditionalParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set the resource string which will be used as 'Selected items' item in the drop down list. If not set, the default text (Selected items) will be displayed.
        /// </summary>
        public string SelectedItemsResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set the resource string which will be used as 'All items' item in the drop down list. If not set, the default text (All items) will be displayed.
        /// </summary>
        public string AllItemsResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Represents drop-down for scope (whether mass action should be applied either on all or selected items) selection.
        /// </summary>
        protected abstract CMSDropDownList ScopeDropDown
        {
            get;
        }


        /// <summary>
        /// Represents drop-down with specified mass actions.
        /// </summary>
        protected abstract CMSDropDownList ActionDropDown
        {
            get;
        }


        /// <summary>
        /// Represents placeholder where error messages (i.e. <see cref="NoActionSelectedMessageResourceString"/> or <see cref="NoItemsSelectedMessageResourceString"/>) could be displayed.
        /// </summary>
        protected abstract Control Messages
        {
            get;
        }


        /// <summary>
        /// Represents the button that executes selected mass action on items from selected scope. 
        /// </summary>
        protected abstract Control ConfirmationButton
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds new mass actions to the underlying collection.
        /// </summary>
        /// <param name="massActionItems">Mass action to be added</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="massActionItems"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="massActionItems"/> contains at least one <c>null</c> item.</exception>
        public void AddMassActions(params MassActionItem[] massActionItems)
        {
            if (massActionItems == null)
            {
                throw new ArgumentNullException("massActionItems");
            }

            if (massActionItems.Any(item => item == null))
            {
                throw new ArgumentException("MassActionItems contains at least one null item.");
            }
            
            mMassActions.AddRange(massActionItems);
        }


        /// <summary>
        /// OnLoad override, setup access denied page depending on current usage and fills drop-downs.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (StopProcessing)
            {
                return;
            }

            base.OnLoad(e);
            FillDropdowns();

            if (string.IsNullOrEmpty(SelectedItemsClientID))
            {
                throw new InvalidOperationException("SelectedItemsClientID property has to be set to pass information about selected items");
            }
        }


        /// <summary>
        /// Registers <see cref="ConfirmationButton"/> script when mass actions control is supposed to be rendered.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!RequestHelper.IsCallback())
            {
                RegisterScript();
            }
        }


        /// <summary>
        /// Registers script and provides it with elements' identifiers and localized error messages. 
        /// </summary>
        private void RegisterScript()
        {
            var moduleParams = new
            {
                CallbackTargetUniqueID = UniqueID,
                ButtonClientID = ConfirmationButton.ClientID,
                ScopeDropDownClientID = ScopeDropDown.ClientID,
                ActionDropDownClientID = ActionDropDown.ClientID,
                SelectedItemsClientID,
                MessagePlaceHolderClientID = Messages.ClientID,
                NoItemsSelectedMessage = GetString(NoItemsSelectedMessageResourceString ?? "massaction.selectsomeitem"),
                NoActionSelectedMessage = GetString(NoActionSelectedMessageResourceString ?? "massaction.selectsomeaction")
            };
            ScriptHelper.RegisterModule(this, "CMS/MassActions", moduleParams);
        }


        /// <summary>
        /// Inserts default items into <see cref="ScopeDropDown"/> and <see cref="ActionDropDown"/> dropdowns.
        /// Also actions from the <see cref="mMassActions"/> are added to the <see cref="ActionDropDown"/>.
        /// </summary>
        private void FillDropdowns()
        {
            if (!RequestHelper.IsPostBack())
            {
                ScopeDropDown.Items.Add(new ListItem(GetString(SelectedItemsResourceString ?? "general.SelectedItems"), Convert.ToInt32(MassActionScopeEnum.SelectedItems).ToString()));
                ScopeDropDown.Items.Add(new ListItem(GetString(AllItemsResourceString ?? "general.AllItems"), Convert.ToInt32(MassActionScopeEnum.AllItems).ToString()));
            }

            ActionDropDown.Items.Add(new ListItem(GetString("general.SelectAction"), "##noaction##"));

            var actionItems = mMassActions
                .Select(action => new ListItem(GetString(action.DisplayNameResourceString), action.CodeName))
                .ToArray();
            ActionDropDown.Items.AddRange(actionItems);
        }

        #endregion


        #region "Callbacks handling (ICallbackEventHandler members and helping fields and types)"

        /// <summary>
        /// Wrapper for arguments needed when performing callback.
        /// </summary>
        private class CallbackArguments
        {
            /// <summary>
            /// Code name of action, used as the unique identifier of mass action.
            /// </summary>
            public string ActionCodeName
            {
                get;
                set;
            }


            /// <summary>
            /// Determines whether action should be performed only on selected items or on all items which satisfies filter condition.
            /// </summary>
            public MassActionScopeEnum Scope
            {
                get;
                set;
            }


            /// <summary>
            /// Collection containing IDs of selected items.
            /// </summary>
            public List<int> SelectedItems
            {
                get;
                set;
            }
        }


        private CallbackArguments mCallbackArguments;


        /// <summary>
        /// Processes a callback event that targets a control.
        /// </summary>
        /// <param name="eventArgument">A string that represents an event argument to pass to the event handler.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventArgument"/> is null</exception>
        public void RaiseCallbackEvent(string eventArgument)
        {
            if (eventArgument == null)
            {
                throw new ArgumentNullException("eventArgument");
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            mCallbackArguments = serializer.Deserialize<CallbackArguments>(eventArgument);

            if (mCallbackArguments == null)
            {
                throw new InvalidOperationException("[MassActions.RaiseCallbackEvent]: callback arguments cannot be deserialized");
            }

            if (mCallbackArguments.Scope == MassActionScopeEnum.AllItems)
            {
                mCallbackArguments.SelectedItems = null;
            }
        }


        /// <summary>
        /// Returns the results of a callback event that targets a control.
        /// </summary>
        /// <returns>The result of the callback.</returns>
        public string GetCallbackResult()
        {
            var selectedAction = mMassActions.SingleOrDefault(action => action.CodeName == mCallbackArguments.ActionCodeName);

            if (selectedAction == null)
            {
                throw new InvalidOperationException(string.Format("Specified mass action ({0}) was not found. Mass actions has to be added on every request (event postback).", mCallbackArguments.ActionCodeName));
            }

            var additionalParameters = AdditionalParameters == null ? null : AdditionalParameters.Value;

            var result = new
            {
                url = selectedAction.CreateUrl(mCallbackArguments.Scope, mCallbackArguments.SelectedItems, additionalParameters),
                type = selectedAction.ActionType, // serializes enum to its internal representation (0, 1)
            };

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(result);
        }

        #endregion
    }
}
