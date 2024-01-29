using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using CMS.Base;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Allows disabling license checks while calling methods
    /// </summary>
    public static class LicenseCheckDisabler
    {
        /// <summary>
        /// Executes the given method without license checks. The method must be signed with AllowDisableLicense attribute with a valid hash in order to proceed. If the method is missing the signature, or the hash is invalid, this call throws an exception.
        /// </summary>
        /// <param name="expression">Action to execute</param>
        public static void ExecuteWithoutLicenseCheck(Expression<Action> expression)
        {
            if (CanDisableLicenseCheck(expression))
            {
                // Correct signature, disable license check and execute the action
                using (new CMSActionContext
                {
                    CheckLicense = false
                })
                {
                    var action = expression.Compile();

                    action();
                }
            }
            else
            {
                var message = "The method is not properly signed. It cannot run without license check.";
                var method = GetMethodInfo(expression);
                if (method != null)
                {
                    message += " Method identifier: " + GetMethodIdentifier(method);
                }

                // Invalid signature
                throw new NotSupportedException(message);
            }
        }


        /// <summary>
        /// Returns true if the given action is allowed to disable the license check
        /// </summary>
        /// <param name="expression">Expression to check</param>
        private static bool CanDisableLicenseCheck(Expression<Action> expression)
        {
            var method = GetMethodInfo(expression);
            if (method != null)
            {
                if (CanDisableLicenseCheck(method))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Gets the method info from the given expression
        /// </summary>
        /// <param name="expression">Method expression</param>
        private static MethodInfo GetMethodInfo(Expression<Action> expression)
        {
            MethodInfo method = null;

            var mc = expression.Body as MethodCallExpression;
            if (mc != null)
            {
                method = mc.Method;
            }

            return method;
        }


        /// <summary>
        /// Returns true if the given method is allowed to disable the license check
        /// </summary>
        /// <param name="method">Method to check</param>
        public static bool CanDisableLicenseCheck(MethodInfo method)
        {
            var type = method.DeclaringType;
            if (type != null)
            {
                if (CanDisableLicenseCheck(type))
                {
                    return true;
                }
            }

            // Check method attributes
            var attrs = method.GetCustomAttributes(typeof(CanDisableLicenseCheckAttribute), false);
            if (attrs.Length > 0)
            {
                var methodId = GetMethodIdentifier(method);

                foreach (CanDisableLicenseCheckAttribute attr in attrs)
                {
                    // If the signature is valid, the method can disable license check
                    if (VerifySignature(methodId, attr.Signature))
                    {
                        return true;
                    }
                }
            }

            // Neither method nor class can disable the license check
            return false;
        }


        /// <summary>
        /// Returns true if the given type is allowed to disable the license check
        /// </summary>
        /// <param name="type">Type to check</param>
        public static bool CanDisableLicenseCheck(Type type)
        {
            // Check class attributes
            var attrs = type.GetCustomAttributes(typeof (CanDisableLicenseCheckAttribute), false);
            if (attrs.Length > 0)
            {
                var typeId = GetTypeIdentifier(type);

                foreach (CanDisableLicenseCheckAttribute attr in attrs)
                {
                    // If the signature is valid, the class can disable license check
                    if (VerifySignature(typeId, attr.Signature))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Gets a unique method signature
        /// </summary>
        /// <param name="method">Method</param>
        private static string GetMethodIdentifier(MethodInfo method)
        {
            var type = method.DeclaringType;

            return GetTypeIdentifier(type) + "." + method.Name;
        }


        /// <summary>
        /// Gets a unique type identifier
        /// </summary>
        /// <param name="type">Type</param>
        private static string GetTypeIdentifier(Type type)
        {
            var assemblyName = (type != null) ? type.Assembly.GetName().Name : "";
            var typeName = (type != null) ? type.FullName : "";

            return String.Format("{0}, {1}", assemblyName, typeName);
        }


        /// <summary>
        /// Verifies that a signature for the content matches
        /// </summary>
        /// <param name="content">Signed content</param>
        /// <param name="signature">Signature to verify</param>
        private static bool VerifySignature(string content, string signature)
        {
            const string publicKeyXml = "<RSAKeyValue><Modulus>ovEkg+K5kvdgVdwjMulr5w6yCBVTD5E+cr3RzYdUxHDh49qjdVYyXhXqC6J30CTE3ixDERK6JYylgXGhvoP9hQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            using (var provider = new RSACryptoServiceProvider(512))
            {
                provider.FromXmlString(publicKeyXml);

                var data = Encoding.UTF8.GetBytes(content);
                var signedData = Convert.FromBase64String(signature);

                return provider.VerifyData(data, new SHA1CryptoServiceProvider(), signedData);
            }
        }
    }
}