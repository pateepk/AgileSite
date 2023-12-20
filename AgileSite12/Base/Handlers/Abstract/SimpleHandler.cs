using System;
using System.Collections.Generic;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// General simple handler with one generic parameter
    /// </summary>
    public class SimpleHandler<TParameter> : SimpleHandler<SimpleHandler<TParameter>, CMSEventArgs<TParameter>>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Handled object</param>
        public CMSEventArgs<TParameter> StartEvent(TParameter obj)
        {
            var e = new CMSEventArgs<TParameter>
            {
                Parameter = obj
            };

            return StartEvent(e);
        }
    }


    /// <summary>
    /// General simple handler
    /// </summary>
    public class SimpleHandler : SimpleHandler<SimpleHandler, EventArgs>
    {
    }


    /// <summary>
    /// Generic handler class
    /// </summary>
    public abstract class SimpleHandler<THandler, TArgs> : AbstractHandler
        where THandler : SimpleHandler<THandler, TArgs>, new()
        where TArgs : EventArgs
    {
        #region "Variables"

        private THandler mParent;
        private TArgs mEventArguments;

        #endregion


        #region "Properties"

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
        /// Returns true if the handler has some events bound
        /// </summary>
        public bool IsBound
        {
            get
            {
                if (Volatile.Read(ref mExecute) != null)
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

        #endregion


        #region "Events"

        /// <summary>
        /// Raised before the event occurs
        /// </summary>
        private List<EventHandler<TArgs>> mExecute;


        /// <summary>
        /// Raised before the event occurs
        /// </summary>
        public event EventHandler<TArgs> Execute
        {
            add
            {
                AddEvent(ref mExecute, value);

                if (OneTime && WasExecuted)
                {
                    // Execute immediately if one-time event was already executed
                    value(this, null);
                }
            }
            remove
            {
                RemoveEvent(ref mExecute, value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the execute event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void RaiseExecute(TArgs e)
        {
            // Raise Execute event of this handler
            var executeLocal = Volatile.Read(ref mExecute);
            if (executeLocal != null)
            {
                AssignCurrentHandler(e);

                Raise("Execute", executeLocal, e, true);
            }

            // Raise the Execute event of parent handler
            if ((Parent != null) && Parent.IsBound)
            {
                Parent.RaiseExecute(e);
            }
        }


        /// <summary>
        /// Initiates the new handler context
        /// </summary>
        /// <param name="e">Event arguments</param>
        public TArgs StartEvent(TArgs e)
        {
            // Check one-time event
            if (OneTime && WasExecuted)
            {
                throw new NotSupportedException($"One-time event {Name} was already executed and cannot be executed again.");
            }

            // Create event child
            var h = new THandler();

            h.Name = Name;
            h.Debug = Debug && HandlersDebug.DebugCurrentRequest;
            h.Parent = (THandler)this;
            h.IsEvent = true;
            h.EventArguments = e;

            // Execute
            h.RaiseExecute(e);

            // Dispose objects allocated by event arguments
            var cmsArgs = e as CMSEventArgs;
            cmsArgs?.DisposeUsedObjects();

            CountExecution();

            return e;
        }


        /// <summary>
        /// Sets the parent event of the event
        /// </summary>
        /// <param name="parent">New parent event</param>
        public override void SetParent(AbstractHandler parent)
        {
            Parent = (THandler)parent;
        }


        /// <summary>
        /// Clears all bound event handlers from the event and resets the number of executions of the event
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            Volatile.Write(ref mExecute, null);
        }

        #endregion
    }
}