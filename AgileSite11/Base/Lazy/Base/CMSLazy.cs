using System;
using System.Collections;
using System.Threading;
using System.Reflection;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Provides lazy initialization
    /// </summary>
    public class CMSLazy<TValue>
    {
        /// <summary>
        /// Underlying data (direct value)
        /// </summary>
        internal CMSLazyData<TValue> mData = new CMSLazyData<TValue>();

        private bool mAllowCloneForNewThread = true;



        /// <summary>
        /// Defines the key under which the object value should be stored in the request stock helper. If set, the object exists only within the particular request
        /// </summary>
        public string RequestStockKey
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the item is copied for the new thread
        /// </summary>
        public bool AllowCloneForNewThread
        {
            get
            {
                return mAllowCloneForNewThread;
            }
            set
            {
                mAllowCloneForNewThread = value;
            }
        }


        /// <summary>
        /// Returns the current data used by the lazy object
        /// </summary>
        internal virtual CMSLazyData<TValue> CurrentData
        {
            get
            {
                if (!String.IsNullOrEmpty(RequestStockKey))
                {
                    return GetDataFromItems();
                }

                // Use in-place value
                return mData;
            }
        }


        /// <summary>
        /// Default value
        /// </summary>
        public TValue DefaultValue
        {
            get;
            protected set;
        }


        /// <summary>
        /// Value initialization function
        /// </summary>
        protected Func<TValue> Initializer
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the value was already created
        /// </summary>
        public bool IsValueCreated
        {
            get
            {
                return CurrentData.IsValueCreated;
            }
        }


        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public virtual TValue Value
        {
            get
            {
                return EnsureValue();
            }
            set
            {
                // Explicit initialization
                var data = CurrentData;

                data.Value = value;
                data.IsValueCreated = true;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initializer">Value initialization function</param>
        /// <param name="defaultValue">Default value</param>
        public CMSLazy(Func<TValue> initializer, TValue defaultValue = default(TValue))
        {
            Initializer = initializer;
            DefaultValue = defaultValue;
        }


        /// <summary>
        /// Gets the data from given request items. If items is null, gets the data from the current thread items.
        /// </summary>
        private CMSLazyData<TValue> GetDataFromItems(IDictionary items = null)
        {
            items = items ?? RequestItems.CurrentItems;

            // Get value from the request items
            var value = (CMSLazyData<TValue>)ItemsFunctions.GetItem(items, RequestStockKey, false);
            if (value == null)
            {
                // Ensure the data
                value = new CMSLazyData<TValue>
                {
                    AllowCloneForNewThread = AllowCloneForNewThread
                };

                ItemsFunctions.Add(items, RequestStockKey, value, false);
            }

            return value;
        }


        /// <summary>
        /// Ensures the internal value of the object
        /// </summary>
        /// <param name="data">Data container in which ensure the data. If null, current data is used</param>
        private TValue EnsureValue(CMSLazyData<TValue> data = null)
        {
            data = data ?? CurrentData;

            if (!data.IsValueCreated)
            {
                // Load only once
                lock (data)
                {
                    if (!data.IsValueCreated)
                    {
                        // Prevent infinite loop
                        if ((data.InitializingFromThreadId > 0) && (data.InitializingFromThreadId == CMSThread.GetCurrentThreadId()))
                        {
                            return DefaultValue;
                        }

                        try
                        {
                            data.InitializingFromThreadId = CMSThread.GetCurrentThreadId();

                            // Implicit initialization
                            if (!CanInitialize())
                            {
                                throw new Exception("[CMSLazy.Value]: Property cannot initialize within this context, the initializer condition is not met.");
                            }

                            data.Value = NewValue();
                            data.IsValueCreated = true;
                        }
                        catch (ThreadAbortException)
                        {
                            // Do not log ThreadAbortException
                            throw;
                        }
                        catch (TargetInvocationException ex)
                        {
                            // Do not log exception in situation where initializer was interrupted by thread abort
                            // Thread abort executed within default constructor initialization cause TargetInvocationException which may break test execution
                            if ((ex.InnerException == null) || !(ex.InnerException is ThreadAbortException))
                            {
                                CoreServices.EventLog.LogException("Lazy", "INIT", ex);
                                throw;
                            }
                        }
                        catch (Exception ex)
                        {
                            CoreServices.EventLog.LogException("Lazy", "INIT", ex);
                            throw;
                        }
                        finally
                        {
                            data.InitializingFromThreadId = 0;
                        }
                    }
                }
            }

            return data.Value;
        }


        /// <summary>
        /// Creates a new value using the initializer
        /// </summary>
        protected virtual TValue NewValue()
        {
            return Initializer();
        }


        /// <summary>
        /// Resets the state of the object to re-initialize the value on the next request
        /// </summary>
        public void Reset()
        {
            CurrentData.Reset();
        }


        /// <summary>
        /// Implicit operator for conversion of the lazy object to its value type
        /// </summary>
        /// <param name="obj">Object to convert</param>
        public static implicit operator TValue(CMSLazy<TValue> obj)
        {
            return obj.Value;
        }


        /// <summary>
        /// Returns true, if the object value can initialize
        /// </summary>
        protected virtual bool CanInitialize()
        {
            return true;
        }
    }
}
