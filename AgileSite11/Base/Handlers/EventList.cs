using System;

namespace CMS.Base
{
    using HandlerDictionary = StringSafeDictionary<SimpleHandler>;
    using EventsDictionary = StringSafeDictionary<SafeDictionary<string, SimpleHandler>>;


    /// <summary>
    /// Dynamic event list that allows to register and raise events
    /// </summary>
    public class EventList
    {
        #region "Constants"

        /// <summary>
        /// Global handler key
        /// </summary>
        public const string GLOBAL_ACTION = "##global##";

        #endregion


        #region "Variables"

        /// <summary>
        /// Table of registered events
        /// </summary>
        protected EventsDictionary mEvents = null;


        private bool mIsStatic = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Event list name
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// True, if the event list is static
        /// </summary>
        public bool IsStatic
        {
            get
            {
                return mIsStatic;
            }
            set
            {
                mIsStatic = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the given event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        /// <param name="eventName">Event name</param>
        public void RaiseEvent(object sender, EventArgs e, string eventName)
        {
            RaiseEvent(sender, e, eventName, null);
        }


        /// <summary>
        /// Raises the given component event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        /// <param name="componentName">Component name</param>
        /// <param name="eventName">Event name</param>
        public void RaiseComponentEvent(object sender, EventArgs e, string componentName, string eventName)
        {
            RaiseComponentEvent(sender, e, componentName, eventName, null);
        }


        /// <summary>
        /// Raises the given component event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        /// <param name="componentName">Component name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="actionName">Action name</param>
        public void RaiseComponentEvent(object sender, EventArgs e, string componentName, string eventName, string actionName)
        {
            string eName = string.IsNullOrEmpty(componentName) ? eventName : componentName + ":" + eventName;
            RaiseEvent(sender, e, eName, actionName);
        }


        /// <summary>
        /// Raises the given event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        /// <param name="eventName">Event name</param>
        /// <param name="actionName">Action name</param>
        public void RaiseEvent(object sender, EventArgs e, string eventName, string actionName)
        {
            if (mEvents != null)
            {
                // Get the event handler
                var events = mEvents[eventName];
                if (events != null)
                {
                    // Raise events for all actions
                    if (actionName == null)
                    {
                        foreach (var handler in events.TypedValues)
                        {
                            if (handler != null)
                            {
                                handler.StartEvent(e);
                            }
                        }
                    }
                    else
                    {
                        var handler = events[actionName];
                        if (handler != null)
                        {
                            handler.StartEvent(e);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Registers the given event handler for an event to a specific component by its name
        /// </summary>
        /// <param name="componentName">Component name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="handler">Handler method</param>
        public void RegisterForComponentEvent(string componentName, string eventName, EventHandler<EventArgs> handler)
        {
            RegisterForComponentEvent(componentName, eventName, null, handler);
        }


        /// <summary>
        /// Registers the given event handler for an event to a specific component by its name
        /// </summary>
        /// <param name="componentName">Component name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="actionName">Action name</param>
        /// <param name="handler">Handler method</param>
        public void RegisterForComponentEvent(string componentName, string eventName, string actionName, EventHandler<EventArgs> handler)
        {
            RegisterForComponentEvent<EventArgs>(componentName, eventName, actionName, handler);
        }


        /// <summary>
        /// Registers the given event handler for an event to a specific component by its name
        /// </summary>
        /// <param name="componentName">Component name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="actionName">Action name</param>
        /// <param name="handler">Handler method</param>
        public void RegisterForComponentEvent<ArgsType>(string componentName, string eventName, string actionName, EventHandler<ArgsType> handler) where ArgsType : EventArgs
        {
            if (!string.IsNullOrEmpty(componentName))
            {
                RegisterForEvent<ArgsType>(componentName + ":" + eventName, actionName, handler);
            }
            RegisterForEvent<ArgsType>(eventName, actionName, handler);
        }


        /// <summary>
        /// Registers the given event handler for an event
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="handler">Handler method</param>
        public void RegisterForEvent(string eventName, EventHandler<EventArgs> handler)
        {
            RegisterForEvent(eventName, null, handler);
        }


        /// <summary>
        /// Registers the given event handler for an event
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="actionName">Action name</param>
        /// <param name="handler">Handler method</param>
        public void RegisterForEvent(string eventName, string actionName, EventHandler<EventArgs> handler)
        {
            RegisterForEvent<EventArgs>(eventName, actionName, handler);
        }


        /// <summary>
        /// Registers the given event handler for an event
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="actionName">Action name</param>
        /// <param name="handler">Handler method</param>
        public void RegisterForEvent<ArgsType>(string eventName, string actionName, EventHandler<ArgsType> handler) 
            where ArgsType : EventArgs
        {
            if (mEvents == null)
            {
                mEvents = new EventsDictionary();
            }

            if (mEvents[eventName] == null)
            {
                mEvents[eventName] = new HandlerDictionary();
            }

            // Get action name
            if (actionName == null)
            {
                actionName = GLOBAL_ACTION;
            }

            // Ensure the handler
            var h = mEvents[eventName][actionName];
            if (h == null)
            {
                h = new SimpleHandler
                {
                    Name = Name + "." + eventName,
                    IsStatic = IsStatic
                };
                mEvents[eventName][actionName] = h;
            }

            // Register the event
            h.Execute += (s, e) => handler(s, (ArgsType)e);
        }

        #endregion
    }
}
