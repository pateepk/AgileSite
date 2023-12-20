using System;
using System.Collections.Generic;
using System.Linq;

using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class representing action to send e-mail
    /// </summary>
    public class EmailAction : DocumentWorkflowAction
    {
        #region "Properties"

        /// <summary>
        /// Recipients of e-mail separated by ';' or ','.
        /// </summary>
        protected virtual string Recipients
        {
            get
            {
                return GetResolvedParameter<string>("to", "");
            }
        }


        /// <summary>
        /// Template of e-mail.
        /// </summary>
        protected virtual string TemplateName
        {
            get
            {
                return GetResolvedParameter<string>("emailtemplate", "");
            }
        }


        /// <summary>
        /// Sender of e-mail.
        /// </summary>
        protected virtual string Sender
        {
            get
            {
                return GetResolvedParameter<string>("from", "");
            }
        }


        /// <summary>
        /// Subject specifically defined for step.
        /// </summary>
        protected virtual string Subject
        {
            get
            {
                return GetResolvedParameter<string>("subject", "");
            }
        }


        /// <summary>
        /// E-mail body specifically defined for step.
        /// </summary>
        protected virtual string Body
        {
            get
            {
                return GetResolvedParameter<string>("body", "");
            }
        }


        /// <summary>
        /// Whether is e-mail based or template or not 
        /// </summary>
        protected virtual bool BasedOnTemplate
        {
            get
            {
                return GetResolvedParameter<int>("basedon", 0) == 0;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            // Prepare recipients
            List<string> recipients = Recipients.Split(new char[] { ';', ',', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim())
                                     .ToList<string>();

            WorkflowEmailSettings settings = new WorkflowEmailSettings(User)
            {
                Resolver = MacroResolver,
                LogEvents = true,
                SiteName = InfoObject.Generalized.ObjectSiteName,
                Sender = Sender,
                Recipients = recipients
            };

            if (BasedOnTemplate)
            {
                settings.EmailTemplateName = TemplateName;
            }
            else
            {
                settings.Body = Body;
                settings.Subject = Subject;
            }

            Manager.SendWorkflowEmails(settings);
        }

        #endregion
    }
}
