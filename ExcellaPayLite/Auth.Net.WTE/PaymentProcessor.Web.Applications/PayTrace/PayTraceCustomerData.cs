using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PaymentProcessor.Web.Applications
{
    public class PayTraceCustomerDataArray
    {
        public PayTraceCustomerData[] data { get; set;}

        public void SetPropertiesByDatString(string Data)
        {
            data = new PayTraceCustomerData[0];

            if (Data.Length > 0)
            {
                List<PayTraceCustomerData> list = new List<PayTraceCustomerData>();

                Type t = this.GetType();
                string[] d = Data.Split('|'); // spliter is |
                for (int i = 0; i < d.Length; i++)
                {
                    d[i] = d[i].Trim();
                    if(d[i].Length>0)
                    {
                        PayTraceCustomerData nd = new PayTraceCustomerData(d[i]);
                        list.Add(nd);
                    }
                }
                data = list.ToArray();
            }
        }

        public PayTraceCustomerDataArray(string Data)
        {
            //CUSTOMERRECORD~CUSTID=9999+CUSTOMERID=9999+CC=************5454+EXPMNTH=04+EXPYR=17+SNAME=+SADDRESS=+SADDRESS2=+SCITY=+SCOUNTY=+SSTATE=+SZIP=+SCOUNTRY=US+BNAME=Joe Schmoe+BADDRESS=123 Main St.+BADDRESS2=+BCITY=+BSTATE=+BZIP=90623+BCOUNTRY=US+EMAIL=+PHONE=+FAX=+WHEN=10/4/2013 11:37:40 AM+USER=demo123+IP=216.32.51.8+|CUSTOMERRECORD~CUSTID=11373677+CUSTOMERID=11373677+CC=************4242+EXPMNTH=06+EXPYR=17+SNAME=+SADDRESS=+SADDRESS2=+SCITY=+SCOUNTY=+SSTATE=+SZIP=+SCOUNTRY=US+BNAME=Test Person+BADDRESS=+BADDRESS2=+BCITY=+BSTATE=+BZIP=85284+BCOUNTRY=US+EMAIL=+PHONE=+FAX=+WHEN=8/17/2016 11:22:40 PM+USER=demo123+IP=70.164.97.162+|CUSTOMERRECORD~CUSTID=11373678+CUSTOMERID=11373678+CC=************+EXPMNTH=+EXPYR=+SNAME=+SADDRESS=+SADDRESS2=+SCITY=+SCOUNTY=+SSTATE=+SZIP=+SCOUNTRY=US+BNAME=John Doe+BADDRESS=+BADDRESS2=+BCITY=+BSTATE=+BZIP=85284+BCOUNTRY=US+EMAIL=+PHONE=+FAX=+WHEN=8/17/2016 11:22:46 PM+USER=demo123+IP=70.164.97.162+DDA=1234567890+TR=325070760+|
            SetPropertiesByDatString(Data);
        }
    }

    public class PayTraceCustomerData
    {

        public string CUSTID { get; set; }
        public string CUSTOMERID { get; set; }
        public string CC { get; set; }
        public string EXPMNTH { get; set; }
        public string EXPYR { get; set; }
        public string SNAME { get; set; }
        public string SADDRESS { get; set; }
        public string SADDRESS2 { get; set; }
        public string SCITY { get; set; }
        public string SCOUNTY { get; set; }
        public string SSTATE { get; set; }
        public string SZIP { get; set; }
        public string SCOUNTRY { get; set; }
        public string BNAME { get; set; }
        public string BADDRESS { get; set; }
        public string BADDRESS2 { get; set; }
        public string BCITY { get; set; }
        public string BSTATE { get; set; }
        public string BZIP { get; set; }
        public string BCOUNTRY { get; set; }
        public string EMAIL { get; set; }
        public string PHONE { get; set; }
        public string FAX { get; set; }
        public string WHEN { get; set; }
        public string USER { get; set; }
        public string IP { get; set; }

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

        public PayTraceCustomerData(string Data)
        {
            string prefix = "CUSTOMERRECORD~";
            // CUSTOMERRECORD~CUSTID=9999+CUSTOMERID=9999+CC=************5454+EXPMNTH=04+EXPYR=17+SNAME=+SADDRESS=+SADDRESS2=+SCITY=+SCOUNTY=+SSTATE=+SZIP=+SCOUNTRY=US+BNAME=Joe Schmoe+BADDRESS=123 Main St.+BADDRESS2=+BCITY=+BSTATE=+BZIP=90623+BCOUNTRY=US+EMAIL=+PHONE=+FAX=+WHEN=10/4/2013 11:37:40 AM+USER=demo123+IP=216.32.51.8+|CUSTOMERRECORD~CUSTID=11373677+CUSTOMERID=11373677+CC=************4242+EXPMNTH=06+EXPYR=17+SNAME=+SADDRESS=+SADDRESS2=+SCITY=+SCOUNTY=+SSTATE=+SZIP=+SCOUNTRY=US+BNAME=Test Person+BADDRESS=+BADDRESS2=+BCITY=+BSTATE=+BZIP=85284+BCOUNTRY=US+EMAIL=+PHONE=+FAX=+WHEN=8/17/2016 11:22:40 PM+USER=demo123+IP=70.164.97.162+
            if (Data.StartsWith(prefix))
            {
                Data = Data.Substring(prefix.Length);
            }
            SetPropertiesByDatString(Data);
        }

    }
}
