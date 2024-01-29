using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PaymentProcessor.Web.Applications;
using System.Data.SqlTypes;
using System.Data;

namespace Egiving
{
    public partial class Default : BasePage
    {

        protected DRspTA_DirectTransactions_GetByUserGUID myDirectTransactions;
        protected DRspTA_CheckTransactions_GetByUserGUID myCheckTransactions;
        protected DRspTA_RecurringTransactions_GetByUserGUID myRecurringTransactions;

        public struct Columns
        {
            public const string Status = "Status";
            public const string Card = "Card";
            public const string TR1 = "TR1";
        }

        protected void HowToCreateUpdateDeleteCustomer()
        {
            PayTraceServices p = new PayTraceServices(0);

            //// this is how we Created Customer, Update Customer and delete Customer. There is no View Customer other than export
            //Guid UserGUID = new Guid("6415B8CE-8072-4BCD-8E48-9D7178B826B7");
            //string CUSTID = UserGUID.ToString();
            //PayTraceResponseData r1 = p.CreateCustomer(CUSTID, "Chacha Nugroho", "102 York ST", "Morganton", "NC", "28655", "Chacha Nugroho", "2907 Alderman Ln", "Durham", "NC", "27705", "4012881888818888", "12", "12", "chacha@dejava.com");
            //if (r1.IsSuccess)
            //{
            //    // OK
            //}

            //PayTraceResponseData r2 = p.UpdateCustomer(CUSTID, "Chacha Nugroho", "102 York ST", "Morganton", "NC", "28655", "Fransiska Meteray", "2907 Alderman Ln", "Durham", "NC", "27705", "4012881888818888", "12", "12", "chacha@dejava.com");
            //if (r2.IsSuccess)
            //{
            //    // OK
            //}

            //PayTraceResponseData r3 = p.DeleteCustomer(CUSTID);
            //if (r3.IsSuccess)
            //{
            //    // OK
            //}
        }

        protected void HowToExportCustomers()
        {
            PayTraceServices p = new PayTraceServices(0);

            // this is how we export customer to an array
            PayTraceCustomerDataArray darr = p.ExportCustomer();

            // this is how we save the export to our SQL (temp table to look at)
            SQLDataPayTrace.TA_PayTraceCustomerExport_BulkInsert(darr);
        }


        protected void HowToProcessTransaction()
        {
            PayTraceServices p = new PayTraceServices(0);
            SqlGuid sq = new SqlGuid("7471505D-7480-46FE-A3DB-55DFF8362B74"); // get from CMS_User Table
            // XX: p.ProcessTransactionByUser(sq, "Chacha DeJava", "4012881888818888", "12", "2016", "17.00", "999", "1234", "Durham", "NC", "83852");
            p.ProcessDirectTransaction(sq, 1, "Chacha DeJava", "4012881888818888", "12", "2016", "17.00", "999", "1234", "Durham", "NC", "83852", "", "", "", "");

        }


        private string DeleteLeadingNumber(string s)
        {
            int t1 = s.IndexOf('.');
            if (t1 > -1)
            {
                int cn = 0;
                if (int.TryParse(s.Substring(0, t1), out cn))
                {
                    s = s.Substring(t1 + 1).Trim();
                }
            }

            return s;
        }

        private static string CCMarking(string CC)
        {
            string MCC = "";
            if (CC.Length > 4)
            {
                MCC = new String('*', CC.Length - 4) + CC.Substring(CC.Length - 4, 4);
            }
            else
            {
                MCC = CC;
            }
            return MCC;
        }

        protected void Page_Load1(object sender, EventArgs e)
        {
            PayTraceServices p1 = new PayTraceServices(24);
            PayTraceCustomerDataArray darr = p1.ExportCustomer();
            SQLDataPayTrace.TA_PayTraceCustomerExport_BulkInsert(darr);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            PayTraceServices p = new PayTraceServices(0);
            //HowToCreateUpdateDeleteCustomer();
            //HowToExportCustomers();
            //HowToProcessTransaction();
            //p.ExportTransactions();

            //p.ExportTransactions();
            //string[] hhh = ("614ee780-6013-45d5-a4f0-d8556450fc1c,95619e04-48c8-4ddc-a4ed-9cbe9040ae18,P2P1111_8,keoniHNB,532011533,sdb7440127-2cdc-4320-9cdf-9a89d13673b0,MSP Demo Customer ,sd2f1445df-5650-434f-91f4-510259832751,sd19407b47-492f-4553-aff1-a4f1e2a55ca3,sd1354ffcd-6fae-4a41-be24-b429bb7a307b,sd68ffffb4-9701-434f-a577-e75611bd312d,sdf3fa3b30-3542-4789-a127-169645528178,sd15099b61-21c8-4a99-816b-d88ac606739b,sd50f054ca-e5d4-41b1-a7f6-132126211e57,11110817,11110815,11110814,11110812,P2P1111_6,P2P111_6,P2P11_6,P2P1_6,post111a11_6,post111a111_6,post111a1_6,post111a_6,post1_6,Glenn Jackson,sdb6a72b79-a6c0-4f0a-b552-d79f9b7750e7,sdf7469734-d345-478a-b7a1-1c041753403b,sd4e7a3c00-28ce-4ce6-94f9-312325914cfc,532007089,532007087,532007086,532012473,532007085,532007084,532007082,532007081,532007080,532007079,532007078,532012472,532012471,532012470,532012469,532012468,532012467,532012466,11087939,11087937,11087936,11087935,11087923,11087922,11087921,11087920,532007077,11087907,11087905,11087900,11087899,532007076,11087865,11087864,11087862,11087861,11087842,11087841,11087840,11087839,11087825,11087824,11087823,11087822,11087806,11087805,11087804,11087803,532007075,11087773,11087772,11087771,11087770,532007074,532007073,11087695,11087679,11087678,11087677,11087676,11087670,cust_218570_fe395257,43E804E5-E4CC-4254-A81E-1A85FB59BCA6,2550milwau2576,6288raj6389,6287raj6388,cust_192998_eddff3fa,c_65878_798,2016090918201300000000,2016090918192200000000,101,2016090916031200000000,2016090915294200000000,2016090914272800000000,cust_206204_2fc88d17,5409214_26964,cust_182772_d66c3746,cust_217001_72c5e802,cust_222165_322b3642,11598917,Albequerque VA Brown,Denver VA Brown ,Denver VA Smith,c_65874_796,application1,600001^2222^20160907084102PM,ACC_EDC_280,6284aef6373,2548rtgr2574,tre-347a5545-c340-48a8-a5fc-cd9cf971ddca,tre-b2371a8e-daa3-43d2-9f6c-03f91001d9db,2546hip2572,2545mahi2571,2544uio2570,cust_202856_c47dfca5,cust_206205_c4de2987,cust_220286_7292fa81,cust_206206_c3649a1f,11613024,11613023,11613022,11613021,11613020,11613019,11613018,11613017,11613015,11613014,11613012,11613010,11613006,5299527_26929,cust_220283_2dd5e69e,6283fal6372,6282jimmi6371,6281raj6370,6280ttttt6369,6279raj6368,6278yus6367,6277shool6366,6276hud6365,6275raj6364,6274yyyy6363,6273asj6362,3624shoyab3709,cust_202856_e93d11dd,6272raj6361,6271raj6360,cust_220344_f8fa18b1,6270raj6359,57ce5cdc5fe37-6,cust_220342_14275355,11576156,cust_182772_31c70b54,11576086,11576085,11576083,11576082,11576081,11576080,11576079,11576078,11576074,11576073,11576072,11576071,11576068,cust_220341_0989ba80,cust_220283_63db5250,cust_202856_959cdf9f,11564942,11564940,11564939,11564938,11564937,11564936,11564935,11564934,11564931,11564930,11564928,11564926,11564923,CUST_0caaa7512b2fa061088e7be6fe5991c1,CUST_6b6bb700c8b7a1e4e47c9082f9929006,CUST_532df3d814e7e0f911eb09296b86076f,CUST_03fa9c56342b9ddc06c0bb5a8eac6ade,CUST_70329b41e24cea6bf782c308758f543b,CUST_89bb5bb81d9455caf7a2e0f1f6d2ce7c,CUST_4208994a72f3501566906aadc5253943,CUST_8e0d2ccf503f91fdffd319baa8b0e005,CUST_2a444c719c6972912bb2a0041480f5fb,CUST_fa9afe2852909092183f386b4cfe6215,CUST_036e3bade8184808ebf88d5770127b36,CUST_58bc4e973e5f2bf9df43dc6f61324d2b,23048023948-2094 tomas,CUST_2147b9007ead46adb03c8d1714d361bf,CUST_eea705e831129223982a0dbe883459c6,987654,Skylands V1234,2503shoyab2528,cust_220257_791892bf,11553085,11553084,11553083,11553082,11553081,11553080,11553079,11553078,11553076,11553075,11553074,11553073,11553070,11553069,11553068,11553067,11553066,11553065,11553064,11553063,11553062,11553060,11553059,11553057,11553056,11553054,11553007,11553006,11553005,11553004,11553003,11553002,11553001,11553000,11552998,11552997,11552994,11552993,11552991,11552981,11552974,11552968,11552953,11552942,11552941,11552940,11552939,11552918,11552917,11552915,11552914,c_65877_797,11513415,11513414,11513413,11513412,11513162,11513160,11513159,11513158,thawkinsps@gmail.com,demo123,klongstreet,11373680,11373679,11373678,11373677,9999").Split(',');
            //for (int i = 0; i < hhh.Length; i++)
            //{
            //    p.ExportRecurringTransactions(hhh[i]);
            //}
            //SqlGuid sq = new SqlGuid("7471505D-7480-46FE-A3DB-55DFF8362B74"); // get from CMS_User Table
            //p.ProcessCheckTransaction(sq, "Chacha Hohoho", "123456", "325070760", "90.12", "1234", "83852");

            ////p.ProcessTransactionByUser(sq, "Chacha DeJava", "4012881888818888", "12", "2016", "17.00", "999", "1234", "Durham", "NC", "83852");
            ////
            ////p.CreateRecurringByUser(sq, new DateTime(2016, 9, 19), "5", PayTraceServices.FREQUENCY.BiMonthly, "Chacha DeJava", "4012881888818888", "12", "2016", "17.00", "999", "1234", "Durham", "NC", "83852");
            ////PayTraceRecurringPaymentDataArray p1 = p.ExportRecurringTransactions(sq.ToString());
            ////p.DeleteRecurringTransaction("731438");
            ////p.ProcessDirectTransaction(sq, "Chacha DeJava", "4012881888818888", "12", "2016", "17.00", "999", "1234", "Durham", "NC", "83852");
            ////p.CreateRecurringTransaction(sq.ToString());
            ////p.ExportRecurringTransactions(sq.ToString());
            ////p.ExportRecurringTransactionsByRecurID("11552981");

            SqlGuid userguid;

                // if not logged in use Chacha's UserGUID 
                userguid = new SqlGuid("7471505D-7480-46FE-A3DB-55DFF8362B74");

                p.RefundTransaction("130671441");

            //     myCheckTransactions = SQLDataPayTrace.spTA_CheckTransactions_GetByUserGUID(userguid, 24);



            //     Repeater1.DataSource = null;
            //     Repeater2.DataSource = null;

            //     myDirectTransactions = SQLDataPayTrace.spTA_DirectTransactions_GetByUserGUID(userguid, 24);
            //     if (myDirectTransactions.Count > 0)
            //     {
            //         DataTable dt = myDirectTransactions.DataSource;
            //         dt.Columns.Add(Columns.Status);
            //         dt.Columns.Add(Columns.Card);
            //         for (int i = 0; i < myDirectTransactions.Count; i++)
            //         {
            //             PayTraceResponseData rd = new PayTraceResponseData(myDirectTransactions.ResponseString(i));
            //             dt.Rows[i][Columns.Status] = rd.RESPONSE == null ? "" : DeleteLeadingNumber(rd.RESPONSE);
            //             dt.Rows[i][Columns.Card] = "***" + myDirectTransactions.CC(i).Replace("*", "");
            //         }
            //         Repeater1.DataSource = dt;
            //     }

            //     myCheckTransactions = SQLDataPayTrace.spTA_CheckTransactions_GetByUserGUID(userguid, 24);
            //     if (myCheckTransactions.Count > 0)
            //     {
            //         DataTable dt1 = myCheckTransactions.DataSource;
            //         dt1.Columns.Add(Columns.Status);
            //         dt1.Columns.Add(Columns.TR1);
            //         for (int i = 0; i < myCheckTransactions.Count; i++)
            //         {
            //             PayTraceResponseData rd = new PayTraceResponseData(myCheckTransactions.ResponseString(i));
            //             dt1.Rows[i][Columns.Status] = rd.RESPONSE == null ? "" : DeleteLeadingNumber(rd.RESPONSE);
            //             dt1.Rows[i][Columns.TR1] = "***" + CCMarking(myCheckTransactions.TR(i)).Replace("*", "");
            //         }
            //         Repeater2.DataSource = dt1;
            //     }

            //     // we update last Next Payment Dates iflast check higher than 1 day
            //     p.UpdateRecurringNextPaymentByUserGUID(userguid, 1); 


            //     myRecurringTransactions = SQLDataPayTrace.spTA_RecurringTransactions_GetByUserGUID(userguid, 24);
            //     if (myRecurringTransactions.Count > 0)
            //     {
            //         DataTable dt3 = myRecurringTransactions.DataSource;
            //         dt3.Columns.Add(Columns.Status);
            //         dt3.Columns.Add(Columns.Card);
            //         for (int i = 0; i < myRecurringTransactions.Count; i++)
            //         {
            //             PayTraceResponseData rd = new PayTraceResponseData(myRecurringTransactions.ResponseString(i));
            //             dt3.Rows[i][Columns.Status] = rd.RESPONSE == null ? "" :  DeleteLeadingNumber(rd.RESPONSE);
            //             dt3.Rows[i][Columns.Card] = "***" + myRecurringTransactions.CC(i).Replace("*", "");
            //         }
            //         Repeater3.DataSource = dt3;
            //         Repeater3.Visible = true;
            //     }


            //     Repeater1.DataBind();
            //     Repeater2.DataBind();
            //     Repeater3.DataBind();



            //    int DirectTransactionID = 5;
            //if (DirectTransactionID > 0)
            //{
            //    DRspTA_DirectTransactions_GetForThanks getDT = SQLDataPayTrace.spTA_DirectTransactions_GetForThanks(userguid, 24, DirectTransactionID);
            //    if (getDT.Count > 0)
            //    {
            //        getDT.ResponseString(0);
            //    }
            //}

        }
    }
}