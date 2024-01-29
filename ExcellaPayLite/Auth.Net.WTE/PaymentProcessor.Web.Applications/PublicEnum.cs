namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum HttpMethods
    {
        NA = 0,
        GET = 1,
        POST = 2
    }

    public enum environmentCodes
    {
        NA = 0,
        Localhost = 1,
        Development = 2,
        Staging = 3,
        Production = 4
    }

    public enum appSettingData
    {
        NA = 0,
        Application = 1,
        ConnectionString = 2,
        Links = 3,
        Users = 4
    }

    public enum reportTypeIDs
    {
        Simple = 1,
        Custom = 2
    }

    public enum databaseServer
    {
        na = -1,
        DefaultDB = 0,
        EGivings = 1,
    }

    public enum requestTypeIDs
    {
        na = 0,
        CreateCustomer = 1,
        DeleteCustomer = 2,
        UpdateCustomer = 3,
        ExportCustomers = 4,
    }

    public enum roleIDs
    {
        Unknown = 0,
        User = 100,
        Administrator = 900,
        System = 950
    }


    public enum websiteIDs
    {
        PaymentProcessor = 1
    }

    // Table Activity in the DB
    public enum activityIDs
    {
        UserLoggedIn = 30,
        UserLoggedOut = 40,
        InvoicePDFnotfound = 340,

    }

    public enum invoiceItemStatusIDs
    {
        Created = 0,
        InTransit = 1,
        Paid = 2,
        Failed = 3
    }

    public enum invoiceStatusIDs
    {
        NA = -1,
        Open = 0,
        InTransit = 1,
        Paid = 2,
        Archive = 3,
        VirtualOpenAndInTransit = 4
    }

}
