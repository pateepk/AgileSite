using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Contains method to set DACL permissions for a service.
    /// </summary>
    internal static class DiscretionaryAclHelper
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool QueryServiceObjectSecurity(SafeHandle serviceHandle,
            SecurityInfos secInfo,
            byte[] lpSecDesrBuf,
            uint bufSize,
            out uint bufSizeNeeded);


        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool SetServiceObjectSecurity(SafeHandle serviceHandle, SecurityInfos secInfos, byte[] lpSecDesrBuf);


        /// <summary>
        /// Sets DALC permissions for given <parameter name="serviceName"/> running under specified <paramref name="accountName"/>.
        /// </summary>
        /// <param name="serviceName">Service for which DACL permissions should be set.</param>
        /// <param name="accountName">Name of an account for which DACL permissions should be set.</param>
        public static void SetServicePermissions(string serviceName, string accountName)
        {
            ServiceController serviceController = new ServiceController(serviceName, ".");
            byte[] psd = new byte[0];
            bool result = QueryServiceObjectSecurity(serviceController.ServiceHandle, SecurityInfos.DiscretionaryAcl, psd, 0, out uint bufSizeNeeded);
            if (!result)
            {
                int err = Marshal.GetLastWin32Error();
                if (err == 122 || err == 0) // ERROR_INSUFFICIENT_BUFFER
                {
                    psd = new byte[bufSizeNeeded];
                    result = QueryServiceObjectSecurity(serviceController.ServiceHandle, SecurityInfos.DiscretionaryAcl, psd, bufSizeNeeded, out bufSizeNeeded);
                }
                else
                {
                    throw new ApplicationException($"Error calling QueryServiceObjectSecurity() to get DACL for {serviceName}: error code={err}");
                }
            }

            if (!result)
            {
                throw new ApplicationException($"Error calling QueryServiceObjectSecurity(2) to get DACL for {serviceName}: error code={Marshal.GetLastWin32Error()}");
            }

            // Get security descriptor via raw into DACL form so ACE
            RawSecurityDescriptor rawDescriptor = new RawSecurityDescriptor(psd, 0);
            RawAcl rawAcl = rawDescriptor.DiscretionaryAcl;
            DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, rawAcl);

            NTAccount ntAccount = new NTAccount(accountName);
            SecurityIdentifier sid = (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));

            // https://docs.microsoft.com/en-us/windows/desktop/services/service-security-and-access-rights
            const int accessMask = 0x30; // SERVICE_START |SERVICE_STOP
            dacl.AddAccess(AccessControlType.Allow, sid, accessMask, InheritanceFlags.None, PropagationFlags.None);

            // Convert discretionary ACL back to raw form
            byte[] rawDacl = new byte[dacl.BinaryLength];
            dacl.GetBinaryForm(rawDacl, 0);
            rawDescriptor.DiscretionaryAcl = new RawAcl(rawDacl, 0);

            // Set raw security descriptor on service again
            byte[] rawDescriptorBinaryForm = new byte[rawDescriptor.BinaryLength];
            rawDescriptor.GetBinaryForm(rawDescriptorBinaryForm, 0);

            result = SetServiceObjectSecurity(serviceController.ServiceHandle, SecurityInfos.DiscretionaryAcl, rawDescriptorBinaryForm);
            if (!result)
            {
                throw new ApplicationException("Error calling SetServiceObjectSecurity(); error code=" + Marshal.GetLastWin32Error());
            }
        }
    }
}
