using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PaymentProcessor.Web.Applications
{

    public class PayTraceResponseData
    {
        public string ERROR { get; set; }
        public string APPCODE { get; set; }
        public string TRANSACTIONID { get; set; }
        public string CHECKIDENTIFIER { get; set; }
        public string APPMSG { get; set; }
        public string AVSRESPONSE { get; set; }
        public string CSCRESPONSE { get; set; }
        public string ACHCODE { get; set; }
        public string ACHMSG { get; set; }
        public string RESPONSE { get; set; }
        public string CUSTID { get; set; }
        public string CUSTOMERID { get; set; }
        public string RECURID { get; set; }

        // from SQL table
        public int PayTraceRequestID { get; set; }
        public int DirectTransactionID { get; set; }
        public int CheckTransactionID { get; set; }
        public int RecurringTransactionID { get; set; }
        public int DirectTransactions_EventsID { get; set; }

        public void SetPropertiesByDatString(string Data)
        {
            if (Data.Length > 0)
            {
                Type t = this.GetType();
                string[] d = Data.Split('|');
                for (int i = 0; i < d.Length; i++)
                {
                    string[] nv = d[i].Split('~');
                    if (nv.Length > 1)
                    {
                        PropertyInfo pin = t.GetProperty(nv[0]);
                        if (pin != null)
                        {
                            pin.SetValue(this, nv[1], null);
                        }
                    }
                }
            }
        }

        public bool IsSuccess
        {
            // RESPONSE~160. The customer profile for 6415b8ce-8072-4bcd-8e48-9d7178b826b7/Can Have was successfully created.|CUSTID~6415b8ce-8072-4bcd-8e48-9d7178b826b7|CUSTOMERID~6415b8ce-8072-4bcd-8e48-9d7178b826b7|
            get
            {
                if (RESPONSE != null && RESPONSE.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsError
        {
            // ERROR~171. Please provide a unique customer ID.|
            get
            {
                if (ERROR != null && ERROR.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public PayTraceResponseData(string Data)
        {
            PayTraceRequestID = 0;
            DirectTransactionID = 0;
            CheckTransactionID = 0;
            RecurringTransactionID = 0;
            DirectTransactions_EventsID = 0;
            SetPropertiesByDatString(Data);
        }
    }

}
