using System;
using System.Linq;
using System.Reflection;

namespace CMS.SalesForce
{

    internal abstract class SalesForceClientCommand
    {

        #region "Members"

        protected Session mSession;
        protected WebServiceClient.Soap mClient;

        #endregion

        #region "Constructors"

        public SalesForceClientCommand(Session session, WebServiceClient.Soap client)
        {
            mSession = session;
            mClient = client;
        }

        #endregion

        #region "Methods"

        protected void PrepareRequestHeaders(object request, SalesForceClientOptions options)
        {
            FieldInfo[] fields = request.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo transactionField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.AllOrNoneHeader)));
            if (transactionField != null)
            {
                WebServiceClient.AllOrNoneHeader header = new WebServiceClient.AllOrNoneHeader
                {
                    allOrNone = options.TransactionEnabled
                };
                transactionField.SetValue(request, header);
            }
            FieldInfo attributeTruncationField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.AllowFieldTruncationHeader)));
            if (attributeTruncationField != null)
            {
                WebServiceClient.AllowFieldTruncationHeader header = new WebServiceClient.AllowFieldTruncationHeader
                {
                    allowFieldTruncation = options.AttributeTruncationEnabled
                };
                attributeTruncationField.SetValue(request, header);
            }
            FieldInfo feedTrackingField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.DisableFeedTrackingHeader)));
            if (feedTrackingField != null)
            {
                WebServiceClient.DisableFeedTrackingHeader header = new WebServiceClient.DisableFeedTrackingHeader
                {
                    disableFeedTracking = !options.FeedTrackingEnabled
                };
                feedTrackingField.SetValue(request, header);
            }
            FieldInfo mruUpdateField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.MruHeader)));
            if (mruUpdateField != null)
            {
                WebServiceClient.MruHeader header = new WebServiceClient.MruHeader
                {
                    updateMru = options.MruUpdateEnabled
                };
                mruUpdateField.SetValue(request, header);
            }
            if (!String.IsNullOrEmpty(options.ClientName) || !String.IsNullOrEmpty(options.DefaultNamespace))
            {
                FieldInfo clientField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.CallOptions)));
                if (clientField != null)
                {
                    WebServiceClient.CallOptions header = new WebServiceClient.CallOptions
                    {
                        client = options.ClientName,
                        defaultNamespace = options.DefaultNamespace
                    };
                    clientField.SetValue(request, header);
                }
            }
            if (!String.IsNullOrEmpty(options.CultureName))
            {
                FieldInfo cultureField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.LocaleOptions)));
                if (cultureField != null)
                {
                    WebServiceClient.LocaleOptions header = new WebServiceClient.LocaleOptions
                    {
                        language = options.CultureName
                    };
                    cultureField.SetValue(request, header);
                }
            }
            if (options.BatchSize > 0)
            {
                FieldInfo batchField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.QueryOptions)));
                if (batchField != null)
                {
                    WebServiceClient.QueryOptions header = new WebServiceClient.QueryOptions
                    {
                        batchSize = options.BatchSize,
                        batchSizeSpecified = true
                    };
                    batchField.SetValue(request, header);
                }
            }
            FieldInfo sessionField = fields.SingleOrDefault(x => x.FieldType.Equals(typeof(WebServiceClient.SessionHeader)));
            if (sessionField != null)
            {
                WebServiceClient.SessionHeader header = new WebServiceClient.SessionHeader
                {
                    sessionId = mSession.AccessToken
                };
                sessionField.SetValue(request, header);
            }
        }

        #endregion

    }

}