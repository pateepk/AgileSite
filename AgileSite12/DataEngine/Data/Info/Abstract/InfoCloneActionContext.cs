using System;
using System.ComponentModel;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Action context used for performance optimization within clone process with enabled synchronization
    /// </summary>
    /// <remarks>
    /// <para>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </para>
    /// <para>
    /// LogObjectChange items are not logged directly to the synchronization queue but temporary queue is used instead.
    /// Temporary queue items are copied to the original queue within dispose.
    /// </para>
    /// </remarks>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class InfoCloneActionContext : AbstractActionContext<InfoCloneActionContext>
    {
        private Action<string, Action, Action<Action>> mSynchronizationTempQueueAction;
        private bool createdWithinCurrentContext;
        InfoCloneSynchronizationTempQueue mSynchronizationTempQueue;

        /// <summary>
        /// Tries to enqueue <paramref name="logObjectChange"/> into temporary queue when cloning optimization is used.
        /// </summary>
        /// <param name="key">Key that ensures unique <paramref name="logObjectChange"/> actions in queue.</param>
        /// <param name="logObjectChange">LogObjectChange action</param>
        /// <param name="originalEnqueAction">Original enqueue action (SynchronizationQueueWorker.Current.Enqueue)</param>
        /// <returns><c>true</c> when temporary queue is set; otherwise <c>false</c></returns>
        public static bool TryEnqueue(string key, Action logObjectChange, Action<Action> originalEnqueAction)
        {
            if (CurrentSynchronizationTempQueueAction != null)
            {
                CurrentSynchronizationTempQueueAction(key, logObjectChange, originalEnqueAction);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Action used instead of original enqueue action
        /// </summary>
        /// <remarks>
        /// Action parameters: Key, LogObjectChange action, Original enqueue action (SynchronizationQueueWorker.Current.Enqueue)
        /// </remarks>
        internal static Action<string, Action, Action<Action>> CurrentSynchronizationTempQueueAction
        {
            get
            {
                return Current.mSynchronizationTempQueueAction;
            }
            private set
            {
                Current.mSynchronizationTempQueueAction = value;
            }
        }


        /// <summary>
        /// Temporary synchronization queue.
        /// </summary>
        private static InfoCloneSynchronizationTempQueue CurrentSynchronizationTempQueue
        {
            get
            {
                return Current.mSynchronizationTempQueue;
            }
            set
            {
                Current.mSynchronizationTempQueue = value;
            }
        }


        /// <summary>
        /// Ensures <see cref="CurrentSynchronizationTempQueueAction"/> value.
        /// </summary>
        internal InfoCloneActionContext UseSynchronizationTempQueue()
        {
            if (CurrentSynchronizationTempQueueAction == null)
            {
                CurrentSynchronizationTempQueueAction = (string key, Action action, Action<Action> originalAction) =>
                {
                    if (CurrentSynchronizationTempQueue == null)
                    {
                        CurrentSynchronizationTempQueue = new InfoCloneSynchronizationTempQueue(originalAction);
                    }

                    CurrentSynchronizationTempQueue.TryAdd(key, action);
                };

                createdWithinCurrentContext = true;
            }

            return this;
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (!createdWithinCurrentContext)
            {
                return;
            }

            try
            {
                CopyToOriginalQueue();
            }
            catch
            {
                // No exception allowed within Dispose. CopyToOriginalQueue calls original enqueue action which is not under control of this class
            }

            CurrentSynchronizationTempQueueAction = null;
            CurrentSynchronizationTempQueue = null;

        }


        private void CopyToOriginalQueue()
        {
            if (CurrentSynchronizationTempQueue == null)
            {
                return;
            }

            foreach (var queueAction in CurrentSynchronizationTempQueue.Items)
            {
                CurrentSynchronizationTempQueue.OriginalQueueAction(queueAction);
            }
        }
    }
}
