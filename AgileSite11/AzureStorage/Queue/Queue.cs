using System;

using CMS.Base;

using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Provides operations with azure queue storage.
    /// </summary>
    public class Queue
    {
        #region "Variables"

        CloudQueue mQueue;
        readonly string mQueueName;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new queue object.
        /// </summary>
        /// <param name="name">Queue name.</param>
        public Queue(string name)
        {
            mQueueName = name;
            InitQueue();
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sends message to the queue.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void SendMessage(string message)
        {
            SendMessage(message, null);
        }


        /// <summary>
        /// Sends message to the queue.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="timeToLive">Message time to live.</param>
        public void SendMessage(string message, TimeSpan? timeToLive)
        {
            CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
            if (timeToLive.HasValue)
            {
                mQueue.AddMessage(cloudQueueMessage, timeToLive.Value);
            }
            else
            {
                mQueue.AddMessage(cloudQueueMessage);
            }
        }


        /// <summary>
        /// Gets message from queue and deletes it. If queue is empty returns null.
        /// </summary>
        public string GetMessage()
        {
            // Get message
            TimeSpan span = TimeSpan.FromSeconds(AzureHelper.QUEUE_TIMEOUT);
            CloudQueueMessage cloudQueueMessage = mQueue.GetMessage(span);
            if (cloudQueueMessage != null)
            {
                // Get content
                string message = cloudQueueMessage.AsString;

                // Delete it
                mQueue.DeleteMessage(cloudQueueMessage);

                return message;
            }
            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes queue object.
        /// </summary>
        private void InitQueue()
        {
            if (mQueue == null)
            {
                AccountInfo current = AccountInfo.CurrentAccount;

                // Create objects
                StorageCredentials credentials = new StorageCredentials(current.AccountName, current.SharedKey);
                CloudQueueClient client = new CloudQueueClient(new Uri(current.QueueEndpoint), credentials);

                // Get queue and create if not exist
                mQueue = client.GetQueueReference(mQueueName);
                mQueue.CreateIfNotExists();
            }
        }

        #endregion
    }
}
