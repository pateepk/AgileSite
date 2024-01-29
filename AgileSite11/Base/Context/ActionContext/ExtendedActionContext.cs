using System;

namespace CMS.Base
{
    /// <summary>
    /// Base class for the extended action context
    /// </summary>
    public abstract class ExtendedActionContext<TContext> : AbstractActionContext<TContext>
        where TContext : ExtendedActionContext<TContext>, new()
    {
        #region "Variables"

        private CMSActionContext mActionContext;

        #endregion


        #region "Properties"

        /// <summary>
        /// Action context
        /// </summary>
        public CMSActionContext ActionContext
        {
            get
            {
                return mActionContext ?? (mActionContext = new CMSActionContext() { RestoreOriginal = RestoreOriginal });
            }
            set
            {
                mActionContext = value;

                if (value != null)
                {
                    value.RestoreOriginal = RestoreOriginal;
                }
            }
        }


        /// <summary>
        /// User of the context.
        /// </summary>
        public IUserInfo User
        {
            set
            {
                ActionContext.User = value;
            }
        }


        /// <summary>
        /// Indicates if the staging tasks should be logged.
        /// </summary>
        public bool LogSynchronization
        {
            set
            {
                ActionContext.LogSynchronization = value;
            }
        }


        /// <summary>
        /// Indicates if the integration tasks should be logged.
        /// </summary>
        public bool LogIntegration
        {
            set
            {
                ActionContext.LogIntegration = value;
            }
        }


        /// <summary>
        /// Indicates whether logging through log context (LogContext) is currently enabled.
        /// </summary>
        public bool EnableLogContext
        {
            set
            {
                ActionContext.EnableLogContext = value;
            }
        }


        /// <summary>
        /// Indicates if the events should be logged.
        /// </summary>
        public bool LogEvents
        {
            set
            {
                ActionContext.LogEvents = value;
            }
        }


        /// <summary>
        /// Indicates if the web farm tasks should be logged.
        /// </summary>
        public bool LogWebFarmTasks
        {
            set
            {
                ActionContext.LogWebFarmTasks = value;
            }
        }


        /// <summary>
        /// Indicates if the objects cache dependencies should be touched.
        /// </summary>
        public bool TouchCacheDependencies
        {
            set
            {
                ActionContext.TouchCacheDependencies = value;
            }
        }


        /// <summary>
        /// Indicates if the objects time stamps should be updated.
        /// </summary>
        public bool UpdateTimeStamp
        {
            set
            {
                ActionContext.UpdateTimeStamp = value;
            }
        }


        /// <summary>
        /// Indicates if the search task should be created for the objects.
        /// </summary>
        public bool CreateSearchTask
        {
            set
            {
                ActionContext.CreateSearchTask = value;
            }
        }


        /// <summary>
        /// Indicates if the export tasks should be logged.
        /// </summary>
        public bool LogExport
        {
            set
            {
                ActionContext.LogExport = value;
            }
        }

        /// <summary>
        /// Indicates if the object version should be created.
        /// </summary>
        public bool CreateVersion
        {
            set
            {
                ActionContext.CreateVersion = value;
            }
        }


        /// <summary>
        /// Indicates if the actions can run asynchronously. Default is true, if the application is web site, otherwise false.
        /// </summary>
        public bool AllowAsyncActions
        {
            set
            {
                ActionContext.AllowAsyncActions = value;
            }
        }


        /// <summary>
        /// Guid of the current CMSTread. Guid.Empty when execution is not running under CMSThread.
        /// </summary>
        public Guid ThreadGuid
        {
            set
            {
                ActionContext.ThreadGuid = value;
            }
        }


        /// <summary>
        /// Indicates if the notifications should be sent.
        /// </summary>
        public bool SendNotifications
        {
            set
            {
                ActionContext.SendNotifications = value;
            }
        }


        /// <summary>
        /// Indicates if the system fields should be updated.
        /// </summary>
        public bool UpdateSystemFields
        {
            set
            {
                ActionContext.UpdateSystemFields = value;
            }
        }


        /// <summary>
        /// Indicates if cache should be used for data operations.
        /// </summary>
        public bool UseCacheForSynchronizationXMLs
        {
            set
            {
                ActionContext.UseCacheForSynchronizationXMLs = value;
            }
        }


        /// <summary>
        /// Indicates if the current user should be initialized.
        /// </summary>
        public bool AllowInitUser
        {
            set
            {
                ActionContext.AllowInitUser = value;
            }
        }


        /// <summary>
        /// Indicates if the objects within current context should behave as disconnected
        /// </summary>
        public bool Disconnected
        {
            set
            {
                ActionContext.Disconnected = value;
            }
        }


        /// <summary>
        /// Indicates if the objects within current context should touch parent object
        /// </summary>
        public bool TouchParent
        {
            set
            {
                ActionContext.TouchParent = value;
            }
        }


        /// <summary>
        /// Indicates if email should be sent.
        /// </summary>
        public bool SendEmails
        {
            set
            {
                ActionContext.SendEmails = value;
            }
        }


        /// <summary>
        /// Indicates if global admin context should be used.
        /// </summary>
        public bool UseGlobalAdminContext
        {
            set
            {
                ActionContext.UseGlobalAdminContext = value;
            }
        }


        /// <summary>
        /// Indicates whether a search task should be executed when it is created.
        /// </summary>
        public bool EnableSmartSearchIndexer
        {
            set
            {
                ActionContext.EnableSmartSearchIndexer = value;
            }
        }


        /// <summary>
        /// Indicates whether a physical files should be deleted for attachments, meta files and media files.
        /// </summary>
        public bool DeletePhysicalFiles
        {
            set
            {
                ActionContext.DeletePhysicalFiles = value;
            }
        }


        /// <summary>
        /// Indicates whether a user activity points should be updated.
        /// </summary>
        public bool UpdateUserCounts
        {
            set
            {
                ActionContext.UpdateUserCounts = value;
            }
        }


        /// <summary>
        /// Indicates whether a user activity points should be updated.
        /// </summary>
        public bool UpdateRating
        {
            set
            {
                ActionContext.UpdateRating = value;
            }
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            // Dispose hosted action context
            if (mActionContext != null)
            {
                mActionContext.Dispose();
            }

            base.Dispose();
        }


        /// <summary>
        /// Disables logging for synchronization, event log, web farm tasks, versioning and cache dependencies 
        /// Disables timestamp update 
        /// Disables object invalidation
        /// Disables cache usage
        /// Disables asynchronous actions
        /// Disables sending of emails
        /// </summary>
        public void DisableAll()
        {
            ActionContext.DisableAll();
        }


        /// <summary>
        /// Disables logging of event log, staging and export tasks
        /// </summary>
        public void DisableLogging()
        {
            ActionContext.DisableLogging();
        }

        #endregion
    }
}
