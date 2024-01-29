using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using CMS.DataEngine;

using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;
using Microsoft.Web.Services3.Security.X509;

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// Helper class for X509 standard
    /// </summary>
    public class X509Helper
    {
        #region "Variables"

        /// <summary>
        /// Hashtable of the X509 certificates indexed by keyIdentifier.
        /// </summary>
        private static readonly Hashtable mX509Certificates = new Hashtable();

        #endregion


        #region "X509 Authentication"

        /// <summary>
        /// Returns client token key.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetClientX509TokenKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSStagingServiceX509ClientBase64KeyId");
        }


        /// <summary>
        /// Returns server token key.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetServerX509TokenKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSStagingServiceX509ServerBase64KeyId");
        }


        /// <summary>
        /// Returns the security token for X509 authentication.
        /// </summary>
        /// <param name="keyIdentifier">Token key identifier</param>
        public static X509SecurityToken GetX509Token(string keyIdentifier)
        {
            // Get the token
            return RetrieveX509TokenFromStore(StoreLocation.LocalMachine, StoreName.My, keyIdentifier);
        }


        /// <summary>
        /// Retrieves the X509 security token from the store.
        /// </summary>
        /// <param name="storeLocation">Store location</param>
        /// <param name="storeName">Store name</param>
        /// <param name="keyIdentifier">Token key identifier</param>
        private static X509SecurityToken RetrieveX509TokenFromStore(StoreLocation storeLocation, StoreName storeName, string keyIdentifier)
        {
            string key = storeLocation + "|" + storeName + "|" + keyIdentifier;
            // Try to get the certificate from HashTable
            X509Certificate2 cert = (X509Certificate2)mX509Certificates[key];
            if (cert != null)
            {
                return new X509SecurityToken(cert);
            }

            // Find the certificate based on the base64 key identifier
            byte[] keyId = Convert.FromBase64String(keyIdentifier);
            X509Certificate2Collection certs = X509Util.FindCertificateByKeyIdentifier(keyId, storeLocation, storeName.ToString());
            if ((certs != null) && (certs.Count > 0))
            {
                mX509Certificates[key] = certs[0];
                return new X509SecurityToken(certs[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns true if given client token is valid.
        /// </summary>
        /// <param name="token">Token to check</param>
        /// <param name="siteName">Site name</param>
        public static bool CheckClientX509Token(X509SecurityToken token, string siteName)
        {
            // No token given
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }
            // No token key
            if (token.KeyIdentifier == null)
            {
                throw new ArgumentException("Missing token KeyIdentifier.");
            }

            return CompareArray(token.KeyIdentifier.Value, Convert.FromBase64String(GetClientX509TokenKey(siteName)));
        }


        /// <summary>
        /// Returns true if given server token is valid.
        /// </summary>
        /// <param name="token">Token to check</param>
        /// <param name="siteName">Site name</param>
        public static bool CheckServerX509Token(X509SecurityToken token, string siteName)
        {
            // No token given
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            // Get server token to compare
            string serverKeyId = GetServerX509TokenKey(siteName);
            X509SecurityToken serverToken = GetX509Token(serverKeyId);
            if (serverToken == null)
            {
                throw new Exception("Unable to obtain server security token ID '" + serverKeyId + "'.");
            }

            return CompareArray(token.KeyIdentifier.Value, serverToken.KeyIdentifier.Value);
        }


        /// <summary>
        /// This method checks if the incoming message has signed all the important message parts such as soap:Body, all the addressing headers and timestamp.
        /// </summary>
        /// <param name="context">Soap context to get the signing token for</param>
        /// <param name="security">Security context</param>
        /// <returns>The signing token</returns>
        public static SecurityToken GetSigningX509Token(SoapContext context, Security security)
        {
            foreach (ISecurityElement element in security.Elements)
            {
                if (element is MessageSignature)
                {
                    //  The context contains a Signature element. 
                    MessageSignature sign = (MessageSignature)element;
                    if (CheckSignature(context, security, sign))
                    {
                        return sign.SigningToken;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Checks the context signature of the message.
        /// </summary>
        public static bool CheckSignature(SoapContext context, Security security, MessageSignature signature)
        {
            //  Verify which parts of the message were actually signed.
            SignatureOptions actualOptions = signature.SignatureOptions;
            SignatureOptions expectedOptions = SignatureOptions.IncludeSoapBody;

            if ((security != null) && (security.Timestamp != null))
            {
                expectedOptions = expectedOptions | SignatureOptions.IncludeTimestamp;
            }

            //  The <Action> and <To> are required addressing elements.
            expectedOptions = expectedOptions | SignatureOptions.IncludeAction;
            expectedOptions = expectedOptions | SignatureOptions.IncludeTo;
            if ((context.Addressing.FaultTo != null) && (context.Addressing.FaultTo.TargetElement != null))
            {
                expectedOptions = expectedOptions | SignatureOptions.IncludeFaultTo;
            }
            if ((context.Addressing.From != null) && (context.Addressing.From.TargetElement != null))
            {
                expectedOptions = expectedOptions | SignatureOptions.IncludeFrom;
            }
            if ((context.Addressing.MessageID != null) && (context.Addressing.MessageID.TargetElement != null))
            {
                expectedOptions = expectedOptions | SignatureOptions.IncludeMessageId;
            }
            if ((context.Addressing.RelatesTo != null) && (context.Addressing.RelatesTo.TargetElement != null))
            {
                expectedOptions = expectedOptions | SignatureOptions.IncludeRelatesTo;
            }
            if ((context.Addressing.ReplyTo != null) && (context.Addressing.ReplyTo.TargetElement != null))
            {
                expectedOptions = expectedOptions | SignatureOptions.IncludeReplyTo;
            }
            //  Check if the all the expected options are the present.
            SignatureOptions options = expectedOptions & actualOptions;

            return (options == expectedOptions);
        }


        /// <summary>
        /// Vefies, if all the required message parts are present.
        /// </summary>
        /// <param name="context">Soap context</param>
        public static string VerifyMessageParts(SoapContext context)
        {
            // Body
            if (context.Envelope.Body == null)
            {
                return "The message must contain a soap:Body element";
            }
            // To
            if ((context.Addressing.To == null) || (context.Addressing.To.TargetElement == null))
            {
                return "The message must contain a wsa:To header";
            }
            // Action
            if ((context.Addressing.Action == null) || (context.Addressing.Action.TargetElement == null))
            {
                return "The message must contain a wsa:Action header";
            }
            // Message ID
            if ((context.Addressing.MessageID == null) || (context.Addressing.MessageID.TargetElement == null))
            {
                return "The message must contain a wsa:MessageID header";
            }

            // Message OK
            return "";
        }


        /// <summary>
        /// Compares two byte arrays, returns true, if they are equal.
        /// </summary>
        /// <param name="a">Array 1</param>
        /// <param name="b">Array 2</param>
        private static bool CompareArray(byte[] a, byte[] b)
        {
            if ((a != null) && (b != null) && (a.Length == b.Length))
            {
                // Check the content
                Int32 index = a.Length - 1;
                while (index > -1)
                {
                    if (a[index] != b[index])
                    {
                        // Byte different, not equal
                        return false;
                    }
                    index = index - 1;
                }
                // All bytes same, equal
                return true;
            }
            else if ((a == null) && (b == null))
            {
                // Both null, equal
                return true;
            }
            else
            {
                // Not equal
                return false;
            }
        }

        #endregion


        #region "Username authentication"

        /// <summary>
        /// Returns true if given client token is valid.
        /// </summary>
        /// <param name="token">Token to check</param>
        /// <param name="siteName">Site name</param>
        public static bool CheckUsernameToken(UsernameToken token, string siteName)
        {
            // No token given
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }
            return (token.PasswordOption != PasswordOption.SendPlainText);
        }

        #endregion
    }
}
