using System;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Localization;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Base class for managers
    /// </summary>
    public abstract class CMSAbstractManager<EventArgsType, SimpleEventArgsType> : AbstractUserControl
        where EventArgsType : ManagerEventArgs
        where SimpleEventArgsType : ManagerEventArgs
    {
        #region "Variables"

        private string mResourceCulture;
        private UIContext mUIContext;

        #endregion


        #region "Properties"

        /// <summary>
        /// Control's UI Context
        /// </summary>
        protected UIContext UIContext
        {
            get
            {
                return mUIContext ?? (mUIContext = UIContextHelper.GetUIContext(this));
            }
        }


        /// <summary>
        /// Manager mode (Update, Insert, New culture version)
        /// </summary>
        public FormModeEnum Mode
        {
            get
            {
                object mode = ViewState["Mode"] ?? FormModeEnum.Update;

                return (FormModeEnum)mode;
            }
            set
            {
                ViewState["Mode"] = value;
            }
        }


        /// <summary>
        /// Indicates if document permissions should be checked.
        /// </summary>
        public bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Resource strings culture
        /// </summary>
        public string ResourceCulture
        {
            get
            {
                if (string.IsNullOrEmpty(mResourceCulture))
                {
                    mResourceCulture = HTMLHelper.HTMLEncode(IsLiveSite ? LocalizationContext.PreferredCultureCode : CultureHelper.PreferredUICultureCode);
                }

                return mResourceCulture;
            }
            set
            {
                mResourceCulture = value;
            }
        }


        /// <summary>
        /// Indicates if the data is consistent.
        /// </summary>
        public bool DataConsistent
        {
            get;
            protected set;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// On data validation event handler.
        /// </summary>
        public event EventHandler<EventArgsType> OnValidateData;


        /// <summary>
        /// On check permissions event handler.
        /// </summary>
        public event EventHandler<SimpleEventArgsType> OnCheckPermissions;

        /// <summary>
        /// On check consistency event handler.
        /// </summary>
        public event EventHandler<SimpleEventArgsType> OnCheckConsistency;
        

        /// <summary>
        /// On save data event handler.
        /// </summary>
        public event EventHandler<EventArgsType> OnSaveData;


        /// <summary>
        /// Occurs when saving data fails.
        /// </summary>
        public event EventHandler<EventArgsType> OnSaveFailed;


        /// <summary>
        /// On load data event handler.
        /// </summary>
        public event EventHandler<EventArgsType> OnLoadData;


        /// <summary>
        /// On before action event handler.
        /// </summary>
        public event EventHandler<EventArgsType> OnBeforeAction;


        /// <summary>
        /// On after action event handler.
        /// </summary>
        public event EventHandler<EventArgsType> OnAfterAction;

        #endregion


        #region "Event raising methods"

        /// <summary>
        /// Raises event to validate data
        /// </summary>
        /// <param name="args">Event arguments</param>
        /// <returns>TRUE if data is valid.</returns>
        protected bool RaiseValidateData(EventArgsType args)
        {
            if (OnValidateData != null)
            {
                OnValidateData(this, args);
            }
            else
            {
                ComponentEvents.RequestEvents.RaiseComponentEvent(this, args, ComponentName, DocumentComponentEvents.VALIDATE_DATA, args.ActionName);
            }

            if (!args.IsValid)
            {
                AddError(args.ErrorMessage, null);
            }

            return args.IsValid;
        }


        /// <summary>
        /// Raises event to load data
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected bool RaiseLoadData(EventArgsType args)
        {
            if (OnLoadData != null)
            {
                OnLoadData(this, args);
            }
            else
            {
                ComponentEvents.RequestEvents.RaiseComponentEvent(this, args, ComponentName, DocumentComponentEvents.LOAD_DATA, args.ActionName);
            }

            if (!args.IsValid)
            {
                AddError(args.ErrorMessage, null);
            }

            return args.IsValid;
        }


        /// <summary>
        /// Raises event to check permissions
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected bool RaiseCheckPermissions(SimpleEventArgsType args)
        {
            if (CheckPermissions)
            {
                if (OnCheckPermissions != null)
                {
                    OnCheckPermissions(this, args);

                    if (!args.IsValid && string.IsNullOrEmpty(args.ErrorMessage))
                    {
                        // Add default permission error message
                        args.ErrorMessage = GetDefaultCheckPermissionsError(args);
                    }
                }

                if (args.IsValid && args.CheckDefault)
                {
                    // Check the default security
                    CheckDefaultSecurity(args);
                }
            }

            return args.IsValid;
        }


        /// <summary>
        /// Raises event to check consistency
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected bool RaiseCheckConsistency(SimpleEventArgsType args)
        {
            if (args.IsValid && args.CheckDefault)
            {
                // Check the default consistency
                CheckDefaultConsistency(args);
            }

            if (OnCheckConsistency != null)
            {
                OnCheckConsistency(this, args);
            }

            DataConsistent = args.IsValid;
            return args.IsValid;
        }


        /// <summary>
        /// Gets the default error message for check permission error
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected virtual string GetDefaultCheckPermissionsError(SimpleEventArgsType args)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Checks the default security for the editing context
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected virtual void CheckDefaultSecurity(SimpleEventArgsType args)
        {
            throw new NotImplementedException();
        }
        

        /// <summary>
        /// Checks the default consistency for the editing context
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected virtual void CheckDefaultConsistency(SimpleEventArgsType args)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Raises event to get data to save
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected bool RaiseSaveData(EventArgsType args)
        {
            if (OnSaveData != null)
            {
                OnSaveData(this, args);
            }
            else
            {
                ComponentEvents.RequestEvents.RaiseComponentEvent(this, args, ComponentName, DocumentComponentEvents.SAVE_DATA, args.ActionName);
            }

            if (!args.IsValid)
            {
                AddError(args.ErrorMessage, null);
            }

            return args.IsValid;
        }


        /// <summary>
        /// Raises event when saving data fails
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected void RaiseSaveFailed(EventArgsType args)
        {
            if (OnSaveFailed != null)
            {
                OnSaveFailed(this, args);
            }
        }


        /// <summary>
        /// Raises event before action
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected bool RaiseBeforeAction(EventArgsType args)
        {
            if (OnBeforeAction != null)
            {
                OnBeforeAction(this, args);
            }
            else
            {
                ComponentEvents.RequestEvents.RaiseComponentEvent(this, args, ComponentName, DocumentComponentEvents.BEFORE_ACTION, args.ActionName);
            }

            if (!args.IsValid)
            {
                AddError(args.ErrorMessage, null);
            }

            return args.IsValid;
        }


        /// <summary>
        /// Raises event after action
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected bool RaiseAfterAction(EventArgsType args)
        {
            // Clear settings
            ClearProperties();

            if (OnAfterAction != null)
            {
                OnAfterAction(this, args);
            }
            else
            {
                ComponentEvents.RequestEvents.RaiseComponentEvent(this, args, ComponentName, DocumentComponentEvents.AFTER_ACTION, args.ActionName);
            }

            if (!args.IsValid)
            {
                AddError(args.ErrorMessage, null);
            }

            return args.IsValid;
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Adds additional text
        /// </summary>
        /// <param name="original">Original text</param>
        /// <param name="text">Additional text</param>
        protected string AddText(string original, string text)
        {
            if (!string.IsNullOrEmpty(original))
            {
                original += "<br />";
            }

            return original + text;
        }


        /// <summary>
        /// Clears properties
        /// </summary>
        public virtual void ClearProperties()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}