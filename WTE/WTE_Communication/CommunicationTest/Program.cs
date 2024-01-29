using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WTE.Communication;

namespace CommunicationTest
{
    public class CommunicationTest
    {
        public class myResponseHandler : WTE.Communication.AsynchronousResponseHandler
        {
            public void HandleResponse(CommunicationResponse p_response)
            {
                if (p_response.GetType() == typeof(EmailResponse))
                {
                    EmailResponse emailResponse = (EmailResponse)p_response;
                    Console.WriteLine("Email handleresponse: " + emailResponse.Code + " " + emailResponse.Message);
                }
                else if (p_response.GetType() == typeof(FaxResponse))
                {
                    FaxResponse faxResponse = (FaxResponse)p_response;
                    Console.WriteLine("Fax handleresponse: " + faxResponse.Code + " " + faxResponse.Message);
                }
                else if (p_response.GetType() == typeof(SMSResponse))
                {
                    SMSResponse smsResponse = (SMSResponse)p_response;
                    Console.WriteLine("SMS handleresponse: " + smsResponse.Code + " " + smsResponse.Message);
                }
            }
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("main started");

            ConnectionInfo ci = new ConnectionInfo("http://10.100.0.244:8082/CommunicationWebService.asmx", "admin", "password");
            //ConnectionInfo ci = new ConnectionInfo("http://localhost:2464/CommunicationWebService.asmx", "admin", "password");

            Address toAddress = new Address();
            Console.WriteLine("enter to address:");
            toAddress.EMail = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(toAddress.EMail))
            {
                toAddress.EMail = "pateep@wte.net";
            }

            Address fromAddress = new Address();
            Console.WriteLine("enter from address:");
            fromAddress.EMail = Console.ReadLine();
            fromAddress.Name = "Joe Tester"; //this is how you would set the friendly email address name
            if (string.IsNullOrWhiteSpace(fromAddress.EMail))
            {
                fromAddress.EMail = "pateep@wte.net";
            }

            Address replyToAddress = new Address();
            Console.WriteLine("enter replyTo address:");
            replyToAddress.EMail = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(replyToAddress.EMail))
            {
                replyToAddress.EMail = "replyto@wte.net";
            }

            Console.WriteLine("enter SMS number:");
            string smsNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(smsNumber))
            {
                smsNumber = "9544222327";
            }

            //Console.WriteLine("enter FAX number:");
            //string faxNumber = Console.ReadLine();
            //if (string.IsNullOrWhiteSpace(faxNumber))
            //{
            //    faxNumber = "15613748772";
            //}

            bool runEmail = true, runFax = false, runSMS = false;

            int count = 0, numToQueue = 1;
            while (count < numToQueue)
            {
                //create attachements
                List<Attachment> attachments = new List<Attachment>();
                Attachment attachment = new Attachment();
                attachment.Data = File.ReadAllBytes(@"..\..\attachments\apple.jpg");
                attachment.Filename = "apple.jpg";
                attachment.Name = "applename.jpg";
                attachments.Add(attachment);

                attachment = new Attachment();
                attachment.Data = File.ReadAllBytes(@"..\..\attachments\test.txt");
                attachment.Filename = "test.txt";
                attachment.Name = "testname.txt";
                attachments.Add(attachment);

                //set up emails
                Email.Addresses addresses = new Email.Addresses();
                addresses.ToAddresses = new Address[1] { toAddress };
                addresses.FromAddress = fromAddress;

                addresses.ReplyToAddress = replyToAddress;

                bool sendHtmlEmail = true;

                TemplateEvaluator template = new TemplateEvaluator();
                Dictionary<string, object> fields = new Dictionary<string, object>();
                fields.Add("from", fromAddress.EMail);
                fields.Add("to", toAddress.EMail);
                StringReader sr = new StringReader("<html><body><h1>Communication test template.</h1><br /> Email sent <br />from: {{from}} <br />to: {{to}}.<br /></body></html>");
                template.Load(sr);
                string body = template.Eval(TemplateFieldAccessors.DictionaryAccessor(fields));

                if (1 == 0)
                {
                    //synchronous (blocking) email send
                    Communication.SendEmail(ci,
                       addresses,
                       "test subject", body, sendHtmlEmail, attachments.ToArray());
                }

                //non-blocking email send
                if (runEmail)
                {
                    Communication.SendAsynchronousEmail(ci,
                    addresses,
                    "test subject", body, sendHtmlEmail, attachments.ToArray(), new myResponseHandler());
                }

                if (runSMS)
                {
                    Communication.SendAsynchronousSMS(ci, smsNumber, "communication test message: " + count, new myResponseHandler());
                }

                //if (runFax)
                //{
                //    Communication.SendAsynchronousFax(ci, faxNumber, "communication test", "<html><body><b>this is a communication test fax:" + count + "</b></body></html>", Communication.FaxFormat.HTML, new myResponseHandler());
                //}

                Thread.Sleep(1000);

                count++;
            }

            while (Communication.CommunicationQueueHandler.CommunicationQueue.Count > 0)
            {
                Thread.Sleep(3000);
            }

            Communication.CommunicationQueueHandler.Enabled = false;

            Console.WriteLine("hit <enter> key to exit.");
            Console.ReadKey();
        }
    }
}