using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PaymentProcessor.Web.Applications
{

    public class PayTraceRequestData
    {
        public const string PARMLIST = "PARMLIST";

        //    String sRequest = "PARMLIST=" + Server.UrlEncode("UN~" + ConfigurationManager.AppSettings["username"] +
        //"|PSWD~" + ConfigurationManager.AppSettings["password"] + "|TERMS~Y" +
        //"|TRANXTYPE~Sale|INVOICE~1234" + "|BNAME~" + Request.Form["name"] + "|BADDRESS~" + Request.Form["address1"] +
        //"|BADDRESS2~" + Request.Form["address2"] + "|BCITY~" + Request.Form["city"] +
        //"|BSTATE~" + Request.Form["state"] + "|BZIP~" + Request.Form["zip"] +
        //"|AMOUNT~" + Request.Form["amount"] + "|PHONE~" + Request.Form["phone"] +
        //"|EMAIL~" + Request.Form["email"]);

        // Main Data
        public string UN { get; set; }              // Username
        public string PSWD { get; set; }            // Password
        public string TERMS { get; set; }           // Terms: Y
        public string TRANXTYPE { get; set; }       // Trans Type: Sale (for Credit Card)
        public string TRANXID { get; set; }         // for refund 
        public string CHECKTYPE { get; set; }       // Check Type: Sale (for Check)  
        public string INVOICE { get; set; }         // Invoice Number (internal ID)
        public string BNAME { get; set; }           // Billing Name
        public string BADDRESS { get; set; }        // Billing Address
        public string BADDRESS2 { get; set; }       // Billing Address2
        public string BCITY { get; set; }           // Billing City
        public string BSTATE { get; set; }          // Billing State
        public string BZIP { get; set; }            // Billing Zip
        public string PHONE { get; set; }           // Phone
        public string EMAIL { get; set; }           // Email
        public string AMOUNT { get; set; }          // Amount in String
        public string TEST { get; set; }             // Testing Mode: Y

        public string SNAME { get; set; } 
        public string SADDRESS { get; set; }
        public string SCITY { get; set; }
        public string SSTATE { get; set; }
        public string SZIP { get; set; }

        // Frequency = Once

        //sRequest += Server.UrlEncode("|METHOD~ProcessTranx|CC~" + Request.Form["credit"] + "|EXPMNTH~" + Request.Form["expMonth"] + "|EXPYR~" + Request.Form["expYear"] +
        //"|CSC~" + Request.Form["cvv"]);
        //sRequest += "|CUSTID~" + createCustomer(wClient, sPayTraceURL, sRequest);
        //sRequest += "|METHOD~ProcessTranx";
        //oneTime(wClient, sPayTraceURL, sRequest, false);

        public string METHOD { get; set; }              // METHOD: ProcessTranx, ProcessCheck, CreateRecur, CreateCustomer
        public string CC { get; set; }                  // Credit Card Number
        public string EXPMNTH { get; set; }             // Expiration Month
        public string EXPYR { get; set; }               // Expiration Year
        public string CSC { get; set; }                 // Security Code CVCC/CSC

        public string CUSTID { get; set; }              // CustomerID, internal, create one if new base on email
        public string SWIPE { get; set; }               // Not used
        // Frequency = Check

        //sRequest += Server.UrlEncode("|CHECKTYPE~Sale|DDA~" +
        //    Request.Form["dda"] + "|TR~" + Request.Form["tr"]);
        //sRequest += "|CUSTID~" + createCustomer(wClient, sPayTraceURL, sRequest);
        //sRequest += "|METHOD~ProcessCheck";
        //oneTime(wClient, sPayTraceURL, sRequest, true);

        public string DDA { get; set; }                 // DDA (for check)
        public string TR { get; set; }                  // TR  (for check) 

        // Frequency = Weekly (recurring)

        // sRequest += Server.UrlEncode("|FREQUENCY~5|START~" + Request.Form["startMonth"] +
        // "/" + Request.Form["startDay"] + "/" + Request.Form["startYear"] + "|TOTALCOUNT~" + Request.Form["totalCount"] + "|CC~" + Request.Form["credit"] +
        // "|CSC~" + Request.Form["cvv"] + "|EXPMNTH~" + Request.Form["expMonth"] + "|EXPYR~" + Request.Form["expYear"]);

        //sRequest += Server.UrlEncode("|CUSTID~" + createCustomer(wClient, sPayTraceURL, sRequest));
        //sRequest += Server.UrlEncode("|METHOD~CreateRecur");
        //recurring(wClient, sPayTraceURL, sRequest, "W");

        public string FREQUENCY { get; set; }              // for recurring: 5 = Weekly, 3 = Monthly, 1 = Annual
        public string START { get; set; }                  // Start Date MM/DD/YY
        public string TOTALCOUNT { get; set; }             // Total Count
        public string NEXT { get; set; }                   // Start Date MM/DD/Y for updating

        // Frequency = Monthly (recurring)

        //sRequest += Server.UrlEncode("|FREQUENCY~3|START~" + Request.Form["startMonth"] +
        //    "/" + Request.Form["startDay"] + "/" + Request.Form["startYear"] + "|TOTALCOUNT~" + Request.Form["totalCount"] + "|CC~" + Request.Form["credit"] +
        //    "|CSC~" + Request.Form["cvv"] + "|EXPMNTH~" + Request.Form["expMonth"] + "|EXPYR~" + Request.Form["expYear"]);

        //sRequest += Server.UrlEncode("|CUSTID~" + createCustomer(wClient, sPayTraceURL, sRequest));
        //sRequest += Server.UrlEncode("|METHOD~CreateRecur");
        //recurring(wClient, sPayTraceURL, sRequest, "M");

        // Frequency = Annual (recurring)

        //sRequest += Server.UrlEncode("|FREQUENCY~1|START~" + Request.Form["startMonth"] +
        //"/" + Request.Form["startDay"] + "/" + Request.Form["startYear"] + "|TOTALCOUNT~" + Request.Form["totalCount"] + "|CC~" + Request.Form["credit"] +
        //"|CSC~" + Request.Form["cvv"] + "|EXPMNTH~" + Request.Form["expMonth"] + "|EXPYR~" + Request.Form["expYear"]);

        //sRequest += Server.UrlEncode("|CUSTID~" + createCustomer(wClient, sPayTraceURL, sRequest));
        //sRequest += Server.UrlEncode("|METHOD~CreateRecur");
        //recurring(wClient, sPayTraceURL, sRequest, "A");

        public string SDATE { get; set; }             // Start Date MM/DD/YY
        public string EDATE { get; set; }             // End Date MM/DD/YY

        public string RECURID { get; set; }             // For pulling transactions based on RECURID
        public string CUSTRECEIPT { get; set; }         // for creating recur

        public string BuildStringForNonNulls()
        {
            // todo: loop on all properties and build the string
            return "";
        }

        public string BuildStringFor(string Names)
        {
            string r = "";

            if (Names.Length > 0)
            {
                Type t = this.GetType();
                string[] name = Names.Split(',');
                for (int i = 0; i < name.Length; i++)
                {
                    PropertyInfo pin = t.GetProperty(name[i]);
                    if (pin != null)
                    {
                        if (r.Length > 0)
                        {
                            r += "|";
                        }
                        string vv = pin.GetValue(this, null) as string;
                        if (vv != null)
                        {
                            vv = vv.Replace("~", "").Replace("|", "");
                        }
                        r += String.Format("{0}~{1}", name[i], pin.GetValue(this, null));
                    }
                }
            }

            return PARMLIST + "=" + r;

        }

    }
}
