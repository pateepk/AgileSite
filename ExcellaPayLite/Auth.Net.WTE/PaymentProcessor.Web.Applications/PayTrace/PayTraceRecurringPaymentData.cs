using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PaymentProcessor.Web.Applications
{

    public class PayTraceRecurringPaymentDataArray
    {
        public PayTraceRecurringPaymentData[] data { get; set; }
        public int PayTraceRequestID { get; set; }

        public void SetPropertiesByDatString(string Data)
        {
            data = new PayTraceRecurringPaymentData[0];

            if (Data.Length > 0)
            {
                List<PayTraceRecurringPaymentData> list = new List<PayTraceRecurringPaymentData>();

                Type t = this.GetType();
                string[] d = Data.Split('|'); // spliter is |
                for (int i = 0; i < d.Length; i++)
                {
                    d[i] = d[i].Trim();
                    if (d[i].Length > 0)
                    {
                        PayTraceRecurringPaymentData nd = new PayTraceRecurringPaymentData(d[i]);
                        list.Add(nd);
                    }
                }
                data = list.ToArray();
            }
        }

        public PayTraceRecurringPaymentDataArray(string Data)
        {
            //RECURRINGPAYMENT~RECURID=731110+AMOUNT=1.11+CUSTID=7471505d-7480-46fe-a3db-55dff8362b74+NEXT=9/16/2016+TOTALCOUNT=1+CURRENTCOUNT=0+REPEAT=0+DESCRIPTION=+|RECURRINGPAYMENT~RECURID=731122+AMOUNT=1.12+CUSTID=7471505d-7480-46fe-a3db-55dff8362b74+NEXT=9/17/2016+TOTALCOUNT=5+CURRENTCOUNT=0+REPEAT=0+DESCRIPTION=123+|
            SetPropertiesByDatString(Data);
        }
    }

    public class PayTraceRecurringPaymentData
    {
        public string RECURID { get; set; }
        public string AMOUNT { get; set; }
        public string CUSTID { get; set; }
        public string NEXT { get; set; }
        public string TOTALCOUNT { get; set; }
        public string CURRENTCOUNT { get; set; }
        public string REPEAT { get; set; }
        public string DESCRIPTION { get; set; }

        public void SetPropertiesByDatString(string Data)
        {
            if (Data.Length > 0)
            {
                // the spliters are different here
                Type t = this.GetType();
                string[] d = Data.Split('+'); // spliter is +
                for (int i = 0; i < d.Length; i++)
                {
                    string[] nv = d[i].Split('='); // value pair is =
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

        public PayTraceRecurringPaymentData(string Data)
        {
            string prefix = "RECURRINGPAYMENT~";
            // RECURRINGPAYMENT~RECURID=731110+AMOUNT=1.11+CUSTID=7471505d-7480-46fe-a3db-55dff8362b74+NEXT=9/16/2016+TOTALCOUNT=1+CURRENTCOUNT=0+REPEAT=0+DESCRIPTION=+|RECURRINGPAYMENT~RECURID=731122+AMOUNT=1.12+CUSTID=7471505d-7480-46fe-a3db-55dff8362b74+NEXT=9/17/2016+TOTALCOUNT=5+CURRENTCOUNT=0+REPEAT=0+DESCRIPTION=123+|
            if (Data.StartsWith(prefix))
            {
                Data = Data.Substring(prefix.Length);
            }
            SetPropertiesByDatString(Data);
        }

    }
}
