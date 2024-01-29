using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Defines a conditional event
    /// </summary>
    public abstract class ConditionalEventBase<TEvent, TArgs> : IConditionalEvent<TEvent>
        where TArgs : EventArgs
        where TEvent : ConditionalEventBase<TEvent, TArgs>
    {
        #region "Properties"

        /// <summary>
        /// Delegate for the handler condition
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event arguments</param>
        public delegate bool ConditionFunction(object sender, TArgs args);


        /// <summary>
        /// Condition for the handler execution. If set, must return true in order to execute the handler
        /// </summary>
        protected List<ConditionFunction> Conditions
        {
            get;
            set;
        }


        /// <summary>
        /// Handler to execute
        /// </summary>
        protected EventHandler<TArgs> Handler
        {
            get;
            set;
        }


        /// <summary>
        /// Number of repeats of the action
        /// </summary>
        protected int Repeats
        {
            get;
            set;
        }


        /// <summary>
        /// Strongly typed identity
        /// </summary>
        protected TEvent TypedThis
        {
            get
            {
                return (TEvent)this;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected ConditionalEventBase()
        {
            Repeats = Int32.MaxValue;
        }


        /// <summary>
        /// Calls the given handler
        /// </summary>
        /// <param name="handler">Handler to call</param>
        public TEvent Call(Action handler)
        {
            return Call((sender, args) => handler());
        }


        /// <summary>
        /// Calls the given handler
        /// </summary>
        /// <param name="handler">Handler to call</param>
        public TEvent Call(Action<TArgs> handler)
        {
            return Call((sender, args) => handler(args));
        }


        /// <summary>
        /// Calls the given handler
        /// </summary>
        /// <param name="handler">Handler to call</param>
        public TEvent Call(EventHandler<TArgs> handler)
        {
            Handler = handler;

            return TypedThis;
        }


        /// <summary>
        /// Adds the condition to the conditional event
        /// </summary>
        public TEvent When(ConditionFunction condition)
        {
            if (Conditions == null)
            {
                Conditions = new List<ConditionFunction>();
            }

            Conditions.Add(condition);

            return TypedThis;
        }


        /// <summary>
        /// Adds the condition to the conditional event
        /// </summary>
        public TEvent When(Func<TArgs, bool> condition)
        {
            return When((sender, args) => condition(args));
        }


        /// <summary>
        /// Adds the condition to the conditional event
        /// </summary>
        public TEvent When(Func<bool> condition)
        {
            return When((sender, args) => condition());
        }


        /// <summary>
        /// Sets the number of allowed calls to the event
        /// </summary>
        /// <param name="i">Number of allowed calls</param>
        public TEvent Repeat(int i)
        {
            Repeats = i;

            return TypedThis;
        }


        /// <summary>
        /// Checks if the given handler can execute
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event arguments</param>
        private bool CanExecute(object sender, TArgs args)
        {
            // Check repeats (quick evaluation)
            if (Repeats <= 0)
            {
                return false;
            }

            if (Conditions != null)
            {
                // Check all conditions
                foreach (var condition in Conditions)
                {
                    if (!condition(sender, args))
                    {
                        return false;
                    }
                }
            }

            lock (this)
            {
                // Check remaining repeats
                if (Repeats > 0)
                {
                    Repeats--;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Implicit operator to convert to event handler
        /// </summary>
        public EventHandler<TArgs> GetHandler()
        {
            return (sender, args) =>
            {
                if (CanExecute(sender, args))
                {
                    ExecuteHandler(sender, args);
                }
            };
        }


        /// <summary>
        /// Executes the handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event arguments</param>
        protected virtual void ExecuteHandler(object sender, TArgs args)
        {
            Handler(sender, args);
        }


        /// <summary>
        /// Implicit operator to convert to event handler
        /// </summary>
        public static implicit operator EventHandler<TArgs>(ConditionalEventBase<TEvent, TArgs> ev)
        {
            return ev.GetHandler();
        }

        #endregion
    }
}
