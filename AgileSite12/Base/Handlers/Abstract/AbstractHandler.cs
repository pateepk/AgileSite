using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// Base class for handler classes
    /// </summary>
    public abstract class AbstractHandler : IDisposable
    {
        #region "Variables"

        /// <summary>
        /// Number of executions of the event
        /// </summary>
        private int mExecutions;

        private bool mOneTime;
        private bool mControlRecursion = true;

        private string mName;
        private bool mIsStatic = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Event name. The name serves for debug purposes and to uniquely identify the handler.
        /// </summary>
        public string Name
        {
            get
            {
                return mName ?? (mName = Guid.NewGuid().ToString());
            }
            set
            {
                mName = value;
            }
        }


        /// <summary>
        /// If true, the recursion control is enabled on this handler.
        /// </summary>
        public bool ControlRecursion
        {
            get
            {
                return mControlRecursion;
            }
            set
            {
                CheckBase("ControlRecursion");

                mControlRecursion = value;
            }
        }


        /// <summary>
        /// Returns true if the handler instance is representing an executed event. Returns false in case of static handler base.
        /// </summary>
        internal bool IsEvent
        {
            get;
            set;
        }


        /// <summary>
        /// If true, this event can be executed only once. If the event was already executed, and new handler is added, it executes immediately.
        /// </summary>
        public bool OneTime
        {
            get
            {
                return mOneTime;
            }
            set
            {
                CheckBase("OneTime");

                mOneTime = value;
            }
        }


        /// <summary>
        /// Returns true if the event was already executed
        /// </summary>
        public bool WasExecuted
        {
            get
            {
                return (mExecutions > 0);
            }
        }


        /// <summary>
        /// If true, the handler is included in debug
        /// </summary>
        public bool Debug
        {
            get;
            set;
        } = true;


        /// <summary>
        /// True, if the handler is a static handler
        /// </summary>
        public bool IsStatic
        {
            get
            {
                return !IsEvent && mIsStatic;
            }
            set
            {
                mIsStatic = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Assigns the current handler to the event arguments
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected void AssignCurrentHandler(EventArgs e)
        {
            var args = e as CMSEventArgs;
            if (args != null)
            {
                args.CurrentHandler = this;
            }
        }


        /// <summary>
        /// Checks if the handler is base handler instance, and fires an exception if not
        /// </summary>
        /// <param name="member">Member that is called</param>
        protected void CheckBase(string member)
        {
            if (IsEvent)
            {
                throw new NotSupportedException("[AdvancedHandler." + member + "]: This action can be done only with non-event event handler base.");
            }
        }


        /// <summary>
        /// Checks if the handler is event handler instance, and fires an exception if not
        /// </summary>
        /// <param name="member">Member that is called</param>
        protected void CheckEvent(string member)
        {
            if (!IsEvent)
            {
                throw new NotSupportedException("[AdvancedHandler." + member + "]: This action can be done only with event instance handler.");
            }
        }


        /// <summary>
        /// Counts the execution of the event
        /// </summary>
        protected void CountExecution()
        {
            // Count executions
            Interlocked.Increment(ref mExecutions);
        }


        /// <summary>
        /// Removes the event from the list
        /// </summary>
        /// <param name="list">List of events</param>
        /// <param name="h">Event to remove</param>
        protected void RemoveEvent<THandler>(ref List<THandler> list, THandler h)
        {
            List<THandler> newList, initialList;
            do
            {
                initialList = Volatile.Read(ref list);
                newList = new List<THandler>(initialList ?? Enumerable.Empty<THandler>());
                newList.Remove(h);
            } while (Interlocked.CompareExchange(ref list, newList, initialList) != initialList);
        }


        /// <summary>
        /// Adds the event to the list
        /// </summary>
        /// <param name="list">List of events</param>
        /// <param name="h">Event to add</param>
        protected void AddEvent<THandler>(ref List<THandler> list, THandler h)
        {
            List<THandler> newList, initialList;
            do
            {
                initialList = Volatile.Read(ref list);
                newList = new List<THandler>(initialList ?? Enumerable.Empty<THandler>());
                newList.Add(h);
            } while (Interlocked.CompareExchange(ref list, newList, initialList) != initialList);
        }


        /// <summary>
        /// Raises the list of events
        /// </summary>
        /// <param name="partName">Name of the part executing the handler</param>
        /// <param name="list">List of events to raise</param>
        /// <param name="e">Event arguments</param>
        /// <param name="important">If true, the event if considered important</param>
        [HideFromDebugContext]
        protected void Raise<TArgs>(string partName, List<EventHandler<TArgs>> list, TArgs e, bool important = false)
            where TArgs : EventArgs
        {
            if (list == null)
            {
                return;
            }

            // Start handler operation
            int called = 0;

            DataRow dr = null;

            try
            {
                dr = Debug ? HandlersDebug.StartHandlerOperation(Name, partName) : null;

                // Control the recursion if enabled, has name (to be able to build recursion key) and handler can provide recursion key
                if (ControlRecursion && !String.IsNullOrEmpty(Name))
                {
                    var controlled = this as IRecursionControlHandler<TArgs>;
                    var key = controlled?.GetRecursionKey(e);

                    if (!String.IsNullOrEmpty(key))
                    {
                        // Prepare the key base for particular events
                        key = $"{Name}.{partName}|{key}|";

                        for (int i = 0; i < list.Count; i++)
                        {
                            var h = list[i];
                            if (h != null)
                            {
                                // Control each event handler separately to avoid only that one, the key is "name|index|recursionKey"
                                var hKey = key + i;

                                // Use controlled section to control the recursion
                                using (var rc = new RecursionControl(hKey))
                                {
                                    if (rc.Continue)
                                    {
                                        CallEventHandler(h, e);

                                        called++;
                                    }
                                    else if (Debug)
                                    {
                                        // Log prevented call
                                        HandlersDebug.LogHandlerOperation(h.Method, false);
                                    }
                                }
                            }
                        }

                        return;
                    }
                }

                foreach (EventHandler<TArgs> handler in list)
                {
                    if (handler != null)
                    {
                        // Raise the event
                        CallEventHandler(handler, e);

                        called++;
                    }
                }
            }
            finally
            {
                // Finish handler operation
                if (dr != null)
                {
                    HandlersDebug.FinishHandlerOperation(dr, called);

                    if (important)
                    {
                        HandlersDebug.SetLogItemImportant(dr);
                    }
                }
            }
        }


        /// <summary>
        /// Calls the event handler
        /// </summary>
        /// <param name="h">Handler to call</param>
        /// <param name="e">Event arguments</param>
        [HideFromDebugContext]
        private void CallEventHandler<TArgs>(EventHandler<TArgs> h, TArgs e)
            where TArgs : EventArgs
        {
            DataRow dr = null;

            try
            {
                dr = Debug && HandlersDebug.DebugCurrentRequest && DebugHelper.CanDebug(h.Method) ? HandlersDebug.StartHandlerOperation(h.Method) : null;

                // Raise the event if entry is allowed
                h(this, e);
            }
            finally
            {
                if (dr != null)
                {
                    HandlersDebug.FinishHandlerOperation(dr);
                }
            }
        }


        /// <summary>
        /// Sets the parent event of the event
        /// </summary>
        /// <param name="parent">New parent event</param>
        public abstract void SetParent(AbstractHandler parent);


        /// <summary>
        /// Clears all bound event handlers from the event and resets the number of executions of the event
        /// </summary>
        public virtual void Clear()
        {
            Reset();
        }


        /// <summary>
        /// Resets the number of executions of the event
        /// </summary>
        public virtual void Reset()
        {
            CheckBase("Reset");

            mExecutions = 0;
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion
    }
}
