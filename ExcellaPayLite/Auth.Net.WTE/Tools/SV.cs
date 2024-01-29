using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ssch.tools
{
    public enum moveTypeIDs
    {
           CopyFolder = 1
         , AttachingFile = 2
         , InitialSetup = 3
    }

    public static class SV
    {

        public static class SP
        {
            public const string spStoredProcedures_GetAll = "spStoredProcedures_GetAll";
            public const string spFunctions_GetAll = "spFunctions_GetAll";
            public const string spTables_GetAll = "spTables_GetAll";
            public const string sp_help = "sp_help"; // internal SQL
            public const string sp_helptext = "sp_helptext"; // internal SQL

            public const string spScannedDocument_Insert = "spScannedDocument_Insert";
            public const string spScannedDocument_Update = "spScannedDocument_Update";
            public const string spScannedDocument_Get = "spScannedDocument_Get";
            public const string spScannedDocument_GetRecipientID = "spScannedDocument_GetRecipientID";
            public const string spRecipient_GetByID = "spRecipient_GetByID";
            public const string spIncomingDocument_Insert = "spIncomingDocument_Insert";
            public const string spIncomingDocument_UpdateCurrentPath = "spIncomingDocument_UpdateCurrentPath";
            public const string spScannedDocument_GetNoRecipient = "spScannedDocument_GetNoRecipient";
        }

        // copied from Web App
        public static class UDriveFolders
        {
            public const string ClaimSummaryType = "ClaimSummary";                  // folder in the UDrive for all Claim Summary PDF to Providers
            public const string RespiteHoursSummaryType = "RespiteHoursSummary";    // folder in the UDrive for all Respite Hours PDF
            public const string CaseManagerSummaryType = "CaseManagerSummary";      // folder in the UDrive for all Case Manager Claim Summary PDF to Providers
            public const string MissingInfoType = "MissingInfo";                    // folder in the UDrive for all Missing Info PDF to Providers
            public const string IncomingDocuments = "IncomingDocuments";            // folder in the UDrive for all incoming pdf/documents before we associated to database
            public const string RecipientDocuments = "RecipientDocuments";          // folder in the UDrive for all incoming pdf/documents after we associated to database
            public const string RecycleBin = "RecycleBin";
        }

        public static class FileExtensions
        {
            public const string pdf = ".pdf";
            public const string jpg = ".jpg";
            public const string jpeg = ".jpeg";
            public const string gif = ".gif";
            public const string png = ".png";
        }



    }
}
