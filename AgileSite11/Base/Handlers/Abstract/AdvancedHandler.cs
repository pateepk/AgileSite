using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// General CMS handler with one generic parameter
    /// </summary>
    public class AdvancedHandler<TParameter> : AdvancedHandler<AdvancedHandler<TParameter>, CMSEventArgs<TParameter>>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Handled object</param>
        public AdvancedHandler<TParameter> StartEvent(TParameter obj)
        {
            var e = new CMSEventArgs<TParameter>
            {
                Parameter = obj
            };

            return StartEvent(e);
        }
    }


    /// <summary>
    /// General CMS handler
    /// </summary>
    public class AdvancedHandler : AdvancedHandler<AdvancedHandler, CMSEventArgs>
    {
    }


    /// <summary>
    /// Generic handler class
    /// </summary>
    public abstract class AdvancedHandler<THandler, TArgs> : AbstractAdvancedHandler
        where THandler : AdvancedHandler<THandler, TArgs>, new()
        where TArgs : CMSEventArgs, new()
    {
        #region "Variables"

        /// <summary>
        /// If true, the handler supports cancelling of the event
        /// </summary>
        private bool mSupportsCancel = true;


        private TArgs mEventArguments;
        private THandler mParent;

        private bool mAllow = true;
        private bool mContinue = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Flag indicating whether the event was finished or not
        /// </summary>
        protected bool WasFinished
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the event is allowed to be raised
        /// </summary>
        public bool Allow
        {
            get
            {
                return mAllow;
            }
            set
            {
                CheckBase("Allow");

                mAllow = value;
            }
        }


        /// <summary>
        /// Parent handler
        /// </summary>
        public THandler Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                CheckBase("Parent");

                mParent = value;
            }
        }


        /// <summary>
        /// Handler arguments
        /// </summary>
        public TArgs EventArguments
        {
            get
            {
                return mEventArguments;
            }
            protected set
            {
                CheckEvent("EventArguments");

                mEventArguments = value;
            }
        }


        /// <summary>
        /// Specifies whether the context of execution should continue. If set to false, no additional events will be fired.
        /// </summary>
        protected internal sealed override bool Continue
        {
            get
            {
                return mContinue;
            }
            set
            {
                CheckEvent("Continue");

                mContinue = value;
            }
        }


        /// <summary>
        /// If true, the handler supports cancelling of the event. If set and handler is already cancelled, throws an exception.
        /// </summary>
        protected internal sealed override bool SupportsCancel
        {
            get
            {
                return mSupportsCancel;
            }
            set
            {
                CheckEvent("SupportsCancel");

                mSupportsCancel = value;

                if (!value)
                {
                    CheckContinue();
                }
            }
        }


        /// <summary>
        /// Returns true if the handler has some events bound
        /// </summary>
        public bool IsBound
        {
            get
            {
                if ((Volatile.Read(ref mBefore) != null) || (Volatile.Read(ref mAfter) != null) || (Volatile.Read(ref mFailure) != null))
                {
                    return true;
                }

                if (Parent != null)
                {
                    return Parent.IsBound;
                }

                return false;
            }
        }


        /// <summary>
        /// Handlers log item of this handler
        /// </summary>
        protected DataRow LogItem
        {
            get;
            set;
        }

        #endregion


        #region "Events"

        private List<EventHandler<TArgs>> mBefore;
        private List<EventHandler<TArgs>> mAfter;
        private List<EventHandler<TArgs>> mFailure;


        /// <summary>
        /// Raised before the event occurs
        /// </summary>
        public event EventHandler<TArgs> Before
        {
            add
            {
                AddEvent(ref mBefore, value);
            }
            remove
            {
                RemoveEvent(ref mBefore, value);
            }
        }


        /// <summary>
        /// Raised after the event occurs
        /// </summary>
        public event EventHandler<TArgs> After
        {
            add
            {
                AddEvent(ref mAfter, value);

                if (OneTime && WasExecuted)
                {
                    // Execute immediately if one-time event was already executed
                    value(this, null);
                }
            }
            remove
            {
                RemoveEvent(ref mAfter, value);
            }
        }


        /// <summary>
        /// Raised in case the event handler didn't properly call finalization
        /// </summary>
        public event EventHandler<TArgs> Failure
        {
            add
            {
                AddEvent(ref mFailure, value);
            }
            remove
            {
                RemoveEvent(ref mFailure, value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears all bound event handlers from the event and resets the number of executions of the event
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            Volatile.Write(ref mBefore, null);
            Volatile.Write(ref mAfter, null);
            Volatile.Write(ref mFailure, null);
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            try
            {
                if (IsEvent)
                {
                    if (!WasFinished)
                    {
                        // Raise the failure event
                        RaiseFailure(EventArguments);
                    }
                }
            }
            finally
            {
                // Dispose objects used by the event
                EventArguments?.DisposeUsedObjects();

                if (IsEvent)
                {
                    var dr = LogItem;
                    if (dr != null)
                    {
                        HandlersDebug.FinishHandlerOperation(dr);
                        HandlersDebug.SetLogItemImportant(dr);
                    }

                    Parent.CountExecution();
                }
            }

            base.Dispose();
        }


        /// <summary>
        /// Raises the before event
        /// </summary>
        /// <param name="e">Event arguments</param>
        [HideFromDebugContext]
        protected virtual void RaiseBefore(TArgs e)
        {
            // Check if the event is allowed
            if (!Allow)
            {
                return;
            }

            // Raise before event of this handler
            Raise("Before", Volatile.Read(ref mBefore), e);

            // Raise the before event of parent handler
            if (Continue)
            {
                Parent?.RaiseBefore(e);
            }

            if (!SupportsCancel)
            {
                CheckContinue();
            }
        }


        /// <summary>
        /// Raises the after event
        /// </summary>
        /// <param name="e">Event arguments</param>
        [HideFromDebugContext]
        protected void RaiseAfter(TArgs e)
        {
            // Check if the event is allowed
            if (!Allow)
            {
                return;
            }

            if (Continue)
            {
                // Raise after event of parent handler
                Parent?.RaiseAfter(e);

                // Raise after event of this handler
                Raise("After", Volatile.Read(ref mAfter), e);
            }

            if (!SupportsCancel)
            {
                CheckContinue();
            }
        }


        /// <summary>
        /// Raises the failure event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected void RaiseFailure(TArgs e)
        {
            // Check if the event is allowed
            if (!Allow)
            {
                return;
            }

            if (Continue)
            {
                // Raise failure event of parent handler
                Parent?.RaiseFailure(e);

                // Raise failure event of this handler
                Raise("Failure", Volatile.Read(ref mFailure), e);
            }
        }


        /// <summary>
        /// Checks whether the action can continue and if not, fires an exception.
        /// </summary>
        protected void CheckContinue()
        {
            if (!Continue)
            {
                throw new ActionCancelledException("The action was canceled by the handler.");
            }
        }


        /// <summary>
        /// Initiates the new handler context
        /// </summary>
        public THandler StartEvent()
        {
            TArgs e = new TArgs();

            return StartEvent(e);
        }


        /// <summary>
        /// Initiates the new handler context
        /// </summary>
        /// <param name="e">Event arguments</param>
        public THandler StartEvent(EventArgs e)
        {
            TArgs ev = new TArgs();
            ev.OriginalEventArgs = e;

            return StartEvent(ev);
        }


        /// <summary>
        /// Initiates the new handler context
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <param name="allowEvent">If true, it is allowed to raise the event</param>
        public THandler StartEvent(TArgs e, bool allowEvent = true)
        {
            THandler h = null;

            try
            {
                // Initiate the handler
                h = new THandler();

                h.Name = Name;
                h.Allow = allowEvent;
                h.Debug = Debug && HandlersDebug.DebugCurrentRequest;
                h.Parent = (THandler)this;
                h.IsEvent = true;
                h.EventArguments = e;

                if (e != null)
                {
                    e.CurrentHandler = h;
                }

                // Start handler operation
                if (h.Debug)
                {
                    h.LogItem = HandlersDebug.StartHandlerOperation(Name, null);
                }

                // Raise before event
                h.RaiseBefore(e);

                return h;
            }
            catch
            {
                // Dispose the handler object on thread abort, it won't be used in the process
                h?.Dispose();

                throw;
            }
        }


        /// <summary>
        /// Finishes the event and raises the After event actions
        /// </summary>
        protected internal override void Finish()
        {
            RaiseAfter(EventArguments);

            // Call finish actions
            EventArguments?.CallFinishActions();

            WasFinished = true;
        }


        /// <summary>
        /// Sets the parent event of the event
        /// </summary>
        /// <param name="parent">New parent event</param>
        public override void SetParent(AbstractHandler parent)
        {
            Parent = (THandler)parent;
        }

        #endregion


        #region "Conditional events"

        /// <summary>
        /// Adds the conditional before handler
        /// </summary>
        public BeforeConditionalEvent<TArgs> AddBefore()
        {
            CheckBase("AddBefore");

            var ev = new BeforeConditionalEvent<TArgs>();

            Before += ev.GetHandler();

            return ev;
        }


        /// <summary>
        /// Adds the conditional after handler
        /// </summary>
        public ConditionalEvent<TArgs> AddAfter()
        {
            CheckBase("AddAfter");

            var ev = new ConditionalEvent<TArgs>();

            After += ev.GetHandler();

            return ev;
        }


        /// <summary>
        /// Adds the conditional after handler
        /// </summary>
        public ConditionalEvent<TArgs> AddFailure()
        {
            CheckBase("AddFailure");

            var ev = new ConditionalEvent<TArgs>();

            Failure += ev.GetHandler();

            return ev;
        }

        #endregion
    }
}