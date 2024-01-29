using System;
using System.Web;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

using CMS.EventLog;

namespace CMS.WebServices
{
    /// <summary>
    /// Error handler for the REST requests.
    /// </summary>
    public class RESTErrorHandler : Attribute, IServiceBehavior, IErrorHandler
    {
        #region IServiceBehavior Members

        /// <summary>
        /// Service type.
        /// </summary>
        protected Type ServiceType
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes the error handler.
        /// </summary>
        /// <param name="description">Service description</param>
        /// <param name="host">Service host</param>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
        {
            ServiceType = description.ServiceType;
            foreach (var dispatcher in host.ChannelDispatchers.OfType<ChannelDispatcher>())
            {
                dispatcher.ErrorHandlers.Add(this);
            }
        }


        /// <summary>
        /// Not needed, remains empty, does nothing.
        /// </summary>
        /// <param name="serviceDescription">Operation description</param>
        /// <param name="serviceHostBase">Service host</param>
        /// <param name="endpoints">Endpoints</param>
        /// <param name="bindingParameters">Binding parameters</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }


        /// <summary>
        /// Not needed, remains empty, does nothing.
        /// </summary>
        /// <param name="serviceDescription">Service description</param>
        /// <param name="serviceHostBase">Service host</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion


        #region IErrorHandler Members

        /// <summary>
        /// Error handler. Logs the exception to the EventLog
        /// </summary>
        /// <param name="error">Exception</param>
        public bool HandleError(Exception error)
        {
            try
            {
                if (HttpContext.Current == null)
                {
                    EventLogProvider.LogException("REST Service", "REST", error);
                }
                else
                {
                    // The HandleError method can be called from various threads 
                    // but is able to share HttpContext.Current.Items collection.
                    // This behavior violates rules applied for ContextContainers because
                    // the Items collection is considered uniquely per each thread in ContextContainer.
                    // New thread ensures that nothing is shared with any other thread.

                    Task.Run(() => EventLogProvider.LogException("REST Service", "REST", error));
                }
            }
            catch
            {
                // HandleError cannot produce any error
            }
            return false;
        }


        /// <summary>
        /// Not needed. Does nothing.
        /// </summary>
        /// <param name="error">Exception</param>
        /// <param name="version">Message version</param>
        /// <param name="fault">Fault message</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
        }

        #endregion
    }
}